namespace FileProcessor.Models
{
    using System;

    /// <summary>
    /// 
    /// </summary>
    public class ProcessingSummary
    {
        #region Properties

        /// <summary>
        /// Gets or sets the failed lines.
        /// </summary>
        /// <value>
        /// The failed lines.
        /// </value>
        public Int32 FailedLines { get; set; }

        /// <summary>
        /// Gets or sets the ignored lines.
        /// </summary>
        /// <value>
        /// The ignored lines.
        /// </value>
        public Int32 IgnoredLines { get; set; }

        /// <summary>
        /// Gets or sets the rejected lines.
        /// </summary>
        /// <value>
        /// The rejectedt lines.
        /// </value>
        public Int32 RejectedLines { get; set; }

        /// <summary>
        /// Gets or sets the not processed lines.
        /// </summary>
        /// <value>
        /// The not processed lines.
        /// </value>
        public Int32 NotProcessedLines { get; set; }

        /// <summary>
        /// Gets or sets the successfully processed lines.
        /// </summary>
        /// <value>
        /// The successfully processed lines.
        /// </value>
        public Int32 SuccessfullyProcessedLines { get; set; }

        /// <summary>
        /// Gets or sets the total lines.
        /// </summary>
        /// <value>
        /// The total lines.
        /// </value>
        public Int32 TotalLines { get; set; }

        #endregion
    }
}