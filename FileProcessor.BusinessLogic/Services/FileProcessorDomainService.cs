namespace FileProcessor.BusinessLogic.Services;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Common;
using EstateManagement.Client;
using EstateManagement.DataTransferObjects.Responses;
using EstateManagement.DataTransferObjects.Responses.Contract;
using EstateManagement.DataTransferObjects.Responses.Operator;
using FileAggregate;
using FileFormatHandlers;
using FileImportLogAggregate;
using FileProcessor.DataTransferObjects.Responses;
using FIleProcessor.Models;
using Managers;
using Newtonsoft.Json;
using Requests;
using SecurityService.Client;
using SecurityService.DataTransferObjects.Responses;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;
using Shared.Exceptions;
using Shared.General;
using Shared.Logger;
using TransactionProcessor.Client;
using TransactionProcessor.DataTransferObjects;
using FileDetails = FIleProcessor.Models.FileDetails;
using FileLine = FIleProcessor.Models.FileLine;
using MerchantResponse = EstateManagement.DataTransferObjects.Responses.Merchant.MerchantResponse;

public class FileProcessorDomainService : IFileProcessorDomainService
{
    private readonly IFileProcessorManager FileProcessorManager;

    private readonly IAggregateRepository<FileImportLogAggregate, DomainEvent> FileImportLogAggregateRepository;
        
    private readonly IAggregateRepository<FileAggregate, DomainEvent> FileAggregateRepository;
        
    private readonly ITransactionProcessorClient TransactionProcessorClient;
        
    private readonly IEstateClient EstateClient;
        
    private readonly ISecurityServiceClient SecurityServiceClient;
        
    private readonly Func<String, IFileFormatHandler> FileFormatHandlerResolver;
        
    private readonly IFileSystem FileSystem;

    public FileProcessorDomainService(IFileProcessorManager fileProcessorManager,
                                      IAggregateRepository<FileImportLogAggregate, DomainEvent> fileImportLogAggregateRepository,
                                      IAggregateRepository<FileAggregate, DomainEvent> fileAggregateRepository,
                                      ITransactionProcessorClient transactionProcessorClient,
                                      IEstateClient estateClient,
                                      ISecurityServiceClient securityServiceClient,
                                      Func<String, IFileFormatHandler> fileFormatHandlerResolver,
                                      IFileSystem fileSystem) {
        this.FileProcessorManager = fileProcessorManager;
        this.FileImportLogAggregateRepository = fileImportLogAggregateRepository;
        this.FileAggregateRepository = fileAggregateRepository;
        this.TransactionProcessorClient = transactionProcessorClient;
        this.EstateClient = estateClient;
        this.SecurityServiceClient = securityServiceClient;
        this.FileFormatHandlerResolver = fileFormatHandlerResolver;
        this.FileSystem = fileSystem;
    }

