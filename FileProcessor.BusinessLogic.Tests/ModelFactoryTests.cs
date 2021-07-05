using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileProcessor.BusinessLogic.Tests
{
    using Common;
    using EstateReporting.Database.Entities;
    using FIleProcessor.Models;
    using Shouldly;
    using Testing;
    using Xunit;
    using FileImportLog = FIleProcessor.Models.FileImportLog;

    public class ModelFactoryTests
    {
        [Fact]
        public void ModelFactory_ConvertFrom_FileImportLog_IsConverted()
        {
            IModelFactory modelFactory = new ModelFactory();
            List<FileImportLogFile> files = new List<FileImportLogFile>();
            files.AddRange(TestData.FileImportLog1Files);
            files.AddRange(TestData.FileImportLog2Files);
            List<FileImportLog> result = modelFactory.ConvertFrom(TestData.FileImportLogs, files);

            this.VerifyImportLogs(result);
        }

        [Fact]
        public void ModelFactory_ConvertFrom_FileImportLog_NoFiles_IsConverted()
        {
            IModelFactory modelFactory = new ModelFactory();
            List<FileImportLogFile> files = new List<FileImportLogFile>();
            List<FileImportLog> result = modelFactory.ConvertFrom(TestData.FileImportLogs, files);

            this.VerifyImportLogs(result);
        }

        [Fact]
        public void ModelFactory_ConvertFrom_FileImportLog_NoImportLogs_IsConverted()
        {
            IModelFactory modelFactory = new ModelFactory();
            List<EstateReporting.Database.Entities.FileImportLog> fileImportLogs = new List<EstateReporting.Database.Entities.FileImportLog>();
            List<FileImportLogFile> files = new List<FileImportLogFile>();
            List<FileImportLog> result = modelFactory.ConvertFrom(fileImportLogs, files);

            result.ShouldNotBeNull();
            result.ShouldBeEmpty();
        }

        private void VerifyImportLogs(List<FIleProcessor.Models.FileImportLog> importLogs)
        {
            importLogs.ShouldNotBeNull();
            importLogs.ShouldNotBeEmpty();
            importLogs.Count.ShouldBe(TestData.FileImportLogs.Count);
            foreach (EstateReporting.Database.Entities.FileImportLog fileImportLog in TestData.FileImportLogs)
            {
                var importLog = importLogs.SingleOrDefault(i => i.FileImportLogId == fileImportLog.FileImportLogId);
                importLog.ShouldNotBeNull();
                importLog.FileImportLogDateTime.ShouldBe(fileImportLog.ImportLogDateTime);
                importLog.Files.Count.ShouldBe(importLog.Files.Count);
                
                foreach (ImportLogFile importLogFile in importLog.Files)
                {
                    var file = importLog.Files.SingleOrDefault(impfile => impfile.FileId == importLogFile.FileId);
                    file.ShouldNotBeNull();
                    file.MerchantId.ShouldBe(importLogFile.MerchantId);
                    file.FilePath.ShouldBe(importLogFile.FilePath);
                    file.FileProfileId.ShouldBe(importLogFile.FileProfileId);
                    file.OriginalFileName.ShouldBe(importLogFile.OriginalFileName);
                    file.UserId.ShouldBe(importLogFile.UserId);
                }
            }
        }
    }
}
