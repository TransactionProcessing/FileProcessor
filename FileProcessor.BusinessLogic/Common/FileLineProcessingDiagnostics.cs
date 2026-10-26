using System;
using FileProcessor.BusinessLogic.Requests;
using FileProcessor.File.DomainEvents;
using FileProcessor.FileAggregate;
using FileProcessor.Models;
using Shared.Logger;
using SimpleResults;
using TransactionProcessor.DataTransferObjects;

namespace FileProcessor.BusinessLogic.Common;

internal enum FileLineProcessingStage
{
    FileAggregateLoad,
    FileLineLookup,
    FileProfileLookup,
    LineIgnoreEvaluation,
    TransactionDetailsParsing,
    TokenAcquisition,
    MerchantLookup,
    ContractLookup,
    ContractOperatorMatch,
    VariableValueProductLookup,
    MerchantDeviceResolution,
    TransactionDetailsResolved,
    TransactionDispatchStarted,
    TransactionDispatchCompleted,
    TransactionDispatchAttemptPersisted,
    FileLineStateUpdate,
    FileLineStatePersisted
}

internal sealed class FileLineProcessingContext
{
    public FileLineProcessingContext(FileCommands.ProcessTransactionForFileLineCommand command)
    {
        this.Command = command;
    }

    public FileCommands.ProcessTransactionForFileLineCommand Command { get; }

    public FileLineProcessingStage Stage { get; set; } = FileLineProcessingStage.FileAggregateLoad;

    public Boolean TransactionDispatchAttempted { get; set; }

    public Boolean TransactionDispatchSucceeded { get; set; }

    public String TransactionDispatchFailureType { get; set; }

    public Guid EstateId { get; set; }

    public Guid MerchantId { get; set; }

    public Guid FileProfileId { get; set; }

    public String OperatorName { get; set; }

    public String ResponseCode { get; set; }
}

internal static class FileLineProcessingDiagnostics
{
    public static void LineProcessingStarted(FileLineProcessingContext context)
    {
        Logger.LogInformation(
            "line-processing-started FileId={FileId} LineNumber={LineNumber} EstateId={EstateId} MerchantId={MerchantId} FileProfileId={FileProfileId}",
            context.Command.FileId,
            context.Command.LineNumber,
            context.EstateId,
            context.MerchantId,
            context.FileProfileId);
    }

    public static void LineProcessingSkipped(FileLineProcessingContext context, ProcessingResult processingResult)
    {
        Logger.LogInformation(
            "file-line-processing-skipped FileId={FileId} LineNumber={LineNumber} ProcessingResult={ProcessingResult}",
            context.Command.FileId,
            context.Command.LineNumber,
            processingResult);
    }

    public static void LineIgnored(FileLineProcessingContext context)
    {
        Logger.LogInformation(
            "file-line-ignored FileId={FileId} LineNumber={LineNumber} EstateId={EstateId} MerchantId={MerchantId} Operator={Operator} Reason={Reason}",
            context.Command.FileId,
            context.Command.LineNumber,
            context.EstateId,
            context.MerchantId,
            context.OperatorName,
            "Ignored by file format handler");
    }

    public static void LineRejected(FileLineProcessingContext context, String reason)
    {
        Logger.LogWarning(
            "file-line-rejected FileId={FileId} LineNumber={LineNumber} EstateId={EstateId} MerchantId={MerchantId} Operator={Operator} Reason={Reason}",
            context.Command.FileId,
            context.Command.LineNumber,
            context.EstateId,
            context.MerchantId,
            context.OperatorName,
            reason);
    }

    public static void LineStateDetermined(FileLineProcessingContext context, ProcessingResult processingResult)
    {
        Logger.LogInformation(
            "file-line-state-determined FileId={FileId} LineNumber={LineNumber} ProcessingResult={ProcessingResult} TransactionDispatchAttempted={TransactionDispatchAttempted} TransactionDispatchSucceeded={TransactionDispatchSucceeded} ResponseCode={ResponseCode}",
            context.Command.FileId,
            context.Command.LineNumber,
            processingResult,
            context.TransactionDispatchAttempted,
            context.TransactionDispatchSucceeded,
            context.ResponseCode);
    }

    public static void ProcessingFailed(FileLineProcessingContext context, Result result)
    {
        String stage = context.Stage == FileLineProcessingStage.FileLineStatePersisted
            ? "file-line-state-persisted/failed"
            : GetStageName(context.Stage);
        Logger.LogError(
            "{Stage} failed FileId={FileId} LineNumber={LineNumber} EstateId={EstateId} MerchantId={MerchantId} FileProfileId={FileProfileId} Operator={Operator} TransactionDispatchAttempted={TransactionDispatchAttempted} TransactionDispatchSucceeded={TransactionDispatchSucceeded} ResponseCode={ResponseCode} ResultStatus={ResultStatus} Error={Error}",
            stage,
            context.Command.FileId,
            context.Command.LineNumber,
            context.EstateId,
            context.MerchantId,
            context.FileProfileId,
            context.OperatorName,
            context.TransactionDispatchAttempted,
            context.TransactionDispatchSucceeded,
            context.ResponseCode,
            result.Status,
            result.Message);
    }

