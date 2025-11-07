using Newtonsoft.Json;

namespace FileProcessor.DataTransferObjects.Responses
{
    using System.Collections.Generic;

    /// <summary>
    /// 
    /// </summary>
    public class FileImportLogList
    {
        #region Fields

        /// <summary>
        /// The file import logs
        /// </summary>
        [JsonProperty("file_import_logs")]
        public List<FileImportLog> FileImportLogs { get; set; }

        #endregion
    }
}