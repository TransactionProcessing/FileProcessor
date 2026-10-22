namespace FileProcessor.DataTransferObjects.Responses
{
    using System;
    using System.ComponentModel;
    using System.Collections.Generic;
    
    /// <summary>A summary of files uploaded during one import period.</summary>
    [Description("A summary of files uploaded during one import period.")]
    public class FileImportLog
    {
        /// <summary>Gets or sets the identifier of this import log.</summary>
        [Description("The identifier of this import log.")]
        public Guid FileImportLogId { get; set; }

        /// <summary>Gets or sets the import log timestamp.</summary>
        [Description("The import log timestamp.")]
        public DateTime ImportLogDateTime { get; set; }
        
        /// <summary>Gets or sets the calendar date of the import log.</summary>
        [Description("The calendar date of the import log.")]
        public DateTime ImportLogDate { get; set; }

        /// <summary>Gets or sets the time of day of the import log.</summary>
        [Description("The time of day of the import log.")]
        public TimeSpan ImportLogTime { get; set; }

        /// <summary>Gets or sets the number of files in this log.</summary>
        [Description("The number of files in this log.")]
        public Int32 FileCount { get; set; }

        /// <summary>Gets or sets the files included in this log.</summary>
        [Description("The files included in this log.")]
        public List<FileImportLogFile> Files { get; set; }
    }
}