    public static void LineStatePersisted(FileLineProcessingContext context)
    {
        Logger.LogInformation(
            "file-line-state-persisted FileId={FileId} LineNumber={LineNumber} EstateId={EstateId} MerchantId={MerchantId} FileProfileId={FileProfileId} Operator={Operator} TransactionDispatchAttempted={TransactionDispatchAttempted} TransactionDispatchSucceeded={TransactionDispatchSucceeded} ResponseCode={ResponseCode}",
            context.Command.FileId,
            context.Command.LineNumber,
            context.EstateId,
            context.MerchantId,
            context.FileProfileId,
            context.OperatorName,
            context.TransactionDispatchAttempted,
            context.TransactionDispatchSucceeded,
            context.ResponseCode);
    }

    public static void TransactionDispatchStarted(FileLineProcessingContext context)
    {
        Logger.LogInformation(
            "transaction-dispatch-started FileId={FileId} LineNumber={LineNumber} EstateId={EstateId} MerchantId={MerchantId} Operator={Operator} ClientOperation={ClientOperation}",
            context.Command.FileId,
            context.Command.LineNumber,
            context.EstateId,
            context.MerchantId,
            context.OperatorName,
            "PerformTransaction");
    }

    public static void TransactionDispatchFailed(FileLineProcessingContext context, Result<SaleTransactionResponse> result)
    {
        Logger.LogError(
            "transaction-dispatch-failed FileId={FileId} LineNumber={LineNumber} EstateId={EstateId} MerchantId={MerchantId} Operator={Operator} ClientOperation={ClientOperation} ResultStatus={ResultStatus} Error={Error}",
            context.Command.FileId,
            context.Command.LineNumber,
            context.EstateId,
            context.MerchantId,
            context.OperatorName,
            "PerformTransaction",
            result.Status,
            result.Message);
    }

    public static void TransactionDispatchCompleted(FileLineProcessingContext context, SaleTransactionResponse response)
    {
        Logger.LogInformation(
            "transaction-dispatch-completed FileId={FileId} LineNumber={LineNumber} EstateId={EstateId} MerchantId={MerchantId} Operator={Operator} ClientOperation={ClientOperation} TransactionId={TransactionId} ResponseCode={ResponseCode} ResponseMessage={ResponseMessage}",
            context.Command.FileId,
            context.Command.LineNumber,
            context.EstateId,
            context.MerchantId,
            context.OperatorName,
            "PerformTransaction",
            response.TransactionId,
            response.ResponseCode,
            response.ResponseMessage);
    }

    public static void EventHandlerDispatchFailed(FileLineAddedEvent domainEvent, Result result)
    {
        Logger.LogError(
            "event-handler-dispatch-failed FileId={FileId} LineNumber={LineNumber} EstateId={EstateId} MerchantId={MerchantId} EventType={EventType} ResultStatus={ResultStatus} Error={Error}",
            domainEvent.FileId,
            domainEvent.LineNumber,
            domainEvent.EstateId,
            domainEvent.MerchantId,
            nameof(FileLineAddedEvent),
            result.Status,
            result.Message);
    }

    public static void EventHandlerDispatchCompleted(FileLineAddedEvent domainEvent)
    {
        Logger.LogDebug(
            "event-handler-dispatch-completed FileId={FileId} LineNumber={LineNumber} EstateId={EstateId} MerchantId={MerchantId} EventType={EventType}",
            domainEvent.FileId,
            domainEvent.LineNumber,
            domainEvent.EstateId,
            domainEvent.MerchantId,
            nameof(FileLineAddedEvent));
    }

    public static void EventHandlerDispatchThrew(FileLineAddedEvent domainEvent, Exception exception)
    {
        Logger.LogError(exception,
            "event-handler-dispatch-threw FileId={FileId} LineNumber={LineNumber} EstateId={EstateId} MerchantId={MerchantId} EventType={EventType}",
            domainEvent.FileId,
            domainEvent.LineNumber,
            domainEvent.EstateId,
            domainEvent.MerchantId,
            nameof(FileLineAddedEvent));
    }

    private static String GetStageName(FileLineProcessingStage stage) => stage switch
    {
        FileLineProcessingStage.FileAggregateLoad => "file-aggregate-load",
        FileLineProcessingStage.FileLineLookup => "file-line-lookup",
        FileLineProcessingStage.FileProfileLookup => "file-profile-lookup",
        FileLineProcessingStage.LineIgnoreEvaluation => "line-ignore-evaluation",
        FileLineProcessingStage.TransactionDetailsParsing => "transaction-details-parsing",
        FileLineProcessingStage.TokenAcquisition => "token-acquisition",
        FileLineProcessingStage.MerchantLookup => "merchant-lookup",
        FileLineProcessingStage.ContractLookup => "contract-lookup",
        FileLineProcessingStage.ContractOperatorMatch => "contract-operator-match",
        FileLineProcessingStage.VariableValueProductLookup => "variable-value-product-lookup",
        FileLineProcessingStage.MerchantDeviceResolution => "merchant-device-resolution",
        FileLineProcessingStage.TransactionDetailsResolved => "transaction-details-resolved",
        FileLineProcessingStage.TransactionDispatchStarted => "transaction-dispatch-started",
        FileLineProcessingStage.TransactionDispatchCompleted => "transaction-dispatch-completed",
        FileLineProcessingStage.TransactionDispatchAttemptPersisted => "transaction-dispatch-attempt-persisted",
        FileLineProcessingStage.FileLineStateUpdate => "file-line-state-update",
        FileLineProcessingStage.FileLineStatePersisted => "file-line-state-persisted",
        _ => throw new ArgumentOutOfRangeException(nameof(stage), stage, "Unknown file-line processing stage")
    };
}
