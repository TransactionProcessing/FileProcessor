namespace FileProcessor.DataTransferObjects.Responses
{
    using System;
    using System.ComponentModel;

    /// <summary>Counts of file lines grouped by processing outcome.</summary>
    [Description("Counts of file lines grouped by processing outcome.")]
    public class FileProcessingSummary
    {
        /// <summary>Gets or sets the number of lines that failed during processing.</summary>
        [Description("The number of lines that failed during processing.")]
        public Int32 FailedLines { get; set; }

        /// <summary>Gets or sets the number of lines intentionally ignored.</summary>
        [Description("The number of lines intentionally ignored.")]
        public Int32 IgnoredLines { get; set; }

        /// <summary>Gets or sets the number of lines not processed.</summary>
        [Description("The number of lines not processed.")]
        public Int32 NotProcessedLines { get; set; }

        /// <summary>Gets or sets the number of rejected lines.</summary>
        [Description("The number of rejected lines.")]
        public Int32 RejectedLines { get; set; }

        /// <summary>Gets or sets the number of successfully processed lines.</summary>
        [Description("The number of successfully processed lines.")]
        public Int32 SuccessfullyProcessedLines { get; set; }

        /// <summary>Gets or sets the total number of lines in the file.</summary>
        [Description("The total number of lines in the file.")]
        public Int32 TotalLines { get; set; }
    }
}
