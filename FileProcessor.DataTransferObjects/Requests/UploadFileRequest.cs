using System;
using System.ComponentModel;

namespace FileProcessor.DataTransferObjects
{
    /// <summary>Metadata submitted with a file upload.</summary>
    [Description("Metadata submitted with a file upload.")]
    public class UploadFileRequest
    {
        /// <summary>Gets or sets the caller-supplied identifier for the file being uploaded.</summary>
        [Description("The caller-supplied identifier for the file being uploaded.")]
        public Guid FileId { get; set; }

        /// <summary>Gets or sets the identifier of the estate that owns the file.</summary>
        [Description("The identifier of the estate that owns the file.")]
        public Guid EstateId { get; set; }

        /// <summary>Gets or sets the identifier of the merchant associated with the file.</summary>
        [Description("The identifier of the merchant associated with the file.")]
        public Guid MerchantId { get; set; }

        /// <summary>Gets or sets the identifier of the user submitting the file.</summary>
        [Description("The identifier of the user submitting the file.")]
        public Guid UserId { get; set; }

        /// <summary>Gets or sets the identifier of the file profile used to process the file.</summary>
        [Description("The identifier of the file profile used to process the file.")]
        public Guid FileProfileId { get; set; }

        /// <summary>Gets or sets the time the file was uploaded. If omitted or set to the minimum date value, the server uses the current local time.</summary>
        [Description("The time the file was uploaded. If omitted or set to the minimum date value, the server uses the current local time.")]
        public DateTime UploadDateTime { get; set; }
    }
}
