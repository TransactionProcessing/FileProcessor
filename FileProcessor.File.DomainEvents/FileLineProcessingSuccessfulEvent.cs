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
    /// <seealso cref="System.IEquatable{FileProcessor.File.DomainEvents.FileLineProcessingSuccessfulEvent}" />
    public record FileLineProcessingSuccessfulEvent : DomainEventRecord.DomainEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileLineProcessingSuccessfulEvent"/> class.
        /// </summary>
        /// <param name="aggregateId">The aggregate identifier.</param>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="lineNumber">The line number.</param>
        /// <param name="transactionId">The transaction identifier.</param>
        public FileLineProcessingSuccessfulEvent(Guid aggregateId,
                                                 Guid estateId, Int32 lineNumber,
                                                 Guid transactionId) : base(aggregateId, Guid.NewGuid())
        {
            this.FileId = aggregateId;
            this.EstateId = estateId;
            this.LineNumber = lineNumber;
            this.TransactionId = transactionId;
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

        /// <summary>
        /// Gets or sets the transaction identifier.
        /// </summary>
        /// <value>
        /// The transaction identifier.
        /// </value>
        public Guid TransactionId { get; init; }
    }
}