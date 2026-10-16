namespace FileProcessor.Models
{
    using System.Collections.Generic;

    public class FileImportLogQueryResult
    {
        public List<FileImportLog> FileImportLogs { get; set; } = new();

        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public int TotalCount { get; set; }

        public bool HasMore => PageNumber * PageSize < TotalCount;
    }
}
