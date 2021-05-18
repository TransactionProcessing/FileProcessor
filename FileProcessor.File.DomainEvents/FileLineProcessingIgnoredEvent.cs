namespace FileProcessor.File.DomainEvents
{
    using System;
    using Shared.DomainDrivenDesign.EventSourcing;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Shared.DomainDrivenDesign.EventSourcing.DomainEventRecord.DomainEvent" />
    /// <seealso cref="Shared.DomainDrivenDesign.EventSourcing.IDomainEvent" />
    /// <seealso cref="System.IEquatable{Shared.DomainDrivenDesign.EventSourcing.DomainEventRecord.DomainEvent}" />
    /// <seealso cref="System.IEquatable{FileProcessor.File.DomainEvents.FileLineProcessingIgnoredEvent}" />
    public record FileLineProcessingIgnoredEvent : DomainEventRecord.DomainEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileLineProcessingFailedEvent" /> class.
        /// </summary>
        /// <param name="aggregateId">The aggregate identifier.</param>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="lineNumber">The line number.</param>
        public FileLineProcessingIgnoredEvent(Guid aggregateId,
                                              Guid estateId, Int32 lineNumber) : base(aggregateId, Guid.NewGuid())
        {
            this.FileId = aggregateId;
            this.EstateId = estateId;
            this.LineNumber = lineNumber;
        }

        /// <summary>
        /// Gets or sets the estate identifier.
        /// </summary>
        /// <value>
        /// The estate identifier.
        /// </value>
        public Guid EstateId { get; init; }

        /// <summary>
        /// Gets or sets the file identifier.
        /// </summary>
        /// <value>
        /// The file identifier.
        /// </value>
        public Guid FileId { get; init; }

        /// <summary>
        /// Gets or sets the line number.
        /// </summary>
        /// <value>
        /// The line number.
        /// </value>
        public Int32 LineNumber { get; set; }
    }
}