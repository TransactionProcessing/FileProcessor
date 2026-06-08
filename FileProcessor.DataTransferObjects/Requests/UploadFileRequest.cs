using System;

namespace FileProcessor.DataTransferObjects
{
    /// <summary>
    /// 
    /// </summary>
    public class UploadFileRequest
    {
        public Guid FileId { get; set; }

        public Guid EstateId { get; set; }

        public Guid MerchantId { get; set; }

        public Guid UserId { get; set; }

        public Guid FileProfileId { get; set; }

        public DateTime UploadDateTime { get; set; }
    }
}