    public async Task<Guid> UploadFile(UploadFileRequest request,
                                       CancellationToken cancellationToken) {
        DateTime importLogDateTime = request.FileUploadedDateTime;

        this.ValidateRequest(request);

        // This will now create the import log and add an event for the file being uploaded
        Guid importLogId = Helpers.CalculateFileImportLogAggregateId(importLogDateTime.Date, request.EstateId);

        // Get the import log
        FileImportLogAggregate fileImportLogAggregate = await this.FileImportLogAggregateRepository.GetLatestVersion(importLogId, cancellationToken);

        if (fileImportLogAggregate.IsCreated == false)
        {
            // First file of the day so create
            fileImportLogAggregate.CreateImportLog(request.EstateId, importLogDateTime);
        }

        // Move the file
        FileProfile fileProfile = await this.FileProcessorManager.GetFileProfile(request.FileProfileId, cancellationToken);

        if (fileProfile == null)
        {
            throw new NotFoundException($"No file profile found with Id {request.FileProfileId}");
        }

        // Copy file from the temp location to file processing listening directory
        IFileInfo file = this.FileSystem.FileInfo.New(request.FilePath);
        if (file.Exists == false)
        {
            throw new FileNotFoundException($"File {file.FullName} not found");
        }
        String originalName = file.Name;

        if (this.FileSystem.Directory.Exists(fileProfile.ListeningDirectory) == false)
        {
            throw new DirectoryNotFoundException($"Directory {fileProfile.ListeningDirectory} not found");
        }

        // Read the file data
        String fileContent = null;
        //Open file for Read\Write
        using (Stream fs = file.Open(FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read))
        {
            //Create object of StreamReader by passing FileStream object on which it needs to operates on
            using (StreamReader sr = new StreamReader(fs))
            {
                //Use ReadToEnd method to read all the content from file
                fileContent = await sr.ReadToEndAsync();
            }
        }

        Guid fileId = this.CreateGuidFromFileData(fileContent);

        String fileDestination = $"{fileProfile.ListeningDirectory}//{request.EstateId:N}-{fileId:N}";
        file.MoveTo(fileDestination, overwrite: true);

        // Update Import log aggregate
        fileImportLogAggregate.AddImportedFile(fileId, request.MerchantId, request.UserId, request.FileProfileId, originalName, fileDestination, request.FileUploadedDateTime);

        // Save changes
        await this.FileImportLogAggregateRepository.SaveChanges(fileImportLogAggregate, cancellationToken);

        return fileId;
    }

    private void ValidateRequest(UploadFileRequest request){
        if (request.UserId == Guid.Empty){
            throw new InvalidDataException("No User Id provided with file upload");
        }

        if (request.MerchantId == Guid.Empty)
        {
            throw new InvalidDataException("No Merchant Id provided with file upload");
        }

        if (request.FileProfileId == Guid.Empty)
        {
            throw new InvalidDataException("No File Profile Id provided with file upload");
        }
    }

    public async Task ProcessUploadedFile(ProcessUploadedFileRequest request,
                                          CancellationToken cancellationToken) {
        // TODO: Should the file id be generated from the file uploaded to protect against duplicate files???
        FileAggregate fileAggregate = await this.FileAggregateRepository.GetLatestVersion(request.FileId, cancellationToken);

        Guid operatorId = await GetOperatorIdForFileProfile(request.EstateId, request.FileProfileId, cancellationToken);

        fileAggregate.CreateFile(request.FileImportLogId, request.EstateId, request.MerchantId, request.UserId, request.FileProfileId, request.FilePath, request.FileUploadedDateTime, operatorId);

        await this.FileAggregateRepository.SaveChanges(fileAggregate, cancellationToken);

        await this.ProcessFile(request.FileId, request.FileProfileId, request.FilePath, cancellationToken);
    }

    private async Task<Guid> GetOperatorIdForFileProfile(Guid estateId, Guid fileProfileId, CancellationToken cancellationToken){

        FileProfile fileProfile = await this.FileProcessorManager.GetFileProfile(fileProfileId, cancellationToken);

        if (fileProfile == null){
            Logger.LogInformation($"file profile {fileProfileId} not  found");
            throw new NotFoundException($"file profile {fileProfileId} not  found");
        }

        this.TokenResponse = await this.GetToken(cancellationToken);
        List<OperatorResponse> operatorList = await this.EstateClient.GetOperators(this.TokenResponse.AccessToken, estateId, cancellationToken);
        if (operatorList == null){
            throw new NotFoundException($"No operators returned from API Call");
        }

        OperatorResponse @operator = operatorList.SingleOrDefault(o => o.Name == fileProfile.OperatorName);
        if (@operator == null){
            throw new NotFoundException($"No operator record found with name [{fileProfile.OperatorName}]");
        }

        return @operator.OperatorId;
    }

