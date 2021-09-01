using System;

namespace FileProcessor.FileAggregate
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Runtime.InteropServices.ComTypes;
    using File.DomainEvents;
    using FIleProcessor.Models;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EventStore.Aggregate;
    using Shared.Exceptions;
    using Shared.General;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Shared.EventStore.Aggregate.Aggregate" />
    public class FileAggregate : Aggregate
    {
        /// <summary>
        /// The estate identifier
        /// </summary>
        private Guid EstateId;

        /// <summary>
        /// The merchant identifier
        /// </summary>
        private Guid MerchantId;

        /// <summary>
        /// The file profile identifier
        /// </summary>
        private Guid FileProfileId;

        /// <summary>
        /// The file import log identifier
        /// </summary>
        private Guid FileImportLogId;

        /// <summary>
        /// The file received date time
        /// </summary>
        private DateTime FileReceivedDateTime;

        /// <summary>
        /// The user identifier
        /// </summary>
        private Guid UserId;

        /// <summary>
        /// The file location
        /// </summary>
        private String FileLocation;

        /// <summary>
        /// The is completed
        /// </summary>
        private Boolean IsCompleted;

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
            Guard.ThrowIfInvalidGuid(aggregateId, "Aggregate Id cannot be an Empty Guid");

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

        /// <summary>
        /// Gets a value indicating whether this instance is created.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is created; otherwise, <c>false</c>.
        /// </value>
        public Boolean IsCreated { get; private set; }

        /// <summary>
        /// Plays the event.
        /// </summary>
        /// <param name="domainEvent">The domain event.</param>
        public override void PlayEvent(IDomainEvent domainEvent)
        {
            this.PlayEvent((dynamic)domainEvent);
        }

        /// <summary>
        /// Plays the event.
        /// </summary>
        /// <param name="domainEvent">The domain event.</param>
        private void PlayEvent(FileCreatedEvent domainEvent)
        {
            this.IsCreated = true;
            this.EstateId = domainEvent.EstateId;
            this.MerchantId = domainEvent.MerchantId;
            this.FileProfileId = domainEvent.FileProfileId;
            this.UserId = domainEvent.UserId;
            this.FileLocation = domainEvent.FileLocation;
            this.FileImportLogId = domainEvent.FileImportLogId;
            this.FileReceivedDateTime = domainEvent.FileReceivedDateTime;
        }

        /// <summary>
        /// The file lines
        /// </summary>
        private List<FileLine> FileLines;

        /// <summary>
        /// Gets the file.
        /// </summary>
        /// <returns></returns>
        public FileDetails GetFile()
        {
            return new FileDetails
                   {
                       ProcessingCompleted = this.IsCompleted,
                       FileLines = this.FileLines,
                       EstateId = this.EstateId,
                       MerchantId = this.MerchantId,
                       FileProfileId = this.FileProfileId,
                       FileId = this.AggregateId,
                       FileImportLogId = this.FileImportLogId,
                       FileLocation = this.FileLocation,
                       UserId = this.UserId,
                       ProcessingSummary = new ProcessingSummary
                                           {
                                               TotalLines = this.FileLines.Count,
                                               FailedLines = this.FileLines.Count(x => x.ProcessingResult == ProcessingResult.Failed),
                                               IgnoredLines = this.FileLines.Count(x => x.ProcessingResult == ProcessingResult.Ignored),
                                               NotProcessedLines = this.FileLines.Count(x => x.ProcessingResult == ProcessingResult.NotProcessed),
                                               SuccessfullyProcessedLines = this.FileLines.Count(x => x.ProcessingResult == ProcessingResult.Successful),
                                               RejectedLines = this.FileLines.Count(x => x.ProcessingResult == ProcessingResult.Rejected)
                       }
                   };
        }

        /// <summary>
        /// Plays the event.
        /// </summary>
        /// <param name="domainEvent">The domain event.</param>
        private void PlayEvent(FileLineAddedEvent domainEvent)
        {
            this.FileLines.Add(new FileLine
                               {
                                   LineData = domainEvent.FileLine,
                                   LineNumber = domainEvent.LineNumber
                               });
        }

        /// <summary>
        /// Plays the event.
        /// </summary>
        /// <param name="domainEvent">The domain event.</param>
        private void PlayEvent(FileLineProcessingIgnoredEvent domainEvent)
        {
            // find the line 
            FileLine fileLine = this.FileLines.Single(f => f.LineNumber == domainEvent.LineNumber);
            fileLine.ProcessingResult = ProcessingResult.Ignored;
        }

        private void PlayEvent(FileLineProcessingRejectedEvent domainEvent)
        {
            // find the line 
            FileLine fileLine = this.FileLines.Single(f => f.LineNumber == domainEvent.LineNumber);
            fileLine.ProcessingResult = ProcessingResult.Rejected;
            fileLine.RejectedReason = domainEvent.Reason;
        }

        /// <summary>
        /// Plays the event.
        /// </summary>
        /// <param name="domainEvent">The domain event.</param>
        private void PlayEvent(FileLineProcessingSuccessfulEvent domainEvent)
        {
            // find the line 
            FileLine fileLine = this.FileLines.Single(f => f.LineNumber == domainEvent.LineNumber);
            fileLine.TransactionId = domainEvent.TransactionId;
            fileLine.ProcessingResult = ProcessingResult.Successful;
        }

        /// <summary>
        /// Plays the event.
        /// </summary>
        /// <param name="domainEvent">The domain event.</param>
        private void PlayEvent(FileLineProcessingFailedEvent domainEvent)
        {
            FileLine fileLine = this.FileLines.Single(f => f.LineNumber == domainEvent.LineNumber);
            fileLine.TransactionId = domainEvent.TransactionId;
            fileLine.ProcessingResult = ProcessingResult.Failed;
        }

        /// <summary>
        /// Plays the event.
        /// </summary>
        /// <param name="domainEvent">The domain event.</param>
        private void PlayEvent(FileProcessingCompletedEvent domainEvent)
        {
            this.IsCompleted = true;
        }

        /// <summary>
        /// Uploads the file.
        /// </summary>
        /// <param name="fileImportLogId">The file import log identifier.</param>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="merchantId">The merchant identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="fileProfileId">The file profile identifier.</param>
        /// <param name="fileLocation">The file location.</param>
        /// <param name="fileReceivedDateTime">The file received date time.</param>
        /// <exception cref="InvalidOperationException">File Id {this.AggregateId} has already been created</exception>
        /// <exception cref="System.InvalidOperationException">File Id {this.AggregateId} has already been uploaded</exception>
        public void CreateFile(Guid fileImportLogId, Guid estateId, Guid merchantId, Guid userId, Guid fileProfileId, String fileLocation, DateTime fileReceivedDateTime)
        {
            if (this.IsCreated)
                return;

            FileCreatedEvent fileCreatedEvent = new FileCreatedEvent(this.AggregateId, fileImportLogId, estateId, merchantId, userId, fileProfileId, fileLocation, fileReceivedDateTime);

            this.ApplyAndAppend(fileCreatedEvent);
        }

        /// <summary>
        /// Adds the file line.
        /// </summary>
        /// <param name="fileLine">The file line.</param>
        /// <exception cref="InvalidOperationException">File Id {this.AggregateId} has not been uploaded yet</exception>
        public void AddFileLine(String fileLine)
        {
            if (this.IsCreated == false)
            {
                throw new InvalidOperationException($"File Id {this.AggregateId} has not been uploaded yet");
            }

            Int32 lineNumber = this.FileLines.Count + 1;
            
            FileLineAddedEvent fileLineAddedEvent = new FileLineAddedEvent(this.AggregateId, this.EstateId, lineNumber, fileLine);
            this.ApplyAndAppend(fileLineAddedEvent);
        }

        /// <summary>
        /// Records the file line as ignored.
        /// </summary>
        /// <param name="lineNumber">The line number.</param>
        /// <exception cref="InvalidOperationException">File has no lines to mark as successful</exception>
        /// <exception cref="NotFoundException">File line with number {lineNumber} not found to mark as successful</exception>
        public void RecordFileLineAsIgnored(Int32 lineNumber)
        {
            if (this.FileLines.Any() == false)
            {
                throw new InvalidOperationException("File has no lines to mark as ignored");
            }

            if (this.FileLines.SingleOrDefault(l => l.LineNumber == lineNumber) == null)
            {
                throw new NotFoundException($"File line with number {lineNumber} not found to mark as ignored");
            }

            FileLineProcessingIgnoredEvent fileLineProcessingIgnoredEvent =
                new FileLineProcessingIgnoredEvent(this.AggregateId, this.EstateId, lineNumber);

            this.ApplyAndAppend(fileLineProcessingIgnoredEvent);

            this.CompletedChecks();
        }

        /// <summary>
        /// Records the file line as rejected.
        /// </summary>
        /// <param name="lineNumber">The line number.</param>
        /// <param name="reason">The reason.</param>
        /// <exception cref="InvalidOperationException">File has no lines to mark as rejected</exception>
        /// <exception cref="NotFoundException">File line with number {lineNumber} not found to mark as rejected</exception>
        public void RecordFileLineAsRejected(Int32 lineNumber, String reason)
        {
            if (this.FileLines.Any() == false)
            {
                throw new InvalidOperationException("File has no lines to mark as rejected");
            }

            if (this.FileLines.SingleOrDefault(l => l.LineNumber == lineNumber) == null)
            {
                throw new NotFoundException($"File line with number {lineNumber} not found to mark as rejected");
            }

            FileLineProcessingRejectedEvent fileLineProcessingRejectedEvent =
                new FileLineProcessingRejectedEvent(this.AggregateId, this.EstateId, lineNumber, reason);

            this.ApplyAndAppend(fileLineProcessingRejectedEvent);

            this.CompletedChecks();
        }

        /// <summary>
        /// Records the file line as successful.
        /// </summary>
        /// <param name="lineNumber">The line number.</param>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <exception cref="InvalidOperationException">File has no lines to mark as successful</exception>
        /// <exception cref="NotFoundException">File line with number {lineNumber} not found to mark as successful</exception>
        public void RecordFileLineAsSuccessful(Int32 lineNumber, Guid transactionId)
        {
            if (this.FileLines.Any() == false)
            {
                throw new InvalidOperationException("File has no lines to mark as successful");
            }

            if (this.FileLines.SingleOrDefault(l => l.LineNumber == lineNumber) == null)
            {
                throw new NotFoundException($"File line with number {lineNumber} not found to mark as successful");
            }

            FileLineProcessingSuccessfulEvent fileLineProcessingSuccessfulEvent =
                new FileLineProcessingSuccessfulEvent(this.AggregateId, this.EstateId, lineNumber, transactionId);

            this.ApplyAndAppend(fileLineProcessingSuccessfulEvent);

            this.CompletedChecks();
        }

        /// <summary>
        /// Records the file line as failed.
        /// </summary>
        /// <param name="lineNumber">The line number.</param>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <param name="responseCode">The response code.</param>
        /// <param name="responseMessage">The response message.</param>
        /// <exception cref="InvalidOperationException">File has no lines to mark as failed</exception>
        /// <exception cref="NotFoundException">File line with number {lineNumber} not found to mark as failed</exception>
        public void RecordFileLineAsFailed(Int32 lineNumber, Guid transactionId, String responseCode, String responseMessage)
        {
            if (this.FileLines.Any() == false)
            {
                throw new InvalidOperationException("File has no lines to mark as failed");
            }

            if (this.FileLines.SingleOrDefault(l => l.LineNumber == lineNumber) == null)
            {
                throw new NotFoundException($"File line with number {lineNumber} not found to mark as failed");
            }

            FileLineProcessingFailedEvent fileLineProcessingFailedEvent =
                new FileLineProcessingFailedEvent(this.AggregateId, this.EstateId, lineNumber, transactionId, responseCode,responseMessage);

            this.ApplyAndAppend(fileLineProcessingFailedEvent);

            this.CompletedChecks();
        }

        /// <summary>
        /// Completeds the checks.
        /// </summary>
        private void CompletedChecks()
        {
            if (this.FileLines.Any(f => f.ProcessingResult == ProcessingResult.NotProcessed) == false)
            {
                // All lines have been processed, write out a completed event
                FileProcessingCompletedEvent fileProcessingCompletedEvent = new FileProcessingCompletedEvent(this.AggregateId, this.EstateId, DateTime.Now);

                this.ApplyAndAppend(fileProcessingCompletedEvent);
            }
        }
    }
}
