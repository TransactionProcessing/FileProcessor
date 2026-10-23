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
using System.Globalization;
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
using FileProfileModel = global::FileProcessor.Models.FileProfile;

public interface IFileProcessorDomainService
{
    Task<Result> UploadFile(FileCommands.UploadFileCommand command, CancellationToken cancellationToken);

    Task<Result> ProcessUploadedFile(FileCommands.ProcessUploadedFileCommand command,
                                     CancellationToken cancellationToken);

    Task<Result> ProcessTransactionForFileLine(FileCommands.ProcessTransactionForFileLineCommand command,
                                               CancellationToken cancellationToken);

    Task<Result> ReplayFileLine(FileCommands.ReplayFileLineCommand command, CancellationToken cancellationToken);
}

public class FileProcessorDomainService : IFileProcessorDomainService
{
    private const Int32 DefaultMaximumDetailRecordCount = 1000;
    private const String MaximumDetailRecordsExceededCode = "MaximumDetailRecordsExceeded";

    private readonly IFileProcessorManager FileProcessorManager;

    private readonly IAggregateRepository<FileImportLogAggregate, DomainEvent> FileImportLogAggregateRepository;

    private readonly IAggregateRepository<FileAggregate, DomainEvent> FileAggregateRepository;

    private readonly ITransactionProcessorClient TransactionProcessorClient;

    private readonly ISecurityServiceClient SecurityServiceClient;

    private readonly Func<String, IFileFormatHandler> FileFormatHandlerResolver;

    private readonly IFileSystem FileSystem;

