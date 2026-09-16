namespace FileProcessor.Models
{
    using System;

    public class TransactionDispatchAttempt
    {
        public Int32 TransactionNumber { get; set; }

        public String FailureType { get; set; }

        public DateTime AttemptedAt { get; set; }
    }
}
