﻿namespace FileProcessor.BusinessLogic.Requests
{
    using System;
    using MediatR;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="MediatR.IRequest" />
    /// <seealso cref="MediatR.IRequest{MediatR.Unit}" />
    /// <seealso cref="MediatR.IBaseRequest" />
    /// <seealso cref="System.IEquatable{FileProcessor.BusinessLogic.Requests.UploadFileRequest}" />
    public record UploadFileRequest : IRequest
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UploadFileRequest"/> class.
        /// </summary>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="merchantId">The merchant identifier.</param>
        /// <param name="fileId">The file identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="filePath">The file path.</param>
        /// <param name="fileProfileId">The file profile identifier.</param>
        public UploadFileRequest(Guid estateId,
                                 Guid merchantId,
                                 Guid fileId,
                                 Guid userId,
                                 String filePath,
                                 Guid fileProfileId)
        {
            this.EstateId = estateId;
            this.MerchantId = merchantId;
            this.FileId = fileId;
            this.UserId = userId;
            this.FilePath = filePath;
            this.FileProfileId = fileProfileId;
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