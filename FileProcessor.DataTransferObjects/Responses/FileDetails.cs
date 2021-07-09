namespace FileProcessor.DataTransferObjects.Responses
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class FileDetails
    {
        /// <summary>
        /// Gets or sets the file identifier.
        /// </summary>
        /// <value>
        /// The file identifier.
        /// </value>
        [JsonProperty("file_id")]
        public Guid FileId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [processing completed].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [processing completed]; otherwise, <c>false</c>.
        /// </value>
        [JsonProperty("processing_completed")]
        public Boolean ProcessingCompleted { get; set; }

        /// <summary>
        /// Gets or sets the estate identifier.
        /// </summary>
        /// <value>
        /// The estate identifier.
        /// </value>
        [JsonProperty("estate_id")]
        public Guid EstateId { get; set; }

        /// <summary>
        /// Gets or sets the user identifier.
        /// </summary>
        /// <value>
        /// The user identifier.
        /// </value>
        [JsonProperty("user_id")]
        public Guid UserId { get; set; }

        /// <summary>
        /// Gets or sets the merchant identifier.
        /// </summary>
        /// <value>
        /// The merchant identifier.
        /// </value>
        [JsonProperty("merchant_id")]
        public Guid MerchantId { get; set; }

        /// <summary>
        /// Gets or sets the file profile identifier.
        /// </summary>
        /// <value>
        /// The file profile identifier.
        /// </value>
        [JsonProperty("file_profile_id")]
        public Guid FileProfileId { get; set; }

        /// <summary>
        /// Gets or sets the file import log identifier.
        /// </summary>
        /// <value>
        /// The file import log identifier.
        /// </value>
        [JsonProperty("file_import_log_id")]
        public Guid FileImportLogId { get; set; }

        /// <summary>
        /// Gets or sets the file location.
        /// </summary>
        /// <value>
        /// The file location.
        /// </value>
        [JsonProperty("file_location")]
        public String FileLocation { get; set; }

        /// <summary>
        /// Gets or sets the file lines.
        /// </summary>
        /// <value>
        /// The file lines.
        /// </value>
        [JsonProperty("file_lines")]
        public List<FileLine> FileLines { get; set; }

        /// <summary>
        /// Gets or sets the processing summary.
        /// </summary>
        /// <value>
        /// The processing summary.
        /// </value>
        [JsonProperty("processing_summary")]
        public FileProcessingSummary ProcessingSummary { get; set; }
    }
}