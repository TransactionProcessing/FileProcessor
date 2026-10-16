namespace FileProcessor.DataTransferObjects.Responses
{
    using System;
    using System.Collections.Generic;
    
    public class FileLine
    {
        public String LineData { get; set; }
        public Int32 LineNumber { get; set; }

        public FileLineProcessingResult ProcessingResult { get; set; }

        public Guid TransactionId { get; set; }

        public Guid EventId { get; set; }

        public String RejectionReason { get; set; }

        public String FailureReason { get; set; }

        public String ResponseCode { get; set; }

        public Int32 DispatchAttemptCount { get; set; }

        public List<TransactionDispatchAttempt> FailedDispatchAttempts { get; set; } = new();
    }
}