    private readonly Int32 MaximumDetailRecordCount;

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
        this.MaximumDetailRecordCount = ResolveMaximumDetailRecordCount();
    }

    public static Int32 ResolveMaximumDetailRecordCount()
    {
        String configuredMaximumDetailRecordCount = ConfigurationReader.GetValue("AppSettings", "MaximumDetailRecordCount");
        if (String.IsNullOrWhiteSpace(configuredMaximumDetailRecordCount))
            return DefaultMaximumDetailRecordCount;

        if (Int32.TryParse(configuredMaximumDetailRecordCount, out Int32 maximumDetailRecordCount) == false || maximumDetailRecordCount <= 0)
            throw new InvalidOperationException("AppSettings:MaximumDetailRecordCount must be a positive integer when configured");

        return maximumDetailRecordCount;
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

        // The import log is created only after validation and duplicate checks succeed.
        Guid importLogId = Helpers.CalculateFileImportLogAggregateId(importLogDateTime.Date, command.EstateId);

        Result result = await ApplyFileImportLogUpdates(
            fileImportLogAggregate => this.RegisterUploadedFile(command, importLogDateTime, fileImportLogAggregate, cancellationToken),
            importLogId,
            cancellationToken,
            false);

        return result;
    }

    private async Task<Result> RegisterUploadedFile(FileCommands.UploadFileCommand command,
                                                    DateTime importLogDateTime,
                                                    FileImportLogAggregate fileImportLogAggregate,
                                                    CancellationToken cancellationToken)
    {
        IFileInfo file = this.FileSystem.FileInfo.New(command.FilePath);
        if (file.Exists == false)
            return Result.NotFound($"File {file.FullName} not found");

        String originalName = String.IsNullOrWhiteSpace(command.OriginalFileName) ? file.Name : command.OriginalFileName;
        Result contentResult = await this.ValidateUploadedFileContent(file, command.FileId, cancellationToken);
        if (contentResult.IsFailed)
            return contentResult;

        Result<Boolean> existingFileResult = await this.HandleExistingFile(command, cancellationToken);
        if (existingFileResult.IsFailed)
            return ResultHelpers.CreateFailure(existingFileResult);
        if (existingFileResult.Data)
            return Result.Success();

        if (fileImportLogAggregate.ContainsFile(command.FileId))
            return this.RegisterDuplicateImportLogFile(command, originalName, fileImportLogAggregate);

        return await this.RegisterNewImportLogFile(command,
                                                   originalName,
                                                   importLogDateTime,
                                                   fileImportLogAggregate,
                                                   cancellationToken);
    }

    private async Task<Result> ValidateUploadedFileContent(IFileInfo file,
                                                           Guid fileId,
                                                           CancellationToken cancellationToken)
    {
        // Validate the same original bytes used by the uploader to calculate the file id.
        Byte[] fileContent;
        await using (Stream stream = file.Open(FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read))
        using (MemoryStream memoryStream = new())
        {
            await stream.CopyToAsync(memoryStream, cancellationToken);
            fileContent = memoryStream.ToArray();
        }

        return CreateGuidFromFileData(fileContent) == fileId
            ? Result.Success()
            : Result.Invalid("File content does not match the file id provided");
    }

    private async Task<Result<Boolean>> HandleExistingFile(FileCommands.UploadFileCommand command,
                                                           CancellationToken cancellationToken)
    {
        Result<FileAggregate> existingFileResult = await this.FileAggregateRepository.GetLatestVersion(command.FileId, cancellationToken);
        if (existingFileResult?.IsSuccess == true && existingFileResult.Data?.IsCreated == true)
        {
            FileDetails existingFile = existingFileResult.Data.GetFile();
            if (existingFile.EstateId != command.EstateId ||
                existingFile.MerchantId != command.MerchantId ||
                existingFile.UserId != command.UserId ||
                existingFile.FileProfileId != command.FileProfileId)
                return Result.Invalid($"File Id [{command.FileId}] is already registered with different upload details");

            this.DeleteDuplicateUploadFile(command.FilePath);
            return Result.Success(true);
        }

        if (existingFileResult?.IsFailed == true && existingFileResult.Status != ResultStatus.NotFound)
            return ResultHelpers.CreateFailure(existingFileResult);

        return Result.Success(false);
    }

    private Result RegisterDuplicateImportLogFile(FileCommands.UploadFileCommand command,
                                                  String originalName,
                                                  FileImportLogAggregate fileImportLogAggregate)
    {
        Result result = fileImportLogAggregate.AddImportedFile(new AddImportedFileRequest(command.FileId,
                                                                                         command.MerchantId,
                                                                                         command.UserId,
                                                                                         command.FileProfileId,
                                                                                         originalName,
                                                                                         command.FilePath,
                                                                                         command.FileUploadedDateTime));
        if (result.IsSuccess)
            this.DeleteDuplicateUploadFile(command.FilePath);

        return result;
    }

    private async Task<Result> RegisterNewImportLogFile(FileCommands.UploadFileCommand command,
                                                        String originalName,
                                                        DateTime importLogDateTime,
                                                        FileImportLogAggregate fileImportLogAggregate,
                                                        CancellationToken cancellationToken)
    {
        Result<FileProfileModel> profileResult = await this.FileProcessorManager.GetFileProfile(command.FileProfileId, cancellationToken);
        if (profileResult.IsFailed)
            return ResultHelpers.CreateFailure(profileResult);
        if (profileResult.Data == null)
            return Result.NotFound($"No file profile found with Id {command.FileProfileId}");

        Guid fileProfileVersionId = profileResult.Data.FileProfileVersionId == Guid.Empty
            ? profileResult.Data.FileProfileId
            : profileResult.Data.FileProfileVersionId;

        if (fileImportLogAggregate.IsCreated == false)
        {
            Result createResult = fileImportLogAggregate.CreateImportLog(command.EstateId, importLogDateTime);
            if (createResult.IsFailed)
                return createResult;
        }

        return fileImportLogAggregate.AddImportedFile(new AddImportedFileRequest(command.FileId,
                                                                                 command.MerchantId,
                                                                                 command.UserId,
                                                                                 command.FileProfileId,
                                                                                 originalName,
                                                                                 command.FilePath,
                                                                                 command.FileUploadedDateTime,
                                                                                 fileProfileVersionId));
    }

    public static Guid CreateGuidFromFileData(String fileContents)
    {
        return CreateGuidFromFileData(Encoding.UTF8.GetBytes(fileContents));
    }

    public static Guid CreateGuidFromFileData(Byte[] fileContents)
    {
        using (SHA256 sha256Hash = SHA256.Create())
        {
            //Generate hash from the key
            Byte[] bytes = sha256Hash.ComputeHash(fileContents);

            Byte[] j = bytes.Skip(Math.Max(0, bytes.Count() - 16)).ToArray(); //Take last 16

            //Create our Guid.
            return new Guid(j);
        }
    }
    private Result ValidateRequest(FileCommands.UploadFileCommand command) {
        if (command.EstateId == Guid.Empty) {
            return Result.Invalid("No Estate Id provided with file upload");
        }

        if (command.FileId == Guid.Empty) {
            return Result.Invalid("No File Id provided with file upload");
        }

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

    private void DeleteDuplicateUploadFile(String filePath)
    {
        try
        {
            IFileInfo file = this.FileSystem.FileInfo.New(filePath);
            if (file.Exists)
                file.Delete();
        }
        catch (Exception exception)
        {
            Logger.LogWarning($"Failed to remove duplicate temporary upload file [{filePath}]. Error [{exception.Message}]");
        }
    }

    public async Task<Result> ProcessUploadedFile(FileCommands.ProcessUploadedFileCommand command,
                                                  CancellationToken cancellationToken) {
        Result result = await ApplyFileUpdates(async (FileAggregate fileAggregate) => {
            Logger.LogWarning("In ProcessUploadedFile action");
            Result<Guid> operatorIdResult = await GetOperatorIdForFileProfile(command.FileId,
                                                                              command.EstateId,
                                                                              command.FileProfileId,
                                                                              command.FileProfileVersionId,
                                                                              cancellationToken);
            if (operatorIdResult.IsFailed)
                return ResultHelpers.CreateFailure(operatorIdResult);

            Logger.LogWarning("About to Create File");
            FileAggregateExtensions.CreateFileRequest request = new(command.FileImportLogId, command.EstateId, command.MerchantId, command.UserId, command.FileProfileId, command.FilePath, command.FileUploadedDateTime, operatorIdResult.Data, command.FileProfileVersionId);
            Result stateResult = fileAggregate.CreateFile(request);
            if (stateResult.IsFailed)
                return stateResult;

            Logger.LogWarning("About to return success");
            return Result.Success();

        }, command.FileId, cancellationToken, false );

        if (result.IsFailed) {
            await MoveAcceptedFileToFailedDirectory(command.FileId,
                                                    command.FileProfileId,
                                                    command.FileProfileVersionId,
                                                    command.FilePath,
                                                    cancellationToken);
            return result;
        }

        Result processResult = await this.ProcessFile(command.FileId,
                                                      command.FileProfileId,
                                                      command.FileProfileVersionId,
                                                      command.FilePath,
                                                      cancellationToken);

        return processResult;
    }

    private async Task MoveAcceptedFileToFailedDirectory(Guid fileId,
                                                          Guid fileProfileId,
                                                          Guid fileProfileVersionId,
                                                          String filePath,
                                                          CancellationToken cancellationToken)
    {
        Result<FileProfileModel> fileProfileResult = await this.GetFileProfileForProcessing(fileId,
                                                                                              fileProfileId,
                                                                                              fileProfileVersionId,
                                                                                              cancellationToken);
        if (fileProfileResult.IsFailed || fileProfileResult.Data == null)
            return;

        IFileInfo file = this.FileSystem.FileInfo.New(filePath);
        if (file.Exists == false)
            return;

        if (this.FileSystem.Directory.Exists(fileProfileResult.Data.FailedDirectory) == false)
            this.FileSystem.Directory.CreateDirectory(fileProfileResult.Data.FailedDirectory);

        file.MoveTo($"{fileProfileResult.Data.FailedDirectory}/{file.Name}", overwrite: true);
    }

    private async Task<Result<Guid>> GetOperatorIdForFileProfile(Guid fileId,
                                                                 Guid estateId,
                                                                 Guid fileProfileId,
                                                                 Guid fileProfileVersionId,
                                                                 CancellationToken cancellationToken){

        Result<FileProfileModel> fileProfileResult = await this.GetFileProfileForProcessing(fileId,
                                                                                              fileProfileId,
                                                                                              fileProfileVersionId,
                                                                                              cancellationToken);

        if (fileProfileResult.IsFailed){
            Logger.LogInformation($"file profile {fileProfileId} not  found");
            return ResultHelpers.CreateFailure(fileProfileResult);
        }
        FileProfileModel fileProfile = fileProfileResult.Data;

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

    public async Task<Result> ReplayFileLine(FileCommands.ReplayFileLineCommand command, CancellationToken cancellationToken)
    {
        Result result = await ApplyFileUpdates(async (FileAggregate fileAggregate) =>
        {
            FileDetails fileDetails = fileAggregate.GetFile();
            // Now find the line to be resumbitted
            FileLine fileLine = fileDetails.FileLines.SingleOrDefault(l => l.LineNumber == command.LineNumber);
            if (fileLine == null)
            {
                return Result.NotFound($"Line number {command.LineNumber} not found in file Id {command.FileId}");
            }

            if (fileLine.ProcessingResult != ProcessingResult.NotProcessed)
            {
                return Result.Success();
            }

            // Build the command to reprocess the line
            FileCommands.ProcessTransactionForFileLineCommand processTransactionForFileLineCommand = new FileCommands.ProcessTransactionForFileLineCommand(command.FileId, fileLine.EventId,
                command.LineNumber, fileLine.LineData);
            Result result = await this.ProcessTransactionForFileLine(processTransactionForFileLineCommand, cancellationToken);
            return result;
        }, command.FileId, cancellationToken);

        return result;
    }


    public async Task<Result> ProcessTransactionForFileLine(FileCommands.ProcessTransactionForFileLineCommand command,
                                                            CancellationToken cancellationToken) {
        FileLineProcessingContext processingContext = new(command);
        Result dispatchFailureResult = null;
        Result result = await ApplyFileUpdates(async (FileAggregate fileAggregate) => {
            (Result stateResult, Result dispatchResult) = await this.ProcessTransactionForFileLineInternal(fileAggregate,
                                                                                                             command,
                                                                                                             processingContext,
                                                                                                             cancellationToken);
            dispatchFailureResult = dispatchResult;
            return stateResult;
        }, command.FileId, cancellationToken);

        if (result.IsSuccess && dispatchFailureResult != null)
            result = dispatchFailureResult;

        if (result.IsFailed)
        {
            FileLineProcessingDiagnostics.ProcessingFailed(processingContext, result);
            Logger.LogWarning($"{command.LineNumber} Status {result.Status} Message {result.Message}");
        }
        else if (processingContext.Stage == FileLineProcessingStage.FileLineStatePersisted)
        {
            FileLineProcessingDiagnostics.LineStatePersisted(processingContext);
        }

        return result;
    }

    private async Task<(Result StateResult, Result DispatchFailureResult)> ProcessTransactionForFileLineInternal(FileAggregate fileAggregate,
                                                                                                                    FileCommands.ProcessTransactionForFileLineCommand command,
                                                                                                                    FileLineProcessingContext processingContext,
                                                                                                                    CancellationToken cancellationToken)
    {
        FileDetails fileDetails = fileAggregate.GetFile();
        processingContext.EstateId = fileDetails.EstateId;
        processingContext.MerchantId = fileDetails.MerchantId;
        processingContext.FileProfileId = fileDetails.FileProfileId;
        FileLineProcessingDiagnostics.LineProcessingStarted(processingContext);

        processingContext.Stage = FileLineProcessingStage.FileLineLookup;
        if (fileDetails.FileLines.Any() == false)
            return (Result.Invalid($"File Id [{command.FileId}] has no lines added"), null);

        FileLine fileLine = fileDetails.FileLines.SingleOrDefault(f => f.LineNumber == command.LineNumber);
        if (fileLine == null)
            return (Result.NotFound($"File Line Number {command.LineNumber} not found in File Id {command.FileId}"), null);

        if (fileLine.ProcessingResult != ProcessingResult.NotProcessed)
        {
            FileLineProcessingDiagnostics.LineProcessingSkipped(processingContext, fileLine.ProcessingResult);
            return (Result.Success(), null);
        }

        processingContext.Stage = FileLineProcessingStage.FileProfileLookup;
        Result<FileProfileModel> fileProfileResult = await this.GetFileProfileForProcessing(command.FileId,
                                                                                              fileDetails.FileProfileId,
                                                                                              fileDetails.FileProfileVersionId,
                                                                                              cancellationToken);
        if (fileProfileResult.IsFailed)
            return (ResultHelpers.CreateFailure(fileProfileResult), null);

        FileProfileModel fileProfile = fileProfileResult.Data;
        processingContext.OperatorName = fileProfile.OperatorName;

        processingContext.Stage = FileLineProcessingStage.LineIgnoreEvaluation;
        if (this.FileLineCanBeIgnored(fileLine.LineData, fileProfile.FileFormatHandler))
        {
            processingContext.Stage = FileLineProcessingStage.FileLineStateUpdate;
            Result stateResult = fileAggregate.RecordFileLineAsIgnored(fileLine.LineNumber);
            if (stateResult.IsFailed)
                return (stateResult, null);

            FileLineProcessingDiagnostics.LineIgnored(processingContext);
            processingContext.Stage = FileLineProcessingStage.FileLineStatePersisted;
            return (Result.Success(), null);
        }

        processingContext.Stage = FileLineProcessingStage.TransactionDetailsParsing;
        Result<Dictionary<String, String>> metadataResult = this.BuildTransactionMetadata(command, fileLine, fileProfile, out String parsedOperatorName);
        processingContext.OperatorName = parsedOperatorName;
        if (metadataResult.IsFailed)
        {
            processingContext.Stage = FileLineProcessingStage.FileLineStateUpdate;
            Result stateResult = fileAggregate.RecordFileLineAsRejected(fileLine.LineNumber, "Invalid Format");
            if (stateResult.IsFailed)
                return (ResultHelpers.CreateFailure(stateResult), null);

            if (stateResult.IsSuccess)
            {
                FileLineProcessingDiagnostics.LineRejected(processingContext, "Invalid Format");
                processingContext.Stage = FileLineProcessingStage.FileLineStatePersisted;
            }

            return (stateResult, null);
        }

        return await this.ProcessTransaction(fileAggregate,
                                             command,
                                             fileDetails,
                                             fileProfile,
                                             metadataResult.Data,
                                             processingContext,
                                             cancellationToken);
    }

    private async Task<(Result StateResult, Result DispatchFailureResult)> ProcessTransaction(FileAggregate fileAggregate,
                                                                                                FileCommands.ProcessTransactionForFileLineCommand command,
                                                                                                FileDetails fileDetails,
                                                                                                FileProfileModel fileProfile,
                                                                                                Dictionary<String, String> transactionMetadata,
                                                                                                FileLineProcessingContext processingContext,
                                                                                                CancellationToken cancellationToken)
    {
        Int32 transactionNumber = Interlocked.Increment(ref TransactionNumber);

        Result<(Guid ContractId, Guid OperatorId, Guid ProductId, String MerchantDevice)> detailsForTransaction = await this.GetDetailsForTransaction(cancellationToken,
                                                                                                                                                       fileDetails,
                                                                                                                                                       fileProfile,
                                                                                                                                                       processingContext.OperatorName,
                                                                                                                                                       processingContext);
        if (detailsForTransaction.IsFailed)
            return (ResultHelpers.CreateFailure(detailsForTransaction), null);

        processingContext.Stage = FileLineProcessingStage.TransactionDetailsResolved;
        Result<SaleTransactionResponse> saleResult = await this.SendSaleTransaction(fileDetails,
                                                                                     detailsForTransaction,
                                                                                     transactionMetadata,
                                                                                     transactionNumber,
                                                                                     processingContext,
                                                                                     cancellationToken);
        processingContext.TransactionDispatchSucceeded = saleResult.IsSuccess;
        if (saleResult.IsSuccess)
            processingContext.ResponseCode = saleResult.Data.ResponseCode;

        // A failed dispatch has no definitive processor response. Leave the line unmodified so
        // the file processing workflow can retry it with the same file and line identity.
        if (saleResult.IsFailed)
        {
            Result stateResult = fileAggregate.RecordTransactionDispatchFailure(command.LineNumber,
                                                                                  transactionNumber,
                                                                                  processingContext.TransactionDispatchFailureType,
                                                                                  DateTime.UtcNow);
            if (stateResult.IsFailed)
                return (stateResult, null);

            processingContext.Stage = FileLineProcessingStage.TransactionDispatchAttemptPersisted;
            return (Result.Success(), ResultHelpers.CreateFailure(saleResult));
        }

        processingContext.Stage = FileLineProcessingStage.FileLineStateUpdate;
        Result stateUpdateResult = saleResult.Data.ResponseCode == "0000"
            ? fileAggregate.RecordFileLineAsSuccessful(command.LineNumber, saleResult.Data.TransactionId)
            : fileAggregate.RecordFileLineAsFailed(command.LineNumber, saleResult.Data.TransactionId, saleResult.Data.ResponseCode, saleResult.Data.ResponseMessage);

        ProcessingResult processingResult = saleResult.Data.ResponseCode == "0000"
            ? ProcessingResult.Successful
            : ProcessingResult.Failed;
        FileLineProcessingDiagnostics.LineStateDetermined(processingContext, processingResult);
        if (stateUpdateResult.IsSuccess)
            processingContext.Stage = FileLineProcessingStage.FileLineStatePersisted;

        return (stateUpdateResult, null);
    }

    private Result<Dictionary<String, String>> BuildTransactionMetadata(FileCommands.ProcessTransactionForFileLineCommand command,
                                                                        FileLine fileLine,
                                                                        FileProfileModel fileProfile,
                                                                        out String operatorName) {
        // need to now parse the line (based on the file format), this builds the metadata
        Dictionary<String, String> transactionMetadata = this.ParseFileLine(fileLine.LineData, fileProfile.FileFormatHandler);

        if (transactionMetadata.Any() == false) {
            // Line failed to parse so record this
            operatorName = null;
            return Result.Failure("Invalid Format");
        }

        // Add the file data to the request metadata
        transactionMetadata.Add("FileId", command.FileId.ToString());
        transactionMetadata.Add("FileLineNumber", fileLine.LineNumber.ToString());
        transactionMetadata.Add("EventId", command.EventId.ToString());

        operatorName = fileProfile.OperatorName;
        if (transactionMetadata.ContainsKey("OperatorName")) {
            // extract the value
            operatorName = transactionMetadata["OperatorName"];
            transactionMetadata = transactionMetadata.Where(x => x.Key != "OperatorName").ToDictionary(x => x.Key, x => x.Value);
        }

        return Result.Success(transactionMetadata);
    }

    private async Task<Result<SaleTransactionResponse>> SendSaleTransaction(FileDetails fileDetails,
                                                                            Result<(Guid ContractId, Guid OperatorId, Guid ProductId, String MerchantDevice)> detailsForTransaction,
                                                                            Dictionary<String, String> transactionMetadata,
                                                                            Int32 transactionNumber,
                                                                            FileLineProcessingContext processingContext,
                                                                            CancellationToken cancellationToken) {
        processingContext.Stage = FileLineProcessingStage.TokenAcquisition;
        Result<TokenResponse> getTokenResult = await this.GetToken(cancellationToken);
        if (getTokenResult.IsFailed) {
            return ResultHelpers.CreateFailure(getTokenResult);
        }

        this.TokenResponse = getTokenResult.Data;

        // Build a transaction request message
        SaleTransactionRequest saleTransactionRequest = new() {
            EstateId = fileDetails.EstateId,
            MerchantId = fileDetails.MerchantId,
            TransactionDateTime = fileDetails.FileReceivedDateTime,
            TransactionNumber = transactionNumber.ToString(CultureInfo.InvariantCulture),
            TransactionType = "Sale",
            ContractId = detailsForTransaction.Data.ContractId,
            DeviceIdentifier = detailsForTransaction.Data.MerchantDevice,
            OperatorId = detailsForTransaction.Data.OperatorId,
            ProductId = detailsForTransaction.Data.ProductId,
            AdditionalTransactionMetadata = transactionMetadata,
            TransactionSource = 2, // File based transaction
        };


        // Send request to transaction processor
        processingContext.Stage = FileLineProcessingStage.TransactionDispatchStarted;
        processingContext.TransactionDispatchAttempted = true;
        FileLineProcessingDiagnostics.TransactionDispatchStarted(processingContext);
        Result<SaleTransactionResponse> result;
        try
        {
            result = await this.TransactionProcessorClient.PerformTransaction(this.TokenResponse.AccessToken, saleTransactionRequest, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            processingContext.TransactionDispatchFailureType = exception is TimeoutException or TaskCanceledException
                ? "Timeout"
                : "SendFailure";
            return Result.Failure(exception.GetExceptionMessages());
        }

        if (result.IsFailed)
        {
            processingContext.TransactionDispatchFailureType = "SendFailure";
            FileLineProcessingDiagnostics.TransactionDispatchFailed(processingContext, result);
            return ResultHelpers.CreateFailure(result);
        }

        FileLineProcessingDiagnostics.TransactionDispatchCompleted(processingContext, result.Data);
        processingContext.Stage = FileLineProcessingStage.TransactionDispatchCompleted;

        return result;
    }

    private async Task<Result<(Guid ContractId, Guid OperatorId, Guid ProductId, String MerchantDevice)>> GetDetailsForTransaction(CancellationToken cancellationToken,
                                                                                                                                   FileDetails fileDetails,
                                                                                                                                   FileProfileModel fileProfile,
                                                                                                                                  String operatorName,
                                                                                                                                   FileLineProcessingContext processingContext) {

        processingContext.Stage = FileLineProcessingStage.TokenAcquisition;
        Result<TokenResponse> getTokenResult = await this.GetToken(cancellationToken);
        if (getTokenResult.IsFailed)
        {
            return ResultHelpers.CreateFailure(getTokenResult);
        }

        this.TokenResponse = getTokenResult.Data;

        // Get the merchant details
        processingContext.Stage = FileLineProcessingStage.MerchantLookup;
        Result<MerchantResponse> getMerchantResult = await this.TransactionProcessorClient.GetMerchant(this.TokenResponse.AccessToken, fileDetails.EstateId, fileDetails.MerchantId, cancellationToken);
        if (getMerchantResult.IsFailed) {
            return ResultHelpers.CreateFailure(getMerchantResult);
        }

        MerchantResponse merchant = getMerchantResult.Data;
        Result merchantResult = this.ValidateMerchant(merchant, fileDetails);
        if (merchantResult.IsFailed)
            return ResultHelpers.CreateFailure(merchantResult);

        processingContext.Stage = FileLineProcessingStage.ContractLookup;
        Result<List<ContractResponse>> getContractsResult = await this.TransactionProcessorClient.GetMerchantContracts(this.TokenResponse.AccessToken, fileDetails.EstateId, fileDetails.MerchantId, cancellationToken);
        if (getContractsResult.IsFailed) {
            return ResultHelpers.CreateFailure(getContractsResult);
        }

        List<ContractResponse> contracts = getContractsResult.Data;
        Result contractsResult = this.ValidateContracts(contracts, fileDetails);
        if (contractsResult.IsFailed)
            return ResultHelpers.CreateFailure(contractsResult);

        processingContext.Stage = FileLineProcessingStage.ContractOperatorMatch;
        Result<ContractResponse> contractResult = this.FindContract(contracts, fileProfile.OperatorName, operatorName, merchant.MerchantId);
        if (contractResult.IsFailed)
            return ResultHelpers.CreateFailure(contractResult);

        processingContext.Stage = FileLineProcessingStage.VariableValueProductLookup;
        Result<ContractProduct> productResult = this.FindVariableValueProduct(contractResult.Data, fileProfile.OperatorName, merchant.MerchantId);
        if (productResult.IsFailed)
            return ResultHelpers.CreateFailure(productResult);

        processingContext.Stage = FileLineProcessingStage.MerchantDeviceResolution;
        Result<String> merchantDeviceResult = this.ResolveMerchantDevice(merchant, fileDetails);
        if (merchantDeviceResult.IsFailed)
            return ResultHelpers.CreateFailure(merchantDeviceResult);

        return Result.Success((contractResult.Data.ContractId, contractResult.Data.OperatorId, productResult.Data.ProductId, merchantDeviceResult.Data));
    }

    private Result ValidateMerchant(MerchantResponse merchant,
                                     FileDetails fileDetails)
    {
        if (merchant == null)
            return Result.NotFound($"Merchant Id {fileDetails.MerchantId} not found on estate Id {fileDetails.EstateId}");

        return Result.Success();
    }

    private Result ValidateContracts(List<ContractResponse> contracts,
                                     FileDetails fileDetails)
    {
        if (contracts == null || contracts.Any() == false)
            return Result.NotFound($"No contracts found for Merchant Id {fileDetails.MerchantId} on estate Id {fileDetails.EstateId}");

        return Result.Success();
    }

    private Result<ContractResponse> FindContract(List<ContractResponse> contracts,
                                                  String profileOperatorName,
                                                  String operatorName,
                                                  Guid merchantId)
    {
        ContractResponse contract = profileOperatorName switch
        {
            "Voucher" => contracts.SingleOrDefault(c => c.Description.Contains(operatorName)),
            _ => contracts.SingleOrDefault(c => c.OperatorName == operatorName)
        };

        if (contract == null)
            return Result.NotFound($"No merchant contract for operator Id {operatorName} found for Merchant Id {merchantId}");

        return Result.Success(contract);
    }

    private Result<ContractProduct> FindVariableValueProduct(ContractResponse contract,
                                                              String profileOperatorName,
                                                              Guid merchantId)
    {
        ContractProduct product = contract.Products?.SingleOrDefault(p => p.Value == null);

        if (product == null)
            return Result.NotFound($"No variable value product found on the merchant contract for operator Id {profileOperatorName} and Merchant Id {merchantId}");

        return Result.Success(product);
    }

    private Result<String> ResolveMerchantDevice(MerchantResponse merchant,
                                                  FileDetails fileDetails)
    {
        if (merchant.Devices == null || merchant.Devices.Any() == false || String.IsNullOrWhiteSpace(merchant.Devices.First().Value))
            return Result.NotFound($"No valid merchant device found for Merchant Id {merchant.MerchantId} on estate Id {fileDetails.EstateId}");

        return Result.Success(merchant.Devices.First().Value);
    }

    private async Task<Result> ProcessFile(Guid fileId,
                                           Guid fileProfileId,
                                           Guid fileProfileVersionId,
                                           String fileName,
                                           CancellationToken cancellationToken) {
        Result<FileProfileModel> fileProfileResult =
            await this.GetFileProfileForProcessing(fileId, fileProfileId, fileProfileVersionId, cancellationToken);

        if (fileProfileResult.IsFailed)
            return ResultHelpers.CreateFailure(fileProfileResult);
        FileProfileModel fileProfile = fileProfileResult.Data;

        Result<IFileInfo> inProgressFileResult = this.ResolveInProgressFile(fileName, fileProfile);
        if (inProgressFileResult.IsFailed)
            return ResultHelpers.CreateFailure(inProgressFileResult);

        IFileInfo inProgressFile = inProgressFileResult.Data;

        Boolean fileWasRejected = false;
        Result result = await ApplyFileUpdates(async (FileAggregate fileAggregate) => {
            if (fileAggregate.GetFile().IsRejected)
            {
                fileWasRejected = true;
                return Result.Success();
            }

            Result contentResult = await this.ProcessFileContents(fileAggregate, inProgressFile, fileProfile, cancellationToken);
            fileWasRejected = fileAggregate.GetFile().IsRejected;
            return contentResult;
        }, fileId, cancellationToken);

        if (result.IsSuccess) {
            if (fileWasRejected)
            {
                Logger.LogWarning($"About to move rejected file {inProgressFile.Name} to [{fileProfile.FailedDirectory}]");
                inProgressFile.MoveTo($"{fileProfile.FailedDirectory}/{inProgressFile.Name}", true);
                return Result.Success();
            }

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

    private Result<IFileInfo> ResolveInProgressFile(String fileName, FileProfileModel fileProfile)
    {
        EnsureDirectoryExists(fileProfile.ProcessedDirectory);
        EnsureDirectoryExists(fileProfile.FailedDirectory);

        IFileInfo inProgressFile = this.FileSystem.FileInfo.New(fileName);
        if (inProgressFile.Exists)
            return Result.Success(inProgressFile);

        IFileInfo failedFileInfo = this.FileSystem.FileInfo.New($"{fileProfile.FailedDirectory}/{inProgressFile.Name}");
        IFileInfo processedFileInfo = this.FileSystem.FileInfo.New($"{fileProfile.ProcessedDirectory}/{inProgressFile.Name}");
        IFileInfo replayFile = (failedFileInfo.Exists, processedFileInfo.Exists) switch
        {
            (true, false) => failedFileInfo,
            (false, true) => processedFileInfo,
            _ => null
        };

        return replayFile == null
            ? Result.NotFound($"File {inProgressFile.FullName} not found")
            : Result.Success(replayFile);
    }

    private void EnsureDirectoryExists(String directory)
    {
        if (this.FileSystem.Directory.Exists(directory))
            return;

        Logger.LogInformation($"Creating Directory {directory} as not found");
        this.FileSystem.Directory.CreateDirectory(directory);
    }

    private async Task<Result> ProcessFileContents(FileAggregate fileAggregate,
                                                   IFileInfo inProgressFile,
                                                   FileProfileModel fileProfile,
                                                   CancellationToken cancellationToken)
    {
        String fileContent;
        using (Stream fs = inProgressFile.Open(FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read))
        using (StreamReader reader = new(fs))
            fileContent = await reader.ReadToEndAsync(cancellationToken);

        String[] fileLines = fileContent.Split(fileProfile.LineTerminator);
        IFileFormatHandler fileFormatHandler = this.FileFormatHandlerResolver(fileProfile.FileFormatHandler);
        Int32 detailRecordCount = fileLines.Count(fileLine =>
        {
            String trimmedLine = fileLine.Trim();
            return String.IsNullOrEmpty(trimmedLine) == false && fileFormatHandler.FileLineCanBeIgnored(trimmedLine) == false;
        });

        if (detailRecordCount > this.MaximumDetailRecordCount)
            return this.RejectFile(fileAggregate,
                                   detailRecordCount,
                                   MaximumDetailRecordsExceededCode,
                                   $"File contains {detailRecordCount} detail records; the maximum permitted is {this.MaximumDetailRecordCount}");

        FileValidationResult validationResult;
        using (StringReader validationReader = new(fileContent))
            validationResult = fileFormatHandler.ValidateFile(validationReader, fileProfile.LineTerminator);

        if (validationResult == null)
            return this.RejectFile(fileAggregate, detailRecordCount, "ValidationUnavailable", "File validation did not return a result");

        if (validationResult.IsValid == false)
            return this.RejectFile(fileAggregate,
                                   detailRecordCount,
                                   validationResult.Errors.First().Code,
                                   String.Join("; ", validationResult.Errors.Select(FormatValidationError)));

        return PersistFileLines(fileAggregate, fileLines);
    }

    private Result RejectFile(FileAggregate fileAggregate, Int32 detailRecordCount, String errorCode, String reason)
    {
        return fileAggregate.RejectFile(detailRecordCount,
                                        this.MaximumDetailRecordCount,
                                        errorCode,
                                        reason,
                                        DateTime.UtcNow);
    }

    private static Result PersistFileLines(FileAggregate fileAggregate, IEnumerable<String> fileLines)
    {
        HashSet<String> previouslyPersistedLines = fileAggregate.GetFile().FileLines
            .Select(fileLine => fileLine.LineData)
            .ToHashSet(StringComparer.Ordinal);

        foreach (String fileLine in fileLines)
        {
            String trimmedFileLine = fileLine.Trim();
            if (previouslyPersistedLines.Contains(trimmedFileLine))
                continue;

            Result stateResult = fileAggregate.AddFileLine(trimmedFileLine);
            if (stateResult.IsFailed)
                return stateResult;
        }

        return Result.Success();
    }

    private async Task<Result<FileProfileModel>> GetFileProfileForProcessing(Guid fileId,
                                                                               Guid fileProfileId,
                                                                               Guid fileProfileVersionId,
                                                                               CancellationToken cancellationToken)
    {
        if (fileProfileVersionId == Guid.Empty)
            return await this.FileProcessorManager.GetFileProfile(fileProfileId, cancellationToken);

        Result<FileProfileModel> versionResult = await this.FileProcessorManager.GetFileProfileVersion(fileProfileId,
                                                                                                         fileProfileVersionId,
                                                                                                         cancellationToken);
        if (versionResult.IsFailed == false)
            return versionResult;

        if (versionResult.Status == ResultStatus.NotFound)
            return Result.NotFound($"File [{fileId}] references unavailable file profile version [{fileProfileVersionId}] for profile [{fileProfileId}]");

        return ResultHelpers.CreateFailure(versionResult);
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

    private static String FormatValidationError(FileValidationError error)
    {
        return error switch
        {
            RowValidationError rowError => $"Line {rowError.LineNumber} Field {rowError.Field}: {rowError.Code} - {rowError.Reason}",
            _ => $"{error.Code} - {error.Reason}"
        };
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
