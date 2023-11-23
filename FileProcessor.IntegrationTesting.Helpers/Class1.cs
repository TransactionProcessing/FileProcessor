namespace FileProcessor.IntegrationTesting.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DataTransferObjects.Responses;
    using EstateManagement.IntegrationTesting.Helpers;

    namespace FileProcessor.IntegrationTests.Steps
    {
    }
    public class EstateDetails1
    {
        public EstateDetails1(EstateDetails estateDetails)
        {
            this.EstateDetails = estateDetails;
            this.FileImportLogFiles = new List<FileImportLogFile>();
        }

        public EstateDetails EstateDetails { get; private set; }

        private List<FileImportLogFile> FileImportLogFiles;

        public void AddFileImportLogFile(FileImportLogFile fileImportLogFile)
        {
            this.FileImportLogFiles.Add(fileImportLogFile);
        }

        public Guid GetFileId(String originalFileName)
        {
            return this.FileImportLogFiles.Single(o => o.OriginalFileName == originalFileName).FileId;
        }
    }
}