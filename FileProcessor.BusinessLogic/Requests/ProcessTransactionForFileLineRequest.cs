namespace FileProcessor.BusinessLogic.Requests
{
    using System;
    using MediatR;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="MediatR.IRequest" />
    /// <seealso cref="MediatR.IRequest{MediatR.Unit}" />
    /// <seealso cref="MediatR.IBaseRequest" />
    /// <seealso cref="System.IEquatable{FileProcessor.BusinessLogic.Requests.ProcessTransactionForFileLineRequest}" />
    public record ProcessTransactionForFileLineRequest : IRequest
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessTransactionForFileLineRequest"/> class.
        /// </summary>
        /// <param name="fileId">The file identifier.</param>
        /// <param name="lineNumber">The line number.</param>
        /// <param name="fileLine">The file line.</param>
        public ProcessTransactionForFileLineRequest(Guid fileId,
                                                    Int32 lineNumber,
                                                    String fileLine)
        {
            this.FileId = fileId;
            this.LineNumber = lineNumber;
            this.FileLine = fileLine;
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
        public Int32 LineNumber { get; init; }

        #endregion
    }
}