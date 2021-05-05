namespace FileProcessor.BusinessLogic.Requests
{
    using System;
    using Managers;
    using MediatR;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="MediatR.IRequest" />
    /// <seealso cref="MediatR.IRequest{MediatR.Unit}" />
    /// <seealso cref="MediatR.IBaseRequest" />
    /// <seealso cref="System.IEquatable{FileProcessor.BusinessLogic.Requests.SafaricomTopupRequest}" />
    public record SafaricomTopupRequest : IRequest
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SafaricomTopupRequest" /> class.
        /// </summary>
        /// <param name="fileId">The file identifier.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="fileProfileId">The file profile identifier.</param>
        public SafaricomTopupRequest(Guid fileId,
                                     String fileName,
                                     Guid fileProfileId)
        {
            this.FileId = fileId;
            this.FileName = fileName;
            this.FileProfileId = fileProfileId;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the file identifier.
        /// </summary>
        /// <value>
        /// The file identifier.
        /// </value>
        public Guid FileId { get; init; }

        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        /// <value>
        /// The name of the file.
        /// </value>
        public String FileName { get; init; }

        /// <summary>
        /// Gets or sets the file profile identifier.
        /// </summary>
        /// <value>
        /// The file profile identifier.
        /// </value>
        public Guid FileProfileId { get; init; }

        #endregion
    }
}