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
        Logger.LogInformation($"line-processing-started FileId={context.Command.FileId} LineNumber={context.Command.LineNumber} EstateId={context.EstateId} MerchantId={context.MerchantId} FileProfileId={context.FileProfileId}");
    }

    public static void LineProcessingSkipped(FileLineProcessingContext context, ProcessingResult processingResult)
    {
        Logger.LogInformation($"file-line-processing-skipped FileId={context.Command.FileId} LineNumber={context.Command.LineNumber} ProcessingResult={processingResult}");
    }

    public static void LineIgnored(FileLineProcessingContext context)
    {
        Logger.LogInformation($"file-line-ignored FileId={context.Command.FileId} LineNumber={context.Command.LineNumber} EstateId={context.EstateId} MerchantId={context.MerchantId} Operator={context.OperatorName} Reason=Ignored by file format handler");
    }

    public static void LineRejected(FileLineProcessingContext context, String reason)
    {
        Logger.LogWarning($"file-line-rejected FileId={context.Command.FileId} LineNumber={context.Command.LineNumber} EstateId={context.EstateId} MerchantId={context.MerchantId} Operator={context.OperatorName} Reason={reason}");
    }

    public static void LineStateDetermined(FileLineProcessingContext context, ProcessingResult processingResult)
    {
        Logger.LogInformation($"file-line-state-determined FileId={context.Command.FileId} LineNumber={context.Command.LineNumber} ProcessingResult={processingResult} TransactionDispatchAttempted={context.TransactionDispatchAttempted} TransactionDispatchSucceeded={context.TransactionDispatchSucceeded} ResponseCode={context.ResponseCode}");
    }

    public static void ProcessingFailed(FileLineProcessingContext context, Result result)
    {
        String stage = context.Stage == FileLineProcessingStage.FileLineStatePersisted
            ? "file-line-state-persisted/failed"
            : GetStageName(context.Stage);
        Logger.LogError($"{stage} failed FileId={context.Command.FileId} LineNumber={context.Command.LineNumber} EstateId={context.EstateId} MerchantId={context.MerchantId} FileProfileId={context.FileProfileId} Operator={context.OperatorName} TransactionDispatchAttempted={context.TransactionDispatchAttempted} TransactionDispatchSucceeded={context.TransactionDispatchSucceeded} ResponseCode={context.ResponseCode} ResultStatus={result.Status} Error={result.Message}");
    }

    public static void LineStatePersisted(FileLineProcessingContext context)
    {
        Logger.LogInformation($"file-line-state-persisted FileId={context.Command.FileId} LineNumber={context.Command.LineNumber} EstateId={context.EstateId} MerchantId={context.MerchantId} FileProfileId={context.FileProfileId} Operator={context.OperatorName} TransactionDispatchAttempted={context.TransactionDispatchAttempted} TransactionDispatchSucceeded={context.TransactionDispatchSucceeded} ResponseCode={context.ResponseCode}");
    }

    public static void TransactionDispatchStarted(FileLineProcessingContext context)
    {
        Logger.LogInformation($"transaction-dispatch-started FileId={context.Command.FileId} LineNumber={context.Command.LineNumber} EstateId={context.EstateId} MerchantId={context.MerchantId} Operator={context.OperatorName} ClientOperation=PerformTransaction");
    }

    public static void TransactionDispatchFailed(FileLineProcessingContext context, Result<SaleTransactionResponse> result)
    {
        Logger.LogError($"transaction-dispatch-failed FileId={context.Command.FileId} LineNumber={context.Command.LineNumber} EstateId={context.EstateId} MerchantId={context.MerchantId} Operator={context.OperatorName} ClientOperation=PerformTransaction ResultStatus={result.Status} Error={result.Message}");
    }

    public static void TransactionDispatchCompleted(FileLineProcessingContext context, SaleTransactionResponse response)
    {
        Logger.LogInformation($"transaction-dispatch-completed FileId={context.Command.FileId} LineNumber={context.Command.LineNumber} EstateId={context.EstateId} MerchantId={context.MerchantId} Operator={context.OperatorName} ClientOperation=PerformTransaction TransactionId={response.TransactionId} ResponseCode={response.ResponseCode} ResponseMessage={response.ResponseMessage}");
    }

    public static void EventHandlerDispatchFailed(FileLineAddedEvent domainEvent, Result result)
    {
        Logger.LogError($"event-handler-dispatch-failed FileId={domainEvent.FileId} LineNumber={domainEvent.LineNumber} EstateId={domainEvent.EstateId} MerchantId={domainEvent.MerchantId} EventType={nameof(FileLineAddedEvent)} ResultStatus={result.Status} Error={result.Message}");
    }

    public static void EventHandlerDispatchCompleted(FileLineAddedEvent domainEvent)
    {
        Logger.LogDebug($"event-handler-dispatch-completed FileId={domainEvent.FileId} LineNumber={domainEvent.LineNumber} EstateId={domainEvent.EstateId} MerchantId={domainEvent.MerchantId} EventType={nameof(FileLineAddedEvent)}");
    }

    public static void EventHandlerDispatchThrew(FileLineAddedEvent domainEvent, Exception exception)
    {
        Logger.LogError($"event-handler-dispatch-threw FileId={domainEvent.FileId} LineNumber={domainEvent.LineNumber} EstateId={domainEvent.EstateId} MerchantId={domainEvent.MerchantId} EventType={nameof(FileLineAddedEvent)} Exception={exception}");
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
