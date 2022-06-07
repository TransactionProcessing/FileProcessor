namespace FileProcessor.File.DomainEvents
{
    using System;
    using Shared.DomainDrivenDesign.EventSourcing;

    public record FileLineProcessingRejectedEvent : DomainEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileLineProcessingFailedEvent" /> class.
        /// </summary>
        /// <param name="aggregateId">The aggregate identifier.</param>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="lineNumber">The line number.</param>
        /// <param name="reason">The reason.</param>
        public FileLineProcessingRejectedEvent(Guid aggregateId,
                                               Guid estateId, Int32 lineNumber,
                                               String reason) : base(aggregateId, Guid.NewGuid())
        {
            this.FileId = aggregateId;
            this.EstateId = estateId;
            this.LineNumber = lineNumber;
            this.Reason = reason;
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
        public Int32 LineNumber { get; init; }

        /// <summary>
        /// Gets or sets the reason.
        /// </summary>
        /// <value>
        /// The reason.
        /// </value>
        public String Reason { get; init; }
    }
}