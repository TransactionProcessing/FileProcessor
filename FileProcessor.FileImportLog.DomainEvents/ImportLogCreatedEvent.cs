using System;

namespace FileProcessor.FileImportLog.DomainEvents
{
    using Shared.DomainDrivenDesign.EventSourcing;

    public record ImportLogCreatedEvent : DomainEventRecord.DomainEvent
    {
        public ImportLogCreatedEvent(Guid aggregateId, Guid estateId, DateTime importLogDateTime) : base(aggregateId, Guid.NewGuid())
        {
            this.EstateId = estateId;
            this.ImportLogDateTime = importLogDateTime;
            this.FileImportLogId = aggregateId;
        }
        
        /// <summary>
        /// Gets or sets the estate identifier.
        /// </summary>
        /// <value>
        /// The estate identifier.
        /// </value>
        public Guid EstateId { get; init; }

        /// <summary>
        /// Gets or sets the file import log identifier.
        /// </summary>
        /// <value>
        /// The file import log identifier.
        /// </value>
        public Guid FileImportLogId { get; init; }

        /// <summary>
        /// Gets or sets the import log date time.
        /// </summary>
        /// <value>
        /// The import log date time.
        /// </value>
        public DateTime ImportLogDateTime { get; init; }
    }
}
