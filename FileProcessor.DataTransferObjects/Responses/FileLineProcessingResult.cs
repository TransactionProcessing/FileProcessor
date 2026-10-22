using System.ComponentModel;
namespace FileProcessor.DataTransferObjects.Responses
{
    /// <summary>Possible outcomes for a processed file line.</summary>
    [Description("Possible outcomes for a processed file line.")]
    public enum FileLineProcessingResult
    {
        /// <summary>The processing outcome is not known.</summary>
        [Description("The processing outcome is not known.")]
        Unknown,

        /// <summary>The line has not been processed.</summary>
        [Description("The line has not been processed.")]
        NotProcessed,

        /// <summary>The line was processed successfully.</summary>
        [Description("The line was processed successfully.")]
        Successful,

        /// <summary>Processing the line failed.</summary>
        [Description("Processing the line failed.")]
        Failed,

        /// <summary>The line was intentionally ignored.</summary>
        [Description("The line was intentionally ignored.")]
        Ignored,

        /// <summary>The line or file was rejected by validation.</summary>
        [Description("The line or file was rejected by validation.")]
        Rejected
    }
}
