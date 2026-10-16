namespace FileProcessor.DataTransferObjects.Responses
{
    using System;
    using System.Collections.Generic;
    
    public class FileDetails
    {
        public Guid FileId { get; set; }

        public Boolean ProcessingCompleted { get; set; }

        public Boolean IsRejected { get; set; }

        public String RejectionCode { get; set; }

        public String RejectionReason { get; set; }

        public Int32 ObservedDetailRecordCount { get; set; }

        public Int32 MaximumDetailRecordCount { get; set; }

        public Guid EstateId { get; set; }

        public Guid UserId { get; set; }

        public String UserEmailAddress { get; set; }

        public Guid MerchantId { get; set; }

        public String MerchantName { get; set; }

        public Guid FileProfileId { get; set; }

        public String FileProfileName { get; set; }

        public Guid FileImportLogId { get; set; }

        public String FileLocation { get; set; }

        public List<FileLine> FileLines { get; set; }

        public FileProcessingSummary ProcessingSummary { get; set; }
    }
}
