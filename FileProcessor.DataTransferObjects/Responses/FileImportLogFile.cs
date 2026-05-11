namespace FileProcessor.DataTransferObjects.Responses
{
    using System;
    public class FileImportLogFile
    {
        #region Properties

        public Guid FileId { get; set; }

        public Guid FileImportLogId { get; set; }

        public String FilePath { get; set; }

        public Guid FileProfileId { get; set; }

        public DateTime FileUploadedDateTime { get; set; }

        public Guid MerchantId { get; set; }

        public String OriginalFileName { get; set; }

        public Guid UserId { get; set; }

        #endregion
    }
}