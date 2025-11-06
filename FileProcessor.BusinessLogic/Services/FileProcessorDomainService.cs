using Shared.EventStore.Helpers;
using TransactionProcessor.DataTransferObjects.Responses.Merchant;

namespace FileProcessor.BusinessLogic.Services;

using System.Diagnostics.CodeAnalysis;
using Shared.Results;
using SimpleResults;
using TransactionProcessor.DataTransferObjects.Responses.Contract;
using TransactionProcessor.DataTransferObjects.Responses.Operator;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common;
using FileAggregate;
using FileFormatHandlers;
using FileImportLogAggregate;
using Models;
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
using FileDetails = Models.FileDetails;
using FileLine = Models.FileLine;

public interface IFileProcessorDomainService
{
    Task<Result<Guid>> UploadFile(FileCommands.UploadFileCommand command, CancellationToken cancellationToken);

    Task<Result> ProcessUploadedFile(FileCommands.ProcessUploadedFileCommand command,
                                     CancellationToken cancellationToken);

    Task<Result> ProcessTransactionForFileLine(FileCommands.ProcessTransactionForFileLineCommand command,
                                               CancellationToken cancellationToken);
}

public class FileProcessorDomainService : IFileProcessorDomainService
{
    private readonly IFileProcessorManager FileProcessorManager;

    private readonly IAggregateRepository<FileImportLogAggregate, DomainEvent> FileImportLogAggregateRepository;
        
    private readonly IAggregateRepository<FileAggregate, DomainEvent> FileAggregateRepository;
        
    private readonly ITransactionProcessorClient TransactionProcessorClient;
        
    private readonly ISecurityServiceClient SecurityServiceClient;
        
    private readonly Func<String, IFileFormatHandler> FileFormatHandlerResolver;
        
    private readonly IFileSystem FileSystem;

    public FileProcessorDomainService(IFileProcessorManager fileProcessorManager,
                                      IAggregateRepository<FileImportLogAggregate, DomainEvent> fileImportLogAggregateRepository,
                                      IAggregateRepository<FileAggregate, DomainEvent> fileAggregateRepository,
                                      ITransactionProcessorClient transactionProcessorClient,
                                      ISecurityServiceClient securityServiceClient,
                                      Func<String, IFileFormatHandler> fileFormatHandlerResolver,
                                      IFileSystem fileSystem) {
        this.FileProcessorManager = fileProcessorManager;
        this.FileImportLogAggregateRepository = fileImportLogAggregateRepository;
        this.FileAggregateRepository = fileAggregateRepository;
        this.TransactionProcessorClient = transactionProcessorClient;
        this.SecurityServiceClient = securityServiceClient;
        this.FileFormatHandlerResolver = fileFormatHandlerResolver;
        this.FileSystem = fileSystem;
    }

