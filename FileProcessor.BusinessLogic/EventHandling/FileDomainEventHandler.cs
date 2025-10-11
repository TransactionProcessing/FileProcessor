using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleResults;

namespace FileProcessor.BusinessLogic.EventHandling
{
    using System.Threading;
    using File.DomainEvents;
    using FileAggregate;
    using FileImportLog.DomainEvents;
    using Managers;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Requests;
    using SecurityService.Client;
    using SecurityService.DataTransferObjects.Responses;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EventStore.Aggregate;
    using Shared.EventStore.EventHandling;
    using Shared.General;
    using Shared.Logger;
    using TransactionProcessor.Client;
    using TransactionProcessor.DataTransferObjects;

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

            return await this.Mediator.Send(command, cancellationToken);
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
