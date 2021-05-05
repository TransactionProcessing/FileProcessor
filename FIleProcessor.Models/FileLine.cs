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
        /// Gets or sets a value indicating whether [successfully processed].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [successfully processed]; otherwise, <c>false</c>.
        /// </value>
        public Boolean SuccessfullyProcessed { get; set; }

        /// <summary>
        /// Gets or sets the transaction identifier.
        /// </summary>
        /// <value>
        /// The transaction identifier.
        /// </value>
        public Guid TransactionId { get; set; }

        #endregion
    }
}