    public async Task ProcessTransactionForFileLine(ProcessTransactionForFileLineRequest request,
                                                    CancellationToken cancellationToken) {
        // Get the file aggregate, this tells us the file profile information
        FileAggregate fileAggregate = await this.FileAggregateRepository.GetLatestVersion(request.FileId, cancellationToken);

        FileDetails fileDetails = fileAggregate.GetFile();

        if (fileDetails.FileLines.Any() == false)
        {
            throw new NotSupportedException($"File Id [{request.FileId}] has no lines added");
        }

        FileLine fileLine = fileDetails.FileLines.SingleOrDefault(f => f.LineNumber == request.LineNumber);

        if (fileLine == null)
        {
            throw new NotFoundException($"File Line Number {request.LineNumber} not found in File Id {request.FileId}");
        }

        if (fileLine.ProcessingResult != ProcessingResult.NotProcessed)
        {
            // Line already processed
            return;
        }

        FileProfile fileProfile = await this.FileProcessorManager.GetFileProfile(fileDetails.FileProfileId, cancellationToken);

        if (fileProfile == null)
        {
            throw new NotFoundException($"No file profile found with Id {fileDetails.FileProfileId}");
        }

        // Determine if we need to actually process this file line
        if (this.FileLineCanBeIgnored(fileLine.LineData, fileProfile.FileFormatHandler))
        {
            // Write something to aggregate to say line was explicity ignored
            fileAggregate.RecordFileLineAsIgnored(fileLine.LineNumber);
            await this.FileAggregateRepository.SaveChanges(fileAggregate, cancellationToken);
            return;
        }

        // need to now parse the line (based on the file format), this builds the metadata
        Dictionary<String, String> transactionMetadata = this.ParseFileLine(fileLine.LineData, fileProfile.FileFormatHandler);

        if (transactionMetadata == null)
        {
            // Line failed to parse so record this
            fileAggregate.RecordFileLineAsRejected(fileLine.LineNumber, "Invalid Format");
            await this.FileAggregateRepository.SaveChanges(fileAggregate, cancellationToken);
            return;
        }

        // Add the file data to the request metadata
        transactionMetadata.Add("FileId", request.FileId.ToString());
        transactionMetadata.Add("FileLineNumber", fileLine.LineNumber.ToString());

        String operatorName = fileProfile.OperatorName;
        if (transactionMetadata.ContainsKey("OperatorName"))
        {
            // extract the value
            operatorName = transactionMetadata["OperatorName"];
            transactionMetadata = transactionMetadata.Where(x => x.Key != "OperatorName").ToDictionary(x => x.Key, x => x.Value);
        }

        this.TokenResponse = await this.GetToken(cancellationToken);

        Interlocked.Increment(ref FileProcessorDomainService.TransactionNumber);

        // Get the merchant details
        MerchantResponse merchant = await this.EstateClient.GetMerchant(this.TokenResponse.AccessToken, fileDetails.EstateId, fileDetails.MerchantId, cancellationToken);
        if (merchant == null)
        {
            throw new NotFoundException($"Merchant not found with Id {fileDetails.MerchantId} on estate Id {fileDetails.EstateId}");
        }
        List<ContractResponse> contracts = await this.EstateClient.GetMerchantContracts(this.TokenResponse.AccessToken, fileDetails.EstateId, fileDetails.MerchantId, cancellationToken);

        if (contracts.Any() == false)
        {
            throw new NotFoundException($"No contracts found for Merchant Id {fileDetails.MerchantId} on estate Id {fileDetails.EstateId}");
        }

        ContractResponse? contract = null;
        if (fileProfile.OperatorName == "Voucher")
        {
            contract = contracts.SingleOrDefault(c => c.Description.Contains(operatorName));
        }
        else
        {
            contract = contracts.SingleOrDefault(c => c.OperatorName == operatorName);
        }

        if (contract == null)
        {
            throw new NotFoundException($"No merchant contract for operator Id {operatorName} found for Merchant Id {merchant.MerchantId}");
        }

        ContractProduct? product = contract.Products.SingleOrDefault(p => p.Value == null); // TODO: Is this enough or should the name be used and stored in file profile??

        if (product == null)
        {
            throw new NotFoundException($"No variable value product found on the merchant contract for operator Id {fileProfile.OperatorName} and Merchant Id {merchant.MerchantId}");
        }

        // Build a transaction request message
        SaleTransactionRequest saleTransactionRequest = new SaleTransactionRequest
                                                        {
                                                            EstateId = fileDetails.EstateId,
                                                            MerchantId = fileDetails.MerchantId,
                                                            TransactionDateTime = fileDetails.FileReceivedDateTime,
                                                            TransactionNumber = FileProcessorDomainService.TransactionNumber.ToString(),
                                                            TransactionType = "Sale",
                                                            ContractId = contract.ContractId,
                                                            DeviceIdentifier = merchant.Devices.First().Value,
                                                            OperatorId = contract.OperatorId,
                                                            ProductId = product.ProductId,
                                                            AdditionalTransactionMetadata = transactionMetadata,
                                                            TransactionSource = 2 // File based transaction
                                                        };

        SerialisedMessage serialisedRequestMessage = new SerialisedMessage
                                                     {
                                                         Metadata = new Dictionary<String, String>
                                                                    {
                                                                        {"estate_id", fileDetails.EstateId.ToString()},
                                                                        {"merchant_id", fileDetails.MerchantId.ToString()}
                                                                    },
                                                         SerialisedData = JsonConvert.SerializeObject(saleTransactionRequest, new JsonSerializerSettings
                                                                                                          {
                                                                                                              TypeNameHandling = TypeNameHandling.All
                                                                                                          })
                                                     };

        Logger.LogDebug(serialisedRequestMessage.SerialisedData);

        // Send request to transaction processor
        SerialisedMessage serialisedResponseMessage = await this.TransactionProcessorClient.PerformTransaction(this.TokenResponse.AccessToken, serialisedRequestMessage, cancellationToken);

        // Get the sale transaction response
        SaleTransactionResponse saleTransactionResponse = JsonConvert.DeserializeObject<SaleTransactionResponse>(serialisedResponseMessage.SerialisedData);

        if (saleTransactionResponse.ResponseCode == "0000")
        {
            // record response against file line in file aggregate
            fileAggregate.RecordFileLineAsSuccessful(request.LineNumber, saleTransactionResponse.TransactionId);
        }
        else
        {
            fileAggregate.RecordFileLineAsFailed(request.LineNumber, saleTransactionResponse.TransactionId, saleTransactionResponse.ResponseCode, saleTransactionResponse.ResponseMessage);
        }

        // Save changes to file aggregate
        // TODO: Add retry round this save (maybe 3 retries)
        await this.FileAggregateRepository.SaveChanges(fileAggregate, cancellationToken);
    }

