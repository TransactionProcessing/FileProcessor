namespace FileProcessor.Models
{
    using System;

    public class TransactionDispatchAttempt
    {
        public Int32 TransactionNumber { get; set; }

        public String FailureType { get; set; }

        public DateTimeOffset AttemptedAt { get; set; }
    }
}
