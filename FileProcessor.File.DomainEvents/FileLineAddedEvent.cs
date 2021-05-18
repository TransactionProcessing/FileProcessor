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
    /// <seealso cref="System.IEquatable{FileProcessor.File.DomainEvents.FileLineAddedEvent}" />
    public record FileLineAddedEvent : DomainEventRecord.DomainEvent
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FileLineAddedEvent" /> class.
        /// </summary>
        /// <param name="aggregateId">The aggregate identifier.</param>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="lineNumber">The line number.</param>
        /// <param name="fileLine">The file line.</param>
        public FileLineAddedEvent(Guid aggregateId,
                                  Guid estateId,
                                  Int32 lineNumber,
                                  String fileLine) : base(aggregateId, Guid.NewGuid())
        {
            this.FileId = aggregateId;
            this.EstateId = estateId;
            this.LineNumber = lineNumber;
            this.FileLine = fileLine;
        }

        #endregion

        #region Properties

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
        /// Gets or sets the file line.
        /// </summary>
        /// <value>
        /// The file line.
        /// </value>
        public String FileLine { get; init; }

        /// <summary>
        /// Gets or sets the line number.
        /// </summary>
        /// <value>
        /// The line number.
        /// </value>
        public Int32 LineNumber { get; set; }

        #endregion
    }

    public record FileProcessingCompletedEvent : DomainEventRecord.DomainEvent
    {
        public FileProcessingCompletedEvent(Guid aggregateId,
                                            Guid estateId,
                                            DateTime processingCompletedDateTime) : base(aggregateId, Guid.NewGuid())
        {
            this.EstateId = estateId;
            this.FileId = aggregateId;
            this.ProcessingCompletedDateTime = processingCompletedDateTime;
        }

        /// <summary>
        /// Gets or sets the estate identifier.
        /// </summary>
        /// <value>
        /// The estate identifier.
        /// </value>
        public Guid EstateId { get; init; }

        /// <summary>
        /// Gets or sets the processing completed date time.
        /// </summary>
        /// <value>
        /// The processing completed date time.
        /// </value>
        public DateTime ProcessingCompletedDateTime { get; init; }

        /// <summary>
        /// Gets or sets the file identifier.
        /// </summary>
        /// <value>
        /// The file identifier.
        /// </value>
        public Guid FileId { get; init; }
    }
}