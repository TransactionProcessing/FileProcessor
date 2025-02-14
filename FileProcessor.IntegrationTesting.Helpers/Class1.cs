using TransactionProcessor.IntegrationTesting.Helpers;

namespace FileProcessor.IntegrationTesting.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DataTransferObjects.Responses;

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

    public static class SubscriptionsHelper
    {
        public static List<(String streamName, String groupName, Int32 maxRetries)> GetSubscriptions()
        {
            List<(String streamName, String groupName, Int32 maxRetries)> subscriptions = new(){
                                                                                                   ("$ce-FileImportLogAggregate", "File Processor", 0),
                                                                                               ("$ce-FileAggregate", "File Processor", 0)
                                                                                           };
            return subscriptions;
        }
    }
}