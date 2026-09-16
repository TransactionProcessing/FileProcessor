using System.Threading.Tasks;
using System;
using Shared.Logger;
using SimpleResults;

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
            FileCommands.ProcessTransactionForFileLineCommand command = new (domainEvent.FileId, domainEvent.LineNumber, domainEvent.FileLine);
            try
            {
                Result result = await this.Mediator.Send(command, cancellationToken);
                if (result.IsFailed)
                {
                    Logger.LogError($"event-handler-dispatch-failed FileId={domainEvent.FileId} LineNumber={domainEvent.LineNumber} EstateId={domainEvent.EstateId} MerchantId={domainEvent.MerchantId} EventType={nameof(FileLineAddedEvent)} ResultStatus={result.Status} Error={result.Message}");
                }
                else
                {
                    Logger.LogDebug($"event-handler-dispatch-completed FileId={domainEvent.FileId} LineNumber={domainEvent.LineNumber} EstateId={domainEvent.EstateId} MerchantId={domainEvent.MerchantId} EventType={nameof(FileLineAddedEvent)}");
                }

                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError($"event-handler-dispatch-threw FileId={domainEvent.FileId} LineNumber={domainEvent.LineNumber} EstateId={domainEvent.EstateId} MerchantId={domainEvent.MerchantId} EventType={nameof(FileLineAddedEvent)} Exception={ex}");
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
