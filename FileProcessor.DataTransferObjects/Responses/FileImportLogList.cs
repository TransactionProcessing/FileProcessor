namespace FileProcessor.DataTransferObjects.Responses
{
    using System.Collections.Generic;

    /// <summary>
    /// 
    /// </summary>
    public class FileImportLogList
    {
        #region Fields

        public List<FileImportLog> FileImportLogs { get; set; }

        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public int TotalCount { get; set; }

        public bool HasMore { get; set; }

        #endregion
    }
}
