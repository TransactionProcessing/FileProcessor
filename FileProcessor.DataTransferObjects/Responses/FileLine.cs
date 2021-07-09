namespace FileProcessor.DataTransferObjects.Responses
{
    using System;
    using Newtonsoft.Json;

    public class FileLine
    {
        #region Properties

        /// <summary>
        /// Gets or sets the line data.
        /// </summary>
        /// <value>
        /// The line data.
        /// </value>
        [JsonProperty("line_data")]
        public String LineData { get; set; }

        /// <summary>
        /// Gets or sets the line number.
        /// </summary>
        /// <value>
        /// The line number.
        /// </value>
        [JsonProperty("line_number")]
        public Int32 LineNumber { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="FileLine"/> is ignored.
        /// </summary>
        /// <value>
        ///   <c>true</c> if ignored; otherwise, <c>false</c>.
        /// </value>
        [JsonProperty("processing_result")]
        public FileLineProcessingResult ProcessingResult { get; set; }

        /// <summary>
        /// Gets or sets the transaction identifier.
        /// </summary>
        /// <value>
        /// The transaction identifier.
        /// </value>
        [JsonProperty("transaction_id")]
        public Guid TransactionId { get; set; }

        #endregion
    }
}