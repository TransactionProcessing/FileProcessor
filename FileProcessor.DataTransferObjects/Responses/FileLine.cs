namespace FileProcessor.DataTransferObjects.Responses
{
    using System;
    using System.ComponentModel;
    using System.Collections.Generic;
    
    /// <summary>Processing details for one line in an uploaded file.</summary>
    [Description("Processing details for one line in an uploaded file.")]
    public class FileLine
    {
        /// <summary>Gets or sets the original text of this line.</summary>
        [Description("The original text of this line.")]
        public String LineData { get; set; }
        /// <summary>Gets or sets the one-based line number in the uploaded file.</summary>
        [Description("The one-based line number in the uploaded file.")]
        public Int32 LineNumber { get; set; }

        /// <summary>Gets or sets the processing outcome for this line.</summary>
        [Description("The processing outcome for this line.")]
        public FileLineProcessingResult ProcessingResult { get; set; }

        /// <summary>Gets or sets the transaction identifier created for this line, when available.</summary>
        [Description("The transaction identifier created for this line, when available.")]
        public Guid TransactionId { get; set; }

        /// <summary>Gets or sets the domain event identifier produced for this line, when available.</summary>
        [Description("The domain event identifier produced for this line, when available.")]
        public Guid EventId { get; set; }

        /// <summary>Gets or sets the rejection reason when the line was rejected.</summary>
        [Description("The rejection reason when the line was rejected.")]
        public String RejectionReason { get; set; }

        /// <summary>Gets or sets the failure reason when processing the line failed.</summary>
        [Description("The failure reason when processing the line failed.")]
        public String FailureReason { get; set; }

        /// <summary>Gets or sets the response code returned for the line, when available.</summary>
        [Description("The response code returned for the line, when available.")]
        public String ResponseCode { get; set; }

        /// <summary>Gets or sets the number of dispatch attempts made for the line.</summary>
        [Description("The number of dispatch attempts made for the line.")]
        public Int32 DispatchAttemptCount { get; set; }

        /// <summary>Gets or sets failed transaction dispatch attempts associated with the line.</summary>
        [Description("Failed transaction dispatch attempts associated with the line.")]
        public List<TransactionDispatchAttempt> FailedDispatchAttempts { get; set; } = new();
    }
}
