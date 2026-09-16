using System.Threading.Tasks;
using System;
using SimpleResults;
using FileProcessor.BusinessLogic.Common;

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

        #region Fields

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FileDomainEventHandler" /> class.
        /// </summary>
        /// <param name="mediator">The mediator.</param>
        public FileDomainEventHandler(IMediator mediator)
        {
            this.Mediator = mediator;
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
            FileCommands.ProcessTransactionForFileLineCommand command = new (domainEvent.FileId, domainEvent.EventId, domainEvent.LineNumber, domainEvent.FileLine);
            try
            {
                Result result = await this.Mediator.Send(command, cancellationToken);
                if (result.IsFailed)
                {
                    FileLineProcessingDiagnostics.EventHandlerDispatchFailed(domainEvent, result);
                }
                else
                {
                    FileLineProcessingDiagnostics.EventHandlerDispatchCompleted(domainEvent);
                }

                return result;
            }
            catch (Exception ex)
            {
                FileLineProcessingDiagnostics.EventHandlerDispatchThrew(domainEvent, ex);
                throw;
            }
        }

        private Task<Result> HandleSpecificDomainEvent(FileLineTransactionDispatchFailedEvent domainEvent,
                                                       CancellationToken cancellationToken)
        {
            _ = domainEvent;
            _ = cancellationToken;
            // The event records an attempt for audit purposes; it must not enqueue the line as completed.
            return Task.FromResult(Result.Success());
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