    private async Task<Result> ApplyFileUpdates(Func<FileAggregate, Task<Result>> action,
                                                  Guid fileId,
                                                  CancellationToken cancellationToken,
                                                  Boolean isNotFoundError = true)
    {
        try
        {
            Result<FileAggregate> getFileResult = await this.FileAggregateRepository.GetLatestVersion(fileId, cancellationToken);
            Result<FileAggregate> fileAggregateResult =
                DomainServiceHelper.HandleGetAggregateResult(getFileResult, fileId, isNotFoundError);
            if (fileAggregateResult.IsFailed)
                return ResultHelpers.CreateFailure(fileAggregateResult);
            FileAggregate fileAggregate = fileAggregateResult.Data;
            Result result = await action(fileAggregate);
            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);
            Logger.LogWarning("About to save");
            Result saveResult = await this.FileAggregateRepository.SaveChanges(fileAggregate, cancellationToken);
            if (saveResult.IsFailed)
                return ResultHelpers.CreateFailure(saveResult);
            Logger.LogWarning("About to return success after save");
            return Result.Success();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
            return Result.Failure(ex.GetExceptionMessages());
        }
    }

    private async Task<Result<T>> ApplyFileImportLogUpdates<T>(Func<FileImportLogAggregate, Task<Result<T>>> action,
                                                      Guid fileImportLogId,
                                                      CancellationToken cancellationToken,
                                                      Boolean isNotFoundError = true)
    {
        try
        {

            Result<FileImportLogAggregate> getFileImportLogResult = await this.FileImportLogAggregateRepository.GetLatestVersion(fileImportLogId, cancellationToken);
            Result<FileImportLogAggregate> fileImportLogAggregateResult =
                DomainServiceHelper.HandleGetAggregateResult(getFileImportLogResult, fileImportLogId, isNotFoundError);

            FileImportLogAggregate fileImportLogAggregate = fileImportLogAggregateResult.Data;
            Result<T> result = await action(fileImportLogAggregate);
            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            Result saveResult = await this.FileImportLogAggregateRepository.SaveChanges(fileImportLogAggregate, cancellationToken);
            if (saveResult.IsFailed)
                return ResultHelpers.CreateFailure(saveResult);
            return Result.Success(result.Data);
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.GetExceptionMessages());
        }
    }

    public async Task<Result<Guid>> UploadFile(FileCommands.UploadFileCommand command,
                                       CancellationToken cancellationToken) {
        DateTime importLogDateTime = command.FileUploadedDateTime;

        Result validateResult = this.ValidateRequest(command);
        if (validateResult.IsFailed)
            return validateResult;

        // This will now create the import log and add an event for the file being uploaded
        Guid importLogId = Helpers.CalculateFileImportLogAggregateId(importLogDateTime.Date, command.EstateId);

        Result<Guid> result = await ApplyFileImportLogUpdates(async (fileImportLogAggregate) => {
            if (fileImportLogAggregate.IsCreated == false)
            {
                // First file of the day so create
                Result result = fileImportLogAggregate.CreateImportLog(command.EstateId, importLogDateTime);
                if (result.IsFailed) {
                    return result;
                }
            }

            // Move the file
            Result<FileProfile> getFileProfileResult = await this.FileProcessorManager.GetFileProfile(command.FileProfileId, cancellationToken);
            if (getFileProfileResult.IsFailed) {
                return ResultHelpers.CreateFailure(getFileProfileResult);
            }

            FileProfile fileProfile = getFileProfileResult.Data;
            if (fileProfile == null) {
                return Result.NotFound($"No file profile found with Id {command.FileProfileId}");
            }

            // Copy file from the temp location to file processing listening directory
            IFileInfo file = this.FileSystem.FileInfo.New(command.FilePath);
            if (file.Exists == false) {
                return Result.NotFound($"File {file.FullName} not found");
            }

            String originalName = file.Name;

            if (this.FileSystem.Directory.Exists(fileProfile.ListeningDirectory) == false) {
                return Result.NotFound($"Directory {fileProfile.ListeningDirectory} not found");
            }

            // Read the file data
            String fileContent = null;
            //Open file for Read\Write
            await using (Stream fs = file.Open(FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read)) {
                //Create object of StreamReader by passing FileStream object on which it needs to operates on
                using (StreamReader sr = new StreamReader(fs)) {
                    //Use ReadToEnd method to read all the content from file
                    fileContent = await sr.ReadToEndAsync(cancellationToken);
                }
            }

            Guid fileId = this.CreateGuidFromFileData(fileContent);

            String fileDestination = $"{fileProfile.ListeningDirectory}//{command.EstateId:N}-{fileId:N}";
            file.MoveTo(fileDestination, overwrite: true);

            // Update Import log aggregate
            Result stateResult= fileImportLogAggregate.AddImportedFile(fileId, command.MerchantId, command.UserId, command.FileProfileId, originalName, fileDestination, command.FileUploadedDateTime);
            if (stateResult.IsFailed)
                return stateResult;
            
            return Result.Success(fileId);
        }, importLogId, cancellationToken, false);
        
        return result;
    }

    private Result ValidateRequest(FileCommands.UploadFileCommand command) {
        if (command.UserId == Guid.Empty) {
            return Result.Invalid("No User Id provided with file upload");
        }

        if (command.MerchantId == Guid.Empty) {
            return Result.Invalid("No Merchant Id provided with file upload");
        }

        if (command.FileProfileId == Guid.Empty) {
            return Result.Invalid("No File Profile Id provided with file upload");
        }

        return Result.Success();
    }

    public async Task<Result> ProcessUploadedFile(FileCommands.ProcessUploadedFileCommand command,
                                                  CancellationToken cancellationToken) {
        // TODO: Should the file id be generated from the file uploaded to protect against duplicate files???
        Result result = await ApplyFileUpdates(async (FileAggregate fileAggregate) => {
            Logger.LogWarning("In ProcessUploadedFile action");
            Result<Guid> operatorIdResult = await GetOperatorIdForFileProfile(command.EstateId, command.FileProfileId, cancellationToken);
            if (operatorIdResult.IsFailed)
                return ResultHelpers.CreateFailure(operatorIdResult);

            Logger.LogWarning("About to Create File");
            Result stateResult = fileAggregate.CreateFile(command.FileImportLogId, command.EstateId, command.MerchantId, command.UserId, command.FileProfileId, command.FilePath, command.FileUploadedDateTime, operatorIdResult.Data);
            if (stateResult.IsFailed)
                return stateResult;

            Logger.LogWarning("About to return success");
            return Result.Success();

        }, command.FileId, cancellationToken, false );

        if (result.IsFailed)
            return result;
        
        Result processResult = await this.ProcessFile(command.FileId, command.FileProfileId, command.FilePath, cancellationToken);
        
        return processResult;
    }

    private async Task<Result<Guid>> GetOperatorIdForFileProfile(Guid estateId, Guid fileProfileId, CancellationToken cancellationToken){

        Result<FileProfile> fileProfileResult = await this.FileProcessorManager.GetFileProfile(fileProfileId, cancellationToken);

        if (fileProfileResult.IsFailed){
            Logger.LogInformation($"file profile {fileProfileId} not  found");
            return ResultHelpers.CreateFailure(fileProfileResult);
        }
        FileProfile fileProfile = fileProfileResult.Data;

        Result<TokenResponse> getTokenResult = await this.GetToken(cancellationToken);
        if (getTokenResult.IsFailed) {
            return ResultHelpers.CreateFailure(getTokenResult);
        }
        this.TokenResponse = getTokenResult.Data;
        Result <List<OperatorResponse>> getOperatorsResult = await this.TransactionProcessorClient.GetOperators(this.TokenResponse.AccessToken, estateId, cancellationToken);
        if (getOperatorsResult.IsFailed) {
            return ResultHelpers.CreateFailure(getOperatorsResult);
        }

        List<OperatorResponse> operatorList = getOperatorsResult.Data;
        OperatorResponse @operator = operatorList.SingleOrDefault(o => o.Name == fileProfile.OperatorName);
        if (@operator == null){
            return Result.NotFound($"No operator record found with name [{fileProfile.OperatorName}]");
        }

        return Result.Success(@operator.OperatorId);
    }

    public async Task<Result> ProcessTransactionForFileLine(FileCommands.ProcessTransactionForFileLineCommand command,
                                                            CancellationToken cancellationToken) {
        Result result = await ApplyFileUpdates(async (FileAggregate fileAggregate) => {
            FileDetails fileDetails = fileAggregate.GetFile();

            if (fileDetails.FileLines.Any() == false) {
                return Result.Invalid($"File Id [{command.FileId}] has no lines added");
            }

            FileLine fileLine = fileDetails.FileLines.SingleOrDefault(f => f.LineNumber == command.LineNumber);

            if (fileLine == null) {
                return Result.NotFound($"File Line Number {command.LineNumber} not found in File Id {command.FileId}");
            }

            if (fileLine.ProcessingResult != ProcessingResult.NotProcessed) {
                // Line already processed
                return Result.Success();
            }

            Result<FileProfile> fileProfileResult = await this.FileProcessorManager.GetFileProfile(fileDetails.FileProfileId, cancellationToken);

            if (fileProfileResult.IsFailed)
                return ResultHelpers.CreateFailure(fileProfileResult);

            FileProfile fileProfile = fileProfileResult.Data;

            // Determine if we need to actually process this file line
            if (this.FileLineCanBeIgnored(fileLine.LineData, fileProfile.FileFormatHandler))
            {
                // Write something to aggregate to say line was explicity ignored
                Result stateResult = fileAggregate.RecordFileLineAsIgnored(fileLine.LineNumber);
                if (stateResult.IsFailed)
                    return stateResult;
                return Result.Success();
            }

            // need to now parse the line (based on the file format), this builds the metadata
            Dictionary<String, String> transactionMetadata = this.ParseFileLine(fileLine.LineData, fileProfile.FileFormatHandler);

            if (transactionMetadata.Any() == false) {
                // Line failed to parse so record this
                Result stateResult = fileAggregate.RecordFileLineAsRejected(fileLine.LineNumber, "Invalid Format");
                if (stateResult.IsFailed)
                    return stateResult;
                return Result.Success();
            }

            // Add the file data to the request metadata
            transactionMetadata.Add("FileId", command.FileId.ToString());
            transactionMetadata.Add("FileLineNumber", fileLine.LineNumber.ToString());

            String operatorName = fileProfile.OperatorName;
            if (transactionMetadata.ContainsKey("OperatorName")) {
                // extract the value
                operatorName = transactionMetadata["OperatorName"];
                transactionMetadata = transactionMetadata.Where(x => x.Key != "OperatorName").ToDictionary(x => x.Key, x => x.Value);
            }

            Result<TokenResponse> getTokenResult = await this.GetToken(cancellationToken);
            if (getTokenResult.IsFailed) {
                return ResultHelpers.CreateFailure(getTokenResult);
            }

            this.TokenResponse = getTokenResult.Data;

            Interlocked.Increment(ref TransactionNumber);

            // Get the merchant details
            Result<MerchantResponse> getMerchantResult = await this.TransactionProcessorClient.GetMerchant(this.TokenResponse.AccessToken, fileDetails.EstateId, fileDetails.MerchantId, cancellationToken);
            if (getMerchantResult.IsFailed) {
                return ResultHelpers.CreateFailure(getMerchantResult);
            }

            MerchantResponse merchant = getMerchantResult.Data;

            Result<List<ContractResponse>> getContractsResult = await this.TransactionProcessorClient.GetMerchantContracts(this.TokenResponse.AccessToken, fileDetails.EstateId, fileDetails.MerchantId, cancellationToken);
            if (getContractsResult.IsFailed) {
                return ResultHelpers.CreateFailure(getContractsResult);
            }

            List<ContractResponse> contracts = getContractsResult.Data;

            if (contracts.Any() == false) {
                return Result.NotFound($"No contracts found for Merchant Id {fileDetails.MerchantId} on estate Id {fileDetails.EstateId}");
            }

            ContractResponse? contract = null;
            if (fileProfile.OperatorName == "Voucher") {
                contract = contracts.SingleOrDefault(c => c.Description.Contains(operatorName));
            }
            else {
                contract = contracts.SingleOrDefault(c => c.OperatorName == operatorName);
            }

            if (contract == null) {
                return Result.NotFound($"No merchant contract for operator Id {operatorName} found for Merchant Id {merchant.MerchantId}");
            }

            ContractProduct? product = contract.Products.SingleOrDefault(p => p.Value == null); // TODO: Is this enough or should the name be used and stored in file profile??

            if (product == null) {
                return Result.NotFound($"No variable value product found on the merchant contract for operator Id {fileProfile.OperatorName} and Merchant Id {merchant.MerchantId}");
            }

            // Build a transaction request message
            SaleTransactionRequest saleTransactionRequest = new SaleTransactionRequest
            {
                EstateId = fileDetails.EstateId,
                MerchantId = fileDetails.MerchantId,
                TransactionDateTime = fileDetails.FileReceivedDateTime,
                TransactionNumber = TransactionNumber.ToString(),
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
            Result<SerialisedMessage> result= await this.TransactionProcessorClient.PerformTransaction(this.TokenResponse.AccessToken, serialisedRequestMessage, cancellationToken);
            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);
            SerialisedMessage serialisedResponseMessage = result.Data;

            // Get the sale transaction response
            SaleTransactionResponse saleTransactionResponse = JsonConvert.DeserializeObject<SaleTransactionResponse>(serialisedResponseMessage.SerialisedData);

            if (saleTransactionResponse.ResponseCode == "0000") {
                // record response against file line in file aggregate
                Result stateResult = fileAggregate.RecordFileLineAsSuccessful(command.LineNumber, saleTransactionResponse.TransactionId);
                if (stateResult.IsFailed)
                    return stateResult;
            }
            else
            {
                Result stateResult = fileAggregate.RecordFileLineAsFailed(command.LineNumber, saleTransactionResponse.TransactionId, saleTransactionResponse.ResponseCode, saleTransactionResponse.ResponseMessage);
                if (stateResult.IsFailed)
                    return stateResult;
            }

            return Result.Success();

        }, command.FileId, cancellationToken);

        return result;
    }

    private async Task<Result> ProcessFile(Guid fileId,
                                           Guid fileProfileId,
                                           String fileName,
                                           CancellationToken cancellationToken) {
        IFileInfo inProgressFile = null;
        FileProfile fileProfile = null;

        Result<FileProfile> fileProfileResult =
            await this.FileProcessorManager.GetFileProfile(fileProfileId, cancellationToken);

        if (fileProfileResult.IsFailed)
            return ResultHelpers.CreateFailure(fileProfileResult);
        fileProfile = fileProfileResult.Data;
        // Check the processed/failed directories exist
        if (this.FileSystem.Directory.Exists(fileProfile.ProcessedDirectory) == false) {
            Logger.LogInformation($"Creating Directory {fileProfile.ProcessedDirectory} as not found");
            this.FileSystem.Directory.CreateDirectory(fileProfile.ProcessedDirectory);
        }

        if (this.FileSystem.Directory.Exists(fileProfile.FailedDirectory) == false) {
            Logger.LogInformation($"Creating Directory {fileProfile.FailedDirectory} as not found");
            this.FileSystem.Directory.CreateDirectory(fileProfile.FailedDirectory);
        }

        inProgressFile = this.FileSystem.FileInfo.New(fileName);

        if (inProgressFile.Exists == false) {
            // We also want to check the processed and failed folders incase this is an event replay
            IFileInfo failedFileInfo =
                this.FileSystem.FileInfo.New($"{fileProfile.FailedDirectory}/{inProgressFile.Name}");
            IFileInfo processedFileInfo =
                this.FileSystem.FileInfo.New($"{fileProfile.ProcessedDirectory}/{inProgressFile.Name}");

            (Boolean, Boolean) fileStatus = (failedFileInfo.Exists, processedFileInfo.Exists);

            IFileInfo fileInfo = fileStatus switch
            {
                (true, false) => failedFileInfo,
                (false, true) => processedFileInfo,
                _ => null
            };

            if (fileInfo == null) {
                return Result.NotFound($"File {inProgressFile.FullName} not found");
            }

            // Overwrite the inprogress file info object with the file found in the failed folder
            inProgressFile = fileInfo;
        }

        Result result = await ApplyFileUpdates(async (FileAggregate fileAggregate) => {

            String fileContent = null;
            //Open file for Read\Write
            using (Stream fs = inProgressFile.Open(FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read)) {
                //Create object of StreamReader by passing FileStream object on which it needs to operates on
                using (StreamReader sr = new StreamReader(fs)) {
                    //Use ReadToEnd method to read all the content from file
                    fileContent = await sr.ReadToEndAsync(cancellationToken);
                }
            }

            if (String.IsNullOrEmpty(fileContent) == false) {
                String[] fileLines = fileContent.Split(fileProfile.LineTerminator);

                foreach (String fileLine in fileLines) {
                    Result stateResult = fileAggregate.AddFileLine(fileLine.Trim());
                    if (stateResult.IsFailed)
                        return stateResult;
                }
            }

            return Result.Success();
        }, fileId, cancellationToken);

        if (result.IsSuccess) {
            Logger.LogInformation($"About to move file {inProgressFile.Name} to [{fileProfile.ProcessedDirectory}]");

            // Move file now
            inProgressFile.MoveTo($"{fileProfile.ProcessedDirectory}/{inProgressFile.Name}", true);
        }
        else {
            Logger.LogWarning($"About to move file {inProgressFile.Name} to [{fileProfile.FailedDirectory}]. Reason(s) [{String.Join(",", result.Errors)}]");
            inProgressFile.MoveTo($"{fileProfile.FailedDirectory}/{inProgressFile.Name}", true);
        }

        return result;
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
            return new Dictionary<String, String>();
        }
    }

    private TokenResponse TokenResponse;

    [ExcludeFromCodeCoverage]
    private async Task<Result<TokenResponse>> GetToken(CancellationToken cancellationToken)
    {
        // Get a token to talk to the estate service
        String clientId = ConfigurationReader.GetValue("AppSettings", "ClientId");
        String clientSecret = ConfigurationReader.GetValue("AppSettings", "ClientSecret");

        if (this.TokenResponse == null)
        {
            Result<TokenResponse> getTokenResult = await this.SecurityServiceClient.GetToken(clientId, clientSecret, cancellationToken);
            if (getTokenResult.IsFailed)
            {
                Logger.LogWarning($"Failed to get token: {getTokenResult.Message}");
                return ResultHelpers.CreateFailure(getTokenResult);
            }
            
            return getTokenResult.Data;
        }

        if (this.TokenResponse.Expires.UtcDateTime.Subtract(DateTime.UtcNow) < TimeSpan.FromMinutes(2))
        {
            Logger.LogDebug($"Token is about to expire at {this.TokenResponse.Expires.DateTime:O}");
            Result<TokenResponse> getTokenResult = await this.SecurityServiceClient.GetToken(clientId, clientSecret, cancellationToken);
            if (getTokenResult.IsFailed)
            {
                Logger.LogWarning($"Failed to get token: {getTokenResult.Message}");
                return ResultHelpers.CreateFailure(getTokenResult);
            }

            return getTokenResult.Data;
        }

        return this.TokenResponse;
    }
}