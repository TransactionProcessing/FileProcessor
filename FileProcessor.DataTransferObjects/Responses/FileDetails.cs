namespace FileProcessor.DataTransferObjects.Responses
{
    using System;
    using System.ComponentModel;
    using System.Collections.Generic;
    
    /// <summary>File metadata, processing state, line results, and summary returned by file queries.</summary>
    [Description("File metadata, processing state, line results, and summary returned by file queries.")]
    public class FileDetails
    {
        /// <summary>Gets or sets the file identifier.</summary>
        [Description("The file identifier.")]
        public Guid FileId { get; set; }

        /// <summary>Gets or sets whether processing of the file has completed.</summary>
        [Description("Whether processing of the file has completed.")]
        public Boolean ProcessingCompleted { get; set; }

        /// <summary>Gets or sets whether the file was rejected before or during processing.</summary>
        [Description("Whether the file was rejected before or during processing.")]
        public Boolean IsRejected { get; set; }

        /// <summary>Gets or sets the machine-readable reason code when the file was rejected.</summary>
        [Description("The machine-readable reason code when the file was rejected.")]
        public String RejectionCode { get; set; }

        /// <summary>Gets or sets the human-readable reason when the file was rejected.</summary>
        [Description("The human-readable reason when the file was rejected.")]
        public String RejectionReason { get; set; }

        /// <summary>Gets or sets the number of detail records observed in the uploaded file.</summary>
        [Description("The number of detail records observed in the uploaded file.")]
        public Int32 ObservedDetailRecordCount { get; set; }

        /// <summary>Gets or sets the configured maximum number of detail records allowed.</summary>
        [Description("The configured maximum number of detail records allowed.")]
        public Int32 MaximumDetailRecordCount { get; set; }

        /// <summary>Gets or sets the identifier of the owning estate.</summary>
        [Description("The identifier of the owning estate.")]
        public Guid EstateId { get; set; }

        /// <summary>Gets or sets the identifier of the uploading user.</summary>
        [Description("The identifier of the uploading user.")]
        public Guid UserId { get; set; }

        /// <summary>Gets or sets the email address of the uploading user, when available.</summary>
        [Description("The email address of the uploading user, when available.")]
        public String UserEmailAddress { get; set; }

        /// <summary>Gets or sets the identifier of the associated merchant.</summary>
        [Description("The identifier of the associated merchant.")]
        public Guid MerchantId { get; set; }

        /// <summary>Gets or sets the name of the associated merchant.</summary>
        [Description("The name of the associated merchant.")]
        public String MerchantName { get; set; }

        /// <summary>Gets or sets the identifier of the file profile used to process the file.</summary>
        [Description("The identifier of the file profile used to process the file.")]
        public Guid FileProfileId { get; set; }

        /// <summary>Gets or sets the identifier of the file profile version used to process the file.</summary>
        [Description("The identifier of the file profile version used to process the file.")]
        public Guid FileProfileVersionId { get; set; }

        /// <summary>Gets or sets the name of the file profile used to process the file.</summary>
        [Description("The name of the file profile used to process the file.")]
        public String FileProfileName { get; set; }

        /// <summary>Gets or sets the import log that contains this file.</summary>
        [Description("The import log that contains this file.")]
        public Guid FileImportLogId { get; set; }

        /// <summary>Gets or sets the stored file location.</summary>
        [Description("The stored file location.")]
        public String FileLocation { get; set; }

        /// <summary>Gets or sets the individual processing results for the file's lines.</summary>
        [Description("The individual processing results for the file's lines.")]
        public List<FileLine> FileLines { get; set; }

        /// <summary>Gets or sets counts of lines by processing outcome.</summary>
        [Description("Counts of lines by processing outcome.")]
        public FileProcessingSummary ProcessingSummary { get; set; }
    }
}
