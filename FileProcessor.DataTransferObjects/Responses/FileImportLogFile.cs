namespace FileProcessor.DataTransferObjects.Responses
{
    using System;
    using System.ComponentModel;
    /// <summary>File metadata contained in an import log response.</summary>
    [Description("File metadata contained in an import log response.")]
    public class FileImportLogFile
    {
        /// <summary>Gets or sets the identifier of the uploaded file.</summary>
        [Description("The identifier of the uploaded file.")]
        public Guid FileId { get; set; }

        /// <summary>Gets or sets the identifier of the import log containing the file.</summary>
        [Description("The identifier of the import log containing the file.")]
        public Guid FileImportLogId { get; set; }

        /// <summary>Gets or sets the stored file path.</summary>
        [Description("The stored file path.")]
        public String FilePath { get; set; }

        /// <summary>Gets or sets the identifier of the file profile used to process the file.</summary>
        [Description("The identifier of the file profile used to process the file.")]
        public Guid FileProfileId { get; set; }

        /// <summary>Gets or sets the identifier of the file profile version used to process the file.</summary>
        [Description("The identifier of the file profile version used to process the file.")]
        public Guid FileProfileVersionId { get; set; }

        /// <summary>Gets or sets when the file was uploaded.</summary>
        [Description("When the file was uploaded.")]
        public DateTime FileUploadedDateTime { get; set; }

        /// <summary>Gets or sets the identifier of the associated merchant.</summary>
        [Description("The identifier of the associated merchant.")]
        public Guid MerchantId { get; set; }

        /// <summary>Gets or sets the original name of the uploaded file.</summary>
        [Description("The original name of the uploaded file.")]
        public String OriginalFileName { get; set; }

        /// <summary>Gets or sets the identifier of the user who uploaded the file.</summary>
        [Description("The identifier of the user who uploaded the file.")]
        public Guid UserId { get; set; }
    }
}
