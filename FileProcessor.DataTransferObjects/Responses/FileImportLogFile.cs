namespace FileProcessor.DataTransferObjects.Responses
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// 
    /// </summary>
    public class FileImportLogFile
    {
        #region Properties

        /// <summary>
        /// Gets or sets the file identifier.
        /// </summary>
        /// <value>
        /// The file identifier.
        /// </value>
        [JsonProperty("file_id")]
        public Guid FileId { get; set; }

        /// <summary>
        /// Gets or sets the file import log identifier.
        /// </summary>
        /// <value>
        /// The file import log identifier.
        /// </value>
        [JsonProperty("file_import_log_id")]
        public Guid FileImportLogId { get; set; }

        /// <summary>
        /// Gets or sets the file path.
        /// </summary>
        /// <value>
        /// The file path.
        /// </value>
        [JsonProperty("file_path")]
        public String FilePath { get; set; }

        /// <summary>
        /// Gets or sets the file profile identifier.
        /// </summary>
        /// <value>
        /// The file profile identifier.
        /// </value>
        [JsonProperty("file_profile_id")]
        public Guid FileProfileId { get; set; }

        /// <summary>
        /// Gets or sets the file uploaded date time.
        /// </summary>
        /// <value>
        /// The file uploaded date time.
        /// </value>
        [JsonProperty("file_uploaded_date_time")]
        public DateTime FileUploadedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the merchant identifier.
        /// </summary>
        /// <value>
        /// The merchant identifier.
        /// </value>
        [JsonProperty("merchant_id")]
        public Guid MerchantId { get; set; }

        /// <summary>
        /// Gets or sets the name of the original file.
        /// </summary>
        /// <value>
        /// The name of the original file.
        /// </value>
        [JsonProperty("original_file_name")]
        public String OriginalFileName { get; set; }

        /// <summary>
        /// Gets or sets the user identifier.
        /// </summary>
        /// <value>
        /// The user identifier.
        /// </value>
        [JsonProperty("user_id")]
        public Guid UserId { get; set; }

        #endregion
    }
}