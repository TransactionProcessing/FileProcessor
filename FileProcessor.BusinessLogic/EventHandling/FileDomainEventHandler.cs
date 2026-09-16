using System.Threading.Tasks;
using System;
using Shared.Logger;
using SimpleResults;
using Microsoft.Extensions.Logging;

namespace FileProcessor.BusinessLogic.EventHandling
{
    using System.Threading;
    using File.DomainEvents;
    using FileImportLog.DomainEvents;
    using MediatR;
    using Requests;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EventStore.EventHandling;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Shared.EventStore.EventHandling.IDomainEventHandler" />
    public class FileDomainEventHandler : IDomainEventHandler
    {
        /// <summary>
        /// The mediator
        /// </summary>
        private readonly IMediator Mediator;
        private readonly ILogger<FileDomainEventHandler> DiagnosticLogger;

        #region Fields

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FileDomainEventHandler" /> class.
        /// </summary>
        /// <param name="mediator">The mediator.</param>
        public FileDomainEventHandler(IMediator mediator)
            : this(mediator, Microsoft.Extensions.Logging.Abstractions.NullLogger<FileDomainEventHandler>.Instance)
        {
        }

        public FileDomainEventHandler(IMediator mediator, ILogger<FileDomainEventHandler> diagnosticLogger)
        {
            this.Mediator = mediator;
            this.DiagnosticLogger = diagnosticLogger;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Handles the specified domain event.
        /// </summary>
        /// <param name="domainEvent">The domain event.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task<Result> Handle(IDomainEvent domainEvent,
                                         CancellationToken cancellationToken)
        {
            return await this.HandleSpecificDomainEvent((dynamic)domainEvent, cancellationToken);
        }

        /// <summary>
        /// Handles the specific domain event.
        /// </summary>
        /// <param name="domainEvent">The domain event.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        private async Task<Result> HandleSpecificDomainEvent(FileLineAddedEvent domainEvent,
                                                    CancellationToken cancellationToken)
        {
            FileCommands.ProcessTransactionForFileLineCommand command = new (domainEvent.FileId, domainEvent.LineNumber, domainEvent.FileLine);
            try
            {
                Result result = await this.Mediator.Send(command, cancellationToken);
                if (result.IsFailed)
                {
                    this.DiagnosticLogger.LogError("event-handler-dispatch-failed FileId {FileId} LineNumber {LineNumber} EstateId {EstateId} MerchantId {MerchantId} EventType {EventType} ResultStatus {ResultStatus} Error {Error}",
                                                   domainEvent.FileId,
                                                   domainEvent.LineNumber,
                                                   domainEvent.EstateId,
                                                   domainEvent.MerchantId,
                                                   nameof(FileLineAddedEvent),
                                                   result.Status,
                                                   result.Message);
                }
                else
                {
                    this.DiagnosticLogger.LogDebug("event-handler-dispatch-completed FileId {FileId} LineNumber {LineNumber} EstateId {EstateId} MerchantId {MerchantId} EventType {EventType}",
                                                   domainEvent.FileId,
                                                   domainEvent.LineNumber,
                                                   domainEvent.EstateId,
                                                   domainEvent.MerchantId,
                                                   nameof(FileLineAddedEvent));
                }

                return result;
            }
            catch (Exception ex)
            {
                this.DiagnosticLogger.LogError(ex,
                                               "event-handler-dispatch-threw FileId {FileId} LineNumber {LineNumber} EstateId {EstateId} MerchantId {MerchantId} EventType {EventType}",
                                               domainEvent.FileId,
                                               domainEvent.LineNumber,
                                               domainEvent.EstateId,
                                               domainEvent.MerchantId,
                                               nameof(FileLineAddedEvent));
                throw;
            }
        }

        /// <summary>
        /// Handles the specific domain event.
        /// </summary>
        /// <param name="domainEvent">The domain event.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        private async Task<Result> HandleSpecificDomainEvent(FileAddedToImportLogEvent domainEvent,
                                                             CancellationToken cancellationToken)
        {
            FileCommands.ProcessUploadedFileCommand command = new (domainEvent.EstateId,
                                                                                domainEvent.MerchantId,
                                                                                domainEvent.FileImportLogId,
                                                                                domainEvent.FileId,
                                                                                domainEvent.UserId,
                                                                                domainEvent.FilePath,
                                                                                domainEvent.FileProfileId,
                                                                                domainEvent.FileUploadedDateTime);

            return await this.Mediator.Send(command, cancellationToken);
        }

        #endregion
    }
}
