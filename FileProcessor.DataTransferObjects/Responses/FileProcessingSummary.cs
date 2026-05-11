namespace FileProcessor.DataTransferObjects.Responses
{
    using System;

    public class FileProcessingSummary
    {
        public Int32 FailedLines { get; set; }

        public Int32 IgnoredLines { get; set; }

        public Int32 NotProcessedLines { get; set; }

        public Int32 RejectedLines { get; set; }

        public Int32 SuccessfullyProcessedLines { get; set; }

        public Int32 TotalLines { get; set; }
    }
}