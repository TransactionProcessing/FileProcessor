namespace FileProcessor.Models
{
    using System;
    using System.Collections.Generic;

    public class FileLine
    {
        public String LineData { get; set; }

        public Int32 LineNumber { get; set; }

        public ProcessingResult ProcessingResult { get; set; }

        public Guid TransactionId { get; set; }

        public Guid EventId { get; set; }

        public String RejectedReason { get; set; }

        public Int32 DispatchAttemptCount { get; set; }

        public List<TransactionDispatchAttempt> FailedDispatchAttempts { get; set; } = new();
    }
}
