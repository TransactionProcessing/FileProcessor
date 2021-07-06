namespace FileProcessor.DataTransferObjects.Responses
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>
    /// 
    /// </summary>
    public class FileImportLog
    {
        /// <summary>
        /// Gets or sets the file import log identifier.
        /// </summary>
        /// <value>
        /// The file import log identifier.
        /// </value>
        [JsonProperty("file_import_log_id")]
        public Guid FileImportLogId { get; set; }

        /// <summary>
        /// Gets or sets the import log date time.
        /// </summary>
        /// <value>
        /// The import log date time.
        /// </value>
        [JsonProperty("import_log_date_time")]
        public DateTime ImportLogDateTime { get; set; }
        
        /// <summary>
        /// Gets or sets the import log date.
        /// </summary>
        /// <value>
        /// The import log date.
        /// </value>
        [JsonProperty("import_log_date")]
        public DateTime ImportLogDate { get; set; }

        /// <summary>
        /// Gets or sets the import log time.
        /// </summary>
        /// <value>
        /// The import log time.
        /// </value>
        [JsonProperty("import_log_time")]
        public TimeSpan ImportLogTime { get; set; }

        /// <summary>
        /// Gets or sets the file count.
        /// </summary>
        /// <value>
        /// The file count.
        /// </value>
        [JsonProperty("file_count")]
        public Int32 FileCount { get; set; }

        /// <summary>
        /// Gets or sets the files.
        /// </summary>
        /// <value>
        /// The files.
        /// </value>
        [JsonProperty("files")]
        public List<FileImportLogFile> Files { get; set; }
    }
}