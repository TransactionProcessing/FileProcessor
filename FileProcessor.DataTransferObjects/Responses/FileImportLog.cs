namespace FileProcessor.DataTransferObjects.Responses
{
    using System;
    using System.Collections.Generic;
    
    public class FileImportLog
    {
        public Guid FileImportLogId { get; set; }

        public DateTime ImportLogDateTime { get; set; }
        
        public DateTime ImportLogDate { get; set; }

        public TimeSpan ImportLogTime { get; set; }

        public Int32 FileCount { get; set; }

        public List<FileImportLogFile> Files { get; set; }
    }
}