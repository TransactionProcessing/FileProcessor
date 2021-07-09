namespace FileProcessor.DataTransferObjects.Responses
{
    using System;
    using Newtonsoft.Json;

    public class FileProcessingSummary
    {
        #region Properties

        /// <summary>
        /// Gets or sets the failed lines.
        /// </summary>
        /// <value>
        /// The failed lines.
        /// </value>
        [JsonProperty("failed_lines")]
        public Int32 FailedLines { get; set; }

        /// <summary>
        /// Gets or sets the ignored lines.
        /// </summary>
        /// <value>
        /// The ignored lines.
        /// </value>
        [JsonProperty("ignored_lines")]
        public Int32 IgnoredLines { get; set; }

        /// <summary>
        /// Gets or sets the not processed lines.
        /// </summary>
        /// <value>
        /// The not processed lines.
        /// </value>
        [JsonProperty("not_processed_lines")]
        public Int32 NotProcessedLines { get; set; }

        /// <summary>
        /// Gets or sets the successfully processed lines.
        /// </summary>
        /// <value>
        /// The successfully processed lines.
        /// </value>
        [JsonProperty("successfully_processed_lines")]
        public Int32 SuccessfullyProcessedLines { get; set; }

        /// <summary>
        /// Gets or sets the total lines.
        /// </summary>
        /// <value>
        /// The total lines.
        /// </value>
        [JsonProperty("total_lines")]
        public Int32 TotalLines { get; set; }

        #endregion
    }
}