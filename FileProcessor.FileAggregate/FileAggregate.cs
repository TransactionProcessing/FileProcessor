using System;
using FileProcessor.Models;
using SimpleResults;

namespace FileProcessor.FileAggregate
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Runtime.InteropServices.ComTypes;
    using File.DomainEvents;
    using FileProcessor.Models;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EventStore.Aggregate;
    using Shared.Exceptions;
    using Shared.General;

    public static class FileAggregateExtensions{
        public static void PlayEvent(this FileAggregate aggregate, FileCreatedEvent domainEvent)
        {
            aggregate.IsCreated = true;
            aggregate.EstateId = domainEvent.EstateId;
            aggregate.MerchantId = domainEvent.MerchantId;
            aggregate.FileProfileId = domainEvent.FileProfileId;
            aggregate.UserId = domainEvent.UserId;
            aggregate.FileLocation = domainEvent.FileLocation;
            aggregate.FileImportLogId = domainEvent.FileImportLogId;
            aggregate.FileReceivedDateTime = domainEvent.FileReceivedDateTime;
        }

        public static FileDetails GetFile(this FileAggregate aggregate)
        {
            return new FileDetails
            {
                FileReceivedDateTime = aggregate.FileReceivedDateTime,
                ProcessingCompleted = aggregate.IsCompleted,
                FileLines = aggregate.FileLines,
                EstateId = aggregate.EstateId,
                MerchantId = aggregate.MerchantId,
                FileProfileId = aggregate.FileProfileId,
                FileId = aggregate.AggregateId,
                FileImportLogId = aggregate.FileImportLogId,
                FileLocation = aggregate.FileLocation,
                UserId = aggregate.UserId,
                ProcessingSummary = new ProcessingSummary
                {
                    TotalLines = aggregate.FileLines.Count,
                    FailedLines = aggregate.FileLines.Count(x => x.ProcessingResult == ProcessingResult.Failed),
                    IgnoredLines = aggregate.FileLines.Count(x => x.ProcessingResult == ProcessingResult.Ignored),
                    NotProcessedLines = aggregate.FileLines.Count(x => x.ProcessingResult == ProcessingResult.NotProcessed),
                    SuccessfullyProcessedLines = aggregate.FileLines.Count(x => x.ProcessingResult == ProcessingResult.Successful),
                    RejectedLines = aggregate.FileLines.Count(x => x.ProcessingResult == ProcessingResult.Rejected)
                }
            };
        }

        public static void PlayEvent(this FileAggregate aggregate, FileLineAddedEvent domainEvent)
        {
            aggregate.FileLines.Add(new FileLine
                                    {
                                        LineData = domainEvent.FileLine,
                                        LineNumber = domainEvent.LineNumber
                                    });
        }

        public static void PlayEvent(this FileAggregate aggregate, FileLineProcessingIgnoredEvent domainEvent)
        {
            // find the line 
            FileLine fileLine = aggregate.FileLines.Single(f => f.LineNumber == domainEvent.LineNumber);
            fileLine.ProcessingResult = ProcessingResult.Ignored;
        }

        public static void PlayEvent(this FileAggregate aggregate, FileLineProcessingRejectedEvent domainEvent)
        {
            // find the line 
            FileLine fileLine = aggregate.FileLines.Single(f => f.LineNumber == domainEvent.LineNumber);
            fileLine.ProcessingResult = ProcessingResult.Rejected;
            fileLine.RejectedReason = domainEvent.Reason;
        }

        public static void PlayEvent(this FileAggregate aggregate, FileLineProcessingSuccessfulEvent domainEvent)
        {
            // find the line 
            FileLine fileLine = aggregate.FileLines.Single(f => f.LineNumber == domainEvent.LineNumber);
            fileLine.TransactionId = domainEvent.TransactionId;
            fileLine.ProcessingResult = ProcessingResult.Successful;
        }
        
        public static void PlayEvent(this FileAggregate aggregate, FileLineProcessingFailedEvent domainEvent)
        {
            
            FileLine fileLine = aggregate.FileLines.Single(f => f.LineNumber == domainEvent.LineNumber);
            fileLine.TransactionId = domainEvent.TransactionId;
            fileLine.ProcessingResult = ProcessingResult.Failed;
        }

        public static void PlayEvent(this FileAggregate aggregate, FileProcessingCompletedEvent domainEvent)
        {
            aggregate.IsCompleted = true;
        }

        public static Result CreateFile(this FileAggregate aggregate, Guid fileImportLogId, Guid estateId, Guid merchantId, Guid userId, Guid fileProfileId, String fileLocation, DateTime fileReceivedDateTime, Guid operatorId)
        {
            if (aggregate.IsCreated)
                return Result.Success();

            FileCreatedEvent fileCreatedEvent = new(aggregate.AggregateId, fileImportLogId, estateId, merchantId, userId, fileProfileId, fileLocation, fileReceivedDateTime, operatorId);

            aggregate.ApplyAndAppend(fileCreatedEvent);

            return Result.Success();
        }

        public static Result AddFileLine(this FileAggregate aggregate, String fileLine)
        {
            if (aggregate.IsCreated == false) {
                return Result.Invalid($"File Id {aggregate.AggregateId} has not been uploaded yet");
            }

            Boolean lineAlreadyExists = aggregate.FileLines.Any(f => f.LineData == fileLine);

            // We already have this line so just return
            if (lineAlreadyExists)
                return Result.Success();

            Int32 lineNumber = aggregate.FileLines.Count + 1;

            FileLineAddedEvent fileLineAddedEvent = new(aggregate.AggregateId, aggregate.EstateId, aggregate.MerchantId, lineNumber, fileLine);
            aggregate.ApplyAndAppend(fileLineAddedEvent);
            return Result.Success();
        }

        public static Result RecordFileLineAsIgnored(this FileAggregate aggregate, Int32 lineNumber)
        {
            if (aggregate.FileLines.Any() == false)
            {
                return Result.Invalid("File has no lines to mark as ignored");
            }

            if (aggregate.FileLines.SingleOrDefault(l => l.LineNumber == lineNumber) == null)
            {
                return Result.NotFound($"File line with number {lineNumber} not found to mark as ignored");
            }

            FileLineProcessingIgnoredEvent fileLineProcessingIgnoredEvent = new(aggregate.AggregateId, aggregate.EstateId, aggregate.MerchantId, lineNumber);

            aggregate.ApplyAndAppend(fileLineProcessingIgnoredEvent);

            aggregate.CompletedChecks();

            return Result.Success();
        }

        public static Result RecordFileLineAsRejected(this FileAggregate aggregate, Int32 lineNumber, String reason)
        {
            if (aggregate.FileLines.Any() == false)
            {
                return Result.Invalid("File has no lines to mark as rejected");
            }

            if (aggregate.FileLines.SingleOrDefault(l => l.LineNumber == lineNumber) == null)
            {
                return Result.NotFound($"File line with number {lineNumber} not found to mark as rejected");
            }

            FileLineProcessingRejectedEvent fileLineProcessingRejectedEvent = new(aggregate.AggregateId, aggregate.EstateId, aggregate.MerchantId, lineNumber, reason);

            aggregate.ApplyAndAppend(fileLineProcessingRejectedEvent);

            aggregate.CompletedChecks();

            return Result.Success();
        }

        public static Result RecordFileLineAsSuccessful(this FileAggregate aggregate, Int32 lineNumber, Guid transactionId)
        {
            if (aggregate.FileLines.Any() == false)
            {
                return Result.Invalid("File has no lines to mark as successful");
            }

            if (aggregate.FileLines.SingleOrDefault(l => l.LineNumber == lineNumber) == null)
            {
                return Result.NotFound($"File line with number {lineNumber} not found to mark as successful");
            }

            FileLineProcessingSuccessfulEvent fileLineProcessingSuccessfulEvent = new(aggregate.AggregateId, aggregate.EstateId, aggregate.MerchantId, lineNumber, transactionId);

            aggregate.ApplyAndAppend(fileLineProcessingSuccessfulEvent);

            aggregate.CompletedChecks();

            return Result.Success();
        }
        
        public static Result RecordFileLineAsFailed(this FileAggregate aggregate,Int32 lineNumber, Guid transactionId, String responseCode, String responseMessage)
        {
            if (aggregate.FileLines.Any() == false)
            {
                return Result.Invalid("File has no lines to mark as failed");
            }

            if (aggregate.FileLines.SingleOrDefault(l => l.LineNumber == lineNumber) == null)
            {
                return Result.NotFound($"File line with number {lineNumber} not found to mark as failed");
            }

            FileLineProcessingFailedEvent fileLineProcessingFailedEvent = new(aggregate.AggregateId, aggregate.EstateId, aggregate.MerchantId, lineNumber, transactionId, responseCode, responseMessage);

            aggregate.ApplyAndAppend(fileLineProcessingFailedEvent);

            aggregate.CompletedChecks();

            return Result.Success();
        }

        /// <summary>
        /// Completeds the checks.
        /// </summary>
        private static void CompletedChecks(this FileAggregate aggregate)
        {
            if (aggregate.FileLines.Any(f => f.ProcessingResult == ProcessingResult.NotProcessed) == false)
            {
                // All lines have been processed, write out a completed event
                FileProcessingCompletedEvent fileProcessingCompletedEvent = new FileProcessingCompletedEvent(aggregate.AggregateId, aggregate.EstateId, aggregate.MerchantId, DateTime.Now);

                aggregate.ApplyAndAppend(fileProcessingCompletedEvent);
            }
        }
    }

    /// <seealso cref="Shared.EventStore.Aggregate.Aggregate" />
    public record FileAggregate : Aggregate
    {
        internal Guid EstateId;

        internal Guid MerchantId;

        internal Guid FileProfileId;

        internal Guid FileImportLogId;

        internal DateTime FileReceivedDateTime;

        internal Guid UserId;

        internal String FileLocation;

        internal Boolean IsCompleted;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileAggregate" /> class.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public FileAggregate()
        {
            // Nothing here
            this.FileLines = new List<FileLine>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileAggregate" /> class.
        /// </summary>
        /// <param name="aggregateId">The aggregate identifier.</param>
        private FileAggregate(Guid aggregateId)
        {
            if (aggregateId== Guid.Empty)
                throw new ArgumentException("Aggregate Id cannot be an Empty Guid");

            this.AggregateId = aggregateId;
            this.FileLines = new List<FileLine>();
        }

        /// <summary>
        /// Creates the specified aggregate identifier.
        /// </summary>
        /// <param name="aggregateId">The aggregate identifier.</param>
        /// <returns></returns>
        public static FileAggregate Create(Guid aggregateId)
        {
            return new FileAggregate(aggregateId);
        }

        /// <summary>
        /// Gets the metadata.
        /// </summary>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        protected override Object GetMetadata()
        {
            return null;
        }

        public Boolean IsCreated{ get; internal set; }

        /// <summary>
        /// Plays the event.
        /// </summary>
        /// <param name="domainEvent">The domain event.</param>
        public override void PlayEvent(IDomainEvent domainEvent) => FileAggregateExtensions.PlayEvent(this, (dynamic)domainEvent);
        
        internal List<FileLine> FileLines;
        
        
    }
}
