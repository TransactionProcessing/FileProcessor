namespace FileProcessor.FileImportLog.DomainEvents
{
    using System;
    using Shared.DomainDrivenDesign.EventSourcing;

    public record FileAddedToImportLogEvent : DomainEventRecord.DomainEvent
    {
        /// <summary>
        /// Gets or sets the file import log identifier.
        /// </summary>
        /// <value>
        /// The file import log identifier.
        /// </value>
        public Guid FileImportLogId { get; init; }

        /// <summary>
        /// Gets or sets the estate identifier.
        /// </summary>
        /// <value>
        /// The estate identifier.
        /// </value>
        public Guid EstateId { get; init; }

        /// <summary>
        /// Gets or sets the merchant identifier.
        /// </summary>
        /// <value>
        /// The merchant identifier.
        /// </value>
        public Guid MerchantId { get; init; }

        /// <summary>
        /// Gets or sets the file identifier.
        /// </summary>
        /// <value>
        /// The file identifier.
        /// </value>
        public Guid FileId { get; init; }

        /// <summary>
        /// Gets or sets the user identifier.
        /// </summary>
        /// <value>
        /// The user identifier.
        /// </value>
        public Guid UserId { get; init; }

        /// <summary>
        /// Gets or sets the file profile identifier.
        /// </summary>
        /// <value>
        /// The file profile identifier.
        /// </value>
        public Guid FileProfileId { get; init; }

        /// <summary>
        /// Gets or sets the name of the original file.
        /// </summary>
        /// <value>
        /// The name of the original file.
        /// </value>
        public String OriginalFileName { get; init; }

        /// <summary>
        /// Gets or sets the file path.
        /// </summary>
        /// <value>
        /// The file path.
        /// </value>
        public String FilePath { get; init; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileAddedToImportLogEvent" /> class.
        /// </summary>
        /// <param name="aggregateId">The aggregate identifier.</param>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="merchantId">The merchant identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="fileProfileId">The file profile identifier.</param>
        /// <param name="originalFileName">Name of the original file.</param>
        /// <param name="filePath"></param>
        public FileAddedToImportLogEvent(Guid aggregateId, Guid fileId, Guid estateId, Guid merchantId, Guid userId, Guid fileProfileId,
                                         String originalFileName, String filePath) : base(aggregateId, Guid.NewGuid())
        {
            this.FileImportLogId = aggregateId;
            this.FileId = fileId;
            this.EstateId = estateId;
            this.MerchantId = merchantId;
            this.UserId = userId;
            this.FileProfileId = fileProfileId;
            this.OriginalFileName = originalFileName;
            this.FilePath = filePath;
        }
    }
}