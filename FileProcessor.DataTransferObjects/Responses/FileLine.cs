namespace FileProcessor.DataTransferObjects.Responses
{
    using System;
    
    public class FileLine
    {
        public String LineData { get; set; }
        public Int32 LineNumber { get; set; }

        public FileLineProcessingResult ProcessingResult { get; set; }

        public Guid TransactionId { get; set; }

        public String RejectionReason { get; set; }
    }
}