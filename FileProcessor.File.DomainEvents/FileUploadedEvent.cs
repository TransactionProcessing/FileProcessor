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
    /// <seealso cref="System.IEquatable{FileProcessor.File.DomainEvents.FileCreatedEvent}" />
    public record FileCreatedEvent : DomainEventRecord.DomainEvent
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FileCreatedEvent" /> class.
        /// </summary>
        /// <param name="aggregateId">The aggregate identifier.</param>
        /// <param name="fileImportLogId">The file import log identifier.</param>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="merchantId">The merchant identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="fileProfileId">The file profile identifier.</param>
        /// <param name="fileLocation">The file location.</param>
        public FileCreatedEvent(Guid aggregateId,
                                Guid fileImportLogId,
                                Guid estateId,
                                Guid merchantId,
                                Guid userId,
                                Guid fileProfileId,
                                String fileLocation,
                                DateTime fileReceivedDateTime) : base(aggregateId, Guid.NewGuid())
        {
            this.FileId = aggregateId;
            this.FileImportLogId = fileImportLogId;
            this.EstateId = estateId;
            this.MerchantId = merchantId;
            this.UserId = userId;
            this.FileProfileId = fileProfileId;
            this.FileLocation = fileLocation;
            this.FileReceivedDateTime = fileReceivedDateTime;
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
        /// Gets or sets the file import log identifier.
        /// </summary>
        /// <value>
        /// The file import log identifier.
        /// </value>
        public Guid FileImportLogId { get; init; }

        /// <summary>
        /// Gets or sets the file location.
        /// </summary>
        /// <value>
        /// The file location.
        /// </value>
        public String FileLocation { get; init; }

        /// <summary>
        /// Gets or sets the file profile identifier.
        /// </summary>
        /// <value>
        /// The file profile identifier.
        /// </value>
        public Guid FileProfileId { get; init; }

        /// <summary>
        /// Gets or sets the file received date time.
        /// </summary>
        /// <value>
        /// The file received date time.
        /// </value>
        public DateTime FileReceivedDateTime { get; init; }

        /// <summary>
        /// Gets or sets the merchant identifier.
        /// </summary>
        /// <value>
        /// The merchant identifier.
        /// </value>
        public Guid MerchantId { get; init; }

        /// <summary>
        /// Gets or sets the user identifier.
        /// </summary>
        /// <value>
        /// The user identifier.
        /// </value>
        public Guid UserId { get; init; }

        #endregion
    }
}