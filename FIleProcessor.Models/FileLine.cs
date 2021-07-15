namespace FIleProcessor.Models
{
    using System;

    /// <summary>
    /// 
    /// </summary>
    public class FileLine
    {
        #region Properties

        /// <summary>
        /// Gets or sets the line data.
        /// </summary>
        /// <value>
        /// The line data.
        /// </value>
        public String LineData { get; set; }

        /// <summary>
        /// Gets or sets the line number.
        /// </summary>
        /// <value>
        /// The line number.
        /// </value>
        public Int32 LineNumber { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="FileLine"/> is ignored.
        /// </summary>
        /// <value>
        ///   <c>true</c> if ignored; otherwise, <c>false</c>.
        /// </value>
        public ProcessingResult ProcessingResult { get; set; }

        /// <summary>
        /// Gets or sets the transaction identifier.
        /// </summary>
        /// <value>
        /// The transaction identifier.
        /// 1011934
        /// </value>
        public Guid TransactionId { get; set; }

        /// <summary>
        /// Gets or sets the rejected reason.
        /// </summary>
        /// <value>
        /// The rejected reason.
        /// </value>
        public String RejectedReason { get; set; }

        #endregion
    }
}