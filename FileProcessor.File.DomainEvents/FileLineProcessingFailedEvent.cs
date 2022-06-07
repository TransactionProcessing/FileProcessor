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
    /// <seealso cref="System.IEquatable{FileProcessor.File.DomainEvents.FileLineProcessingFailedEvent}" />
    public record FileLineProcessingFailedEvent : DomainEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileLineProcessingFailedEvent"/> class.
        /// </summary>
        /// <param name="aggregateId">The aggregate identifier.</param>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="lineNumber">The line number.</param>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <param name="responseCode">The response code.</param>
        /// <param name="responseMessage">The response message.</param>
        public FileLineProcessingFailedEvent(Guid aggregateId,
                                             Guid estateId, Int32 lineNumber,
                                             Guid transactionId, String responseCode, String responseMessage) : base(aggregateId, Guid.NewGuid())
        {
            this.FileId = aggregateId;
            this.EstateId = estateId;
            this.LineNumber = lineNumber;
            this.TransactionId = transactionId;
            this.ResponseCode = responseCode;
            this.ResponseMessage = responseMessage;
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

        /// <summary>
        /// Gets or sets the response code.
        /// </summary>
        /// <value>
        /// The response code.
        /// </value>
        public String ResponseCode { get; init; }

        /// <summary>
        /// Gets or sets the response message.
        /// </summary>
        /// <value>
        /// The response message.
        /// </value>
        public String ResponseMessage { get; init; }
    }
}