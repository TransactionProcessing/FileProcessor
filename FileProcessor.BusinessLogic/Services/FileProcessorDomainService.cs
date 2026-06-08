using SecurityService.DataTransferObjects;
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
using Requests;
using SecurityService.Client;
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
    Task<Result> UploadFile(FileCommands.UploadFileCommand command, CancellationToken cancellationToken);

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

    private async Task<Result> ApplyFileImportLogUpdates(Func<FileImportLogAggregate, Task<Result>> action,
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
            Result result = await action(fileImportLogAggregate);
            if (result.IsFailed)
                return ResultHelpers.CreateFailure(result);

            Result saveResult = await this.FileImportLogAggregateRepository.SaveChanges(fileImportLogAggregate, cancellationToken);
            if (saveResult.IsFailed)
                return ResultHelpers.CreateFailure(saveResult);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.GetExceptionMessages());
        }
    }

    public async Task<Result> UploadFile(FileCommands.UploadFileCommand command,
                                       CancellationToken cancellationToken) {
        DateTime importLogDateTime = command.FileUploadedDateTime;

        Result validateResult = this.ValidateRequest(command);
        if (validateResult.IsFailed)
            return validateResult;

        // This will now create the import log and add an event for the file being uploaded
        Guid importLogId = Helpers.CalculateFileImportLogAggregateId(importLogDateTime.Date, command.EstateId);

        Result result = await ApplyFileImportLogUpdates(async (fileImportLogAggregate) => {
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
            Guid generatedFileId = CreateGuidFromFileData(fileContent);

            if (generatedFileId != command.FileId) {
                return Result.Invalid("File content does not match the file id provided");
            }
            
            String fileDestination = $"{fileProfile.ListeningDirectory}//{command.EstateId:N}-{command.FileId:N}";
            file.MoveTo(fileDestination, overwrite: true);

            // Update Import log aggregate
            Result stateResult= fileImportLogAggregate.AddImportedFile(command.FileId, command.MerchantId, command.UserId, command.FileProfileId, originalName, fileDestination, command.FileUploadedDateTime);
            if (stateResult.IsFailed)
                return stateResult;
            
            return Result.Success();
        }, importLogId, cancellationToken, false);
        
        return result;
    }

    public static Guid CreateGuidFromFileData(String fileContents)
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
                                                            CancellationToken cancellationToken)
    {
        var result = await ApplyFileUpdates(
            async fileAggregate =>
            {
                var fileDetails = fileAggregate.GetFile();

                var fileLineResult = GetFileLine(fileDetails, command);
                if (fileLineResult.IsFailed)
                    return ResultHelpers.CreateFailure(fileLineResult);

                var fileLine = fileLineResult.Data;

                if (IsAlreadyProcessed(fileLine))
                    return Result.Success();

                var profileResult = await GetFileProfile(fileDetails, cancellationToken);
                if (profileResult.IsFailed)
                    return ResultHelpers.CreateFailure(profileResult);

                var profile = profileResult.Data;

                var parsedResult = await ProcessFileLine(fileAggregate, fileDetails, fileLine, profile, command, cancellationToken);
                if (parsedResult.IsFailed)
                    return ResultHelpers.CreateFailure(parsedResult);

                return Result.Success();
            },
            command.FileId,
            cancellationToken);

        if (result.IsFailed)
            Logger.LogWarning($"{command.LineNumber} Status {result.Status} Message {result.Message}");

        return result;
    }

    private async Task<Result<FileProfile>> GetFileProfile(FileDetails fileDetails,
                                                           CancellationToken cancellationToken)
    {
        var result = await FileProcessorManager.GetFileProfile(
            fileDetails.FileProfileId,
            cancellationToken);

        if (result.IsFailed)
            return ResultHelpers.CreateFailure<FileProfile>(result);

        return Result.Success(result.Data);
    }

    private Result<FileLine> GetFileLine(FileDetails fileDetails, FileCommands.ProcessTransactionForFileLineCommand command)
    {
        if (!fileDetails.FileLines.Any())
            return Result.Invalid($"File Id [{command.FileId}] has no lines added");

        var fileLine = fileDetails.FileLines
            .SingleOrDefault(x => x.LineNumber == command.LineNumber);

        return fileLine is null
            ? Result.NotFound($"File Line Number {command.LineNumber} not found in File Id {command.FileId}")
            : Result.Success(fileLine);
    }

    private static bool IsAlreadyProcessed(FileLine fileLine) =>
        fileLine.ProcessingResult != ProcessingResult.NotProcessed;


    private async Task<Result> ProcessFileLine(FileAggregate fileAggregate,
                                               FileDetails fileDetails,
                                               FileLine fileLine,
                                               FileProfile fileProfile,
                                               FileCommands.ProcessTransactionForFileLineCommand command,
                                               CancellationToken cancellationToken)
    {
        if (FileLineCanBeIgnored(fileLine.LineData, fileProfile.FileFormatHandler))
        {
            var ignored = fileAggregate.RecordFileLineAsIgnored(fileLine.LineNumber);
            return ignored.IsFailed ? ignored : Result.Success();
        }

        var metadata = ParseFileLine(fileLine.LineData, fileProfile.FileFormatHandler);

        if (!metadata.Any())
        {
            var rejected = fileAggregate.RecordFileLineAsRejected(fileLine.LineNumber, "Invalid Format");
            return rejected.IsFailed ? rejected : Result.Success();
        }

        EnrichMetadata(metadata, command.FileId, fileLine.LineNumber);

        var operatorName = ExtractOperatorName(ref metadata, fileProfile);

        var tokenResult = await GetToken(CancellationToken.None);
        if (tokenResult.IsFailed)
            return ResultHelpers.CreateFailure(tokenResult);

        TokenResponse = tokenResult.Data;

        var transactionContext = await BuildTransactionContext(fileDetails, operatorName, cancellationToken);
        if (transactionContext.IsFailed)
            return ResultHelpers.CreateFailure(transactionContext);

        var txResult = await ExecuteTransaction(metadata, transactionContext.Data, fileDetails, cancellationToken);
        if (txResult.IsFailed)
            return ResultHelpers.CreateFailure(txResult);

        return ApplyFileLineOutcome(fileAggregate, fileLine.LineNumber, txResult.Data);
    }

    private static void EnrichMetadata(Dictionary<string, string> metadata,
                                       Guid fileId,
                                       int lineNumber)
    {
        metadata["FileId"] = fileId.ToString();
        metadata["FileLineNumber"] = lineNumber.ToString();
    }

    private static string ExtractOperatorName(ref Dictionary<string, string> metadata,
                                              FileProfile fileProfile)
    {
        var operatorName = fileProfile.OperatorName;

        if (!metadata.TryGetValue("OperatorName", out var extracted))
            return operatorName;

        operatorName = extracted;

        // remove it from metadata
        metadata = metadata
            .Where(x => x.Key != "OperatorName")
            .ToDictionary(x => x.Key, x => x.Value);

        return operatorName;
    }

    private async Task<Result<TransactionContext>> BuildTransactionContext(FileDetails fileDetails,
                                                                           string operatorName,
                                                                           CancellationToken cancellationToken)
    {
        var tokenResult = await GetToken(cancellationToken);
        if (tokenResult.IsFailed)
            return ResultHelpers.CreateFailure(tokenResult);

        TokenResponse = tokenResult.Data;

        var merchantResult = await TransactionProcessorClient.GetMerchant(TokenResponse.AccessToken,
            fileDetails.EstateId,
            fileDetails.MerchantId,
            cancellationToken);

        if (merchantResult.IsFailed)
            return ResultHelpers.CreateFailure(merchantResult);

        var merchant = merchantResult.Data;

        var contractsResult = await TransactionProcessorClient.GetMerchantContracts(TokenResponse.AccessToken,
            fileDetails.EstateId,
            fileDetails.MerchantId,
            cancellationToken);

        if (contractsResult.IsFailed)
            return ResultHelpers.CreateFailure(contractsResult);

        var contracts = contractsResult.Data;

        if (!contracts.Any())
            return Result.NotFound($"No contracts found for Merchant Id {fileDetails.MerchantId} on estate Id {fileDetails.EstateId}");

        var contract = ResolveContract(fileProfileOperator: operatorName, contracts);

        if (contract is null)
            return Result.NotFound($"No merchant contract for operator Id {operatorName} found for Merchant Id {merchant.MerchantId}");

        var product = contract.Products.SingleOrDefault(p => p.Value == null);

        if (product is null)
            return Result.NotFound($"No variable value product found for operator {operatorName} and Merchant Id {merchant.MerchantId}");

        return Result.Success(new TransactionContext
        {
            Merchant = merchant,
            Contract = contract,
            Product = product
        });
    }

    private sealed class TransactionContext
    {
        public MerchantResponse Merchant { get; set; } = default!;
        public ContractResponse Contract { get; set; } = default!;
        public ContractProduct Product { get; set; } = default!;
    }

    private ContractResponse? ResolveContract(string fileProfileOperator, List<ContractResponse> contracts)
    {
        if (fileProfileOperator == "Voucher")
        {
            return contracts.SingleOrDefault(c =>
                c.Description.Contains(fileProfileOperator));
        }

        return contracts.SingleOrDefault(c =>
            c.OperatorName == fileProfileOperator);
    }

    private async Task<Result<SaleTransactionResponse>> ExecuteTransaction(
        Dictionary<string, string> metadata,
        TransactionContext context,
        FileDetails fileDetails,
        CancellationToken cancellationToken)
    {
        var request = new SaleTransactionRequest
        {
            EstateId = fileDetails.EstateId,
            MerchantId = fileDetails.MerchantId,
            TransactionDateTime = fileDetails.FileReceivedDateTime,
            TransactionNumber = Interlocked.Increment(ref TransactionNumber).ToString(),
            TransactionType = "Sale",
            ContractId = context.Contract.ContractId,
            DeviceIdentifier = context.Merchant.Devices.First().Value,
            OperatorId = context.Contract.OperatorId,
            ProductId = context.Product.ProductId,
            AdditionalTransactionMetadata = metadata,
            TransactionSource = 2
        };

        return await TransactionProcessorClient.PerformTransaction(
            TokenResponse.AccessToken,
            request,
            cancellationToken);
    }

    private static Result ApplyFileLineOutcome(
        FileAggregate fileAggregate,
        int lineNumber,
        SaleTransactionResponse response)
    {
        if (response.ResponseCode == "0000")
        {
            var success = fileAggregate.RecordFileLineAsSuccessful(
                lineNumber,
                response.TransactionId);

            return success.IsFailed ? success : Result.Success();
        }

        var failed = fileAggregate.RecordFileLineAsFailed(
            lineNumber,
            response.TransactionId,
            response.ResponseCode,
            response.ResponseMessage);

        return failed.IsFailed ? failed : Result.Success();
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