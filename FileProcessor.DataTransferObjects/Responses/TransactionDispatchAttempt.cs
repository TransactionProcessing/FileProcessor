namespace FileProcessor.DataTransferObjects.Responses
{
    using System;
    using System.ComponentModel;

    /// <summary>Details of a failed attempt to dispatch a transaction from a file line.</summary>
    [Description("Details of a failed attempt to dispatch a transaction from a file line.")]
    public class TransactionDispatchAttempt
    {
        /// <summary>Gets or sets the transaction's position in the dispatch batch.</summary>
        [Description("The transaction's position in the dispatch batch.")]
        public Int32 TransactionNumber { get; set; }

        /// <summary>Gets or sets the type or category of the dispatch failure.</summary>
        [Description("The type or category of the dispatch failure.")]
        public String FailureType { get; set; }

        /// <summary>Gets or sets when the dispatch attempt occurred.</summary>
        [Description("When the dispatch attempt occurred.")]
        public DateTime AttemptedAt { get; set; }
    }
}
