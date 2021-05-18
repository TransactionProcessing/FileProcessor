namespace FileProcessor.BusinessLogic.Requests
{
    using System;
    using MediatR;

    public record ProcessUploadedFileRequest : IRequest
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessUploadedFileRequest" /> class.
        /// </summary>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="merchantId">The merchant identifier.</param>
        /// <param name="fileId">The file identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="filePath">The file path.</param>
        /// <param name="fileProfileId">The file profile identifier.</param>
        /// <param name="fileUploadedDateTime">The file uploaded date time.</param>
        public ProcessUploadedFileRequest(Guid estateId,
                                          Guid merchantId,
                                          Guid fileImportLogId,
                                          Guid fileId,
                                          Guid userId,
                                          String filePath,
                                          Guid fileProfileId,
                                          DateTime fileUploadedDateTime)
        {
            this.FileUploadedDateTime = fileUploadedDateTime;
            this.EstateId = estateId;
            this.MerchantId = merchantId;
            this.FileId = fileId;
            this.UserId = userId;
            this.FilePath = filePath;
            this.FileProfileId = fileProfileId;
            this.FileImportLogId = fileImportLogId;
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
        /// Gets or sets the file uploaded date time.
        /// </summary>
        /// <value>
        /// The file uploaded date time.
        /// </value>
        public DateTime FileUploadedDateTime { get; init; }

        /// <summary>
        /// Gets or sets the file identifier.
        /// </summary>
        /// <value>
        /// The file identifier.
        /// </value>
        public Guid FileId { get; init; }

        /// <summary>
        /// Gets or sets the file path.
        /// </summary>
        /// <value>
        /// The file path.
        /// </value>
        public String FilePath { get; init; }

        /// <summary>
        /// Gets or sets the file profile identifier.
        /// </summary>
        /// <value>
        /// The file profile identifier.
        /// </value>
        public Guid FileProfileId { get; init; }

        /// <summary>
        /// Gets or sets the file import log identifier.
        /// </summary>
        /// <value>
        /// The file import log identifier.
        /// </value>
        public Guid FileImportLogId { get; init; }

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