    private async Task ProcessFile(Guid fileId,
                                   Guid fileProfileId,
                                   String fileName,
                                   CancellationToken cancellationToken)
    {
        IFileInfo inProgressFile = null;
        FileProfile fileProfile = null;
        try
        {
            fileProfile = await this.FileProcessorManager.GetFileProfile(fileProfileId, cancellationToken);

            if (fileProfile == null)
            {
                throw new NotFoundException($"No file profile found with Id {fileProfileId}");
            }

            // Check the processed/failed directories exist
            if (this.FileSystem.Directory.Exists(fileProfile.ProcessedDirectory) == false)
            {
                Logger.LogInformation($"Creating Directory {fileProfile.ProcessedDirectory} as not found");
                this.FileSystem.Directory.CreateDirectory(fileProfile.ProcessedDirectory);
            }

            if (this.FileSystem.Directory.Exists(fileProfile.FailedDirectory) == false)
            {
                Logger.LogInformation($"Creating Directory {fileProfile.FailedDirectory} as not found");
                this.FileSystem.Directory.CreateDirectory(fileProfile.FailedDirectory);
            }

            inProgressFile = this.FileSystem.FileInfo.New(fileName);

            if (inProgressFile.Exists == false)
            {
                // We also want to check the failed folder incase this is an event replay
                IFileInfo failedFileInfo = this.FileSystem.FileInfo.New($"{fileProfile.FailedDirectory}/{inProgressFile.Name}");

                if (failedFileInfo.Exists == false){
                    throw new FileNotFoundException($"File {inProgressFile.FullName} not found");
                }
                // Overwrite the inprogress file info object with the file found in the failed folder
                inProgressFile = failedFileInfo;
            }

            FileAggregate fileAggregate =
                await this.FileAggregateRepository.GetLatestVersion(fileId, cancellationToken);
            
            String fileContent = null;
            //Open file for Read\Write
            using (Stream fs = inProgressFile.Open(FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read))
            {
                //Create object of StreamReader by passing FileStream object on which it needs to operates on
                using (StreamReader sr = new StreamReader(fs))
                {
                    //Use ReadToEnd method to read all the content from file
                    fileContent = await sr.ReadToEndAsync(cancellationToken);
                }
            }

            if (String.IsNullOrEmpty(fileContent) == false)
            {
                String[] fileLines = fileContent.Split(fileProfile.LineTerminator);

                foreach (String fileLine in fileLines)
                {
                    fileAggregate.AddFileLine(fileLine.Trim());
                }

                await this.FileAggregateRepository.SaveChanges(fileAggregate, cancellationToken);
            }

            Logger.LogInformation($"About to move file {inProgressFile.Name} to [{fileProfile.ProcessedDirectory}]");

            // TODO: Move file now
            inProgressFile.MoveTo($"{fileProfile.ProcessedDirectory}/{inProgressFile.Name}");
        }
        catch (Exception e)
        {
            if (inProgressFile != null && fileProfile != null)
                inProgressFile.MoveTo($"{fileProfile.FailedDirectory}/{inProgressFile.Name}");

            Logger.LogError(e);
            throw;
        }
    }

    private static Int32 TransactionNumber = 0;

    private Guid CreateGuidFromFileData(String fileContents)
    {
        using (SHA256 sha256Hash = SHA256.Create())
        {
            //Generate hash from the key
            Byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(fileContents));

            Byte[] j = bytes.Skip(Math.Max(0, bytes.Count() - 16)).ToArray(); //Take last 16

            //Create our Guid.
            return new Guid(j);
        }
    }

    private Boolean FileLineCanBeIgnored(String domainEventFileLine,
                                         String fileProfileFileFormatHandler)
    {
        // Ignore empty files
        if (String.IsNullOrEmpty(domainEventFileLine))
            return true;

        IFileFormatHandler fileFormatHandler = this.FileFormatHandlerResolver(fileProfileFileFormatHandler);

        return fileFormatHandler.FileLineCanBeIgnored(domainEventFileLine);
    }

    private Dictionary<String, String> ParseFileLine(String domainEventFileLine,
                                                     String fileProfileFileFormatHandler)
    {
        try
        {
            IFileFormatHandler fileFormatHandler = this.FileFormatHandlerResolver(fileProfileFileFormatHandler);

            return fileFormatHandler.ParseFileLine(domainEventFileLine);
        }
        catch (InvalidDataException iex)
        {
            Logger.LogWarning(iex.Message);
            return null;
        }
    }

    private TokenResponse TokenResponse;

    private async Task<TokenResponse> GetToken(CancellationToken cancellationToken)
    {
        // Get a token to talk to the estate service
        String clientId = ConfigurationReader.GetValue("AppSettings", "ClientId");
        String clientSecret = ConfigurationReader.GetValue("AppSettings", "ClientSecret");

        if (this.TokenResponse == null)
        {
            TokenResponse token = await this.SecurityServiceClient.GetToken(clientId, clientSecret, cancellationToken);
            Logger.LogDebug($"Token is {token.AccessToken}");
            return token;
        }

        if (this.TokenResponse.Expires.UtcDateTime.Subtract(DateTime.UtcNow) < TimeSpan.FromMinutes(2))
        {
            Logger.LogDebug($"Token is about to expire at {this.TokenResponse.Expires.DateTime:O}");
            TokenResponse token = await this.SecurityServiceClient.GetToken(clientId, clientSecret, cancellationToken);
            Logger.LogDebug($"Token is {token.AccessToken}");
            return token;
        }

        return this.TokenResponse;
    }
}