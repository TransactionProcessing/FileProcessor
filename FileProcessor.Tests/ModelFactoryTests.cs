using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileProcessor.Tests
{
    using Common;
    using DataTransferObjects.Responses;
    using FIleProcessor.Models;
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using Shouldly;
    using Testing;
    using Xunit;
    using FileImportLog = FIleProcessor.Models.FileImportLog;

    public class ModelFactoryTests
    {
        [Fact]
        public void ModelFactory_ConvertFrom_FileImportLogList_IsConverted()
        {
            List<FIleProcessor.Models.FileImportLog> importLogs = TestData.FileImportLogModels;

            ModelFactory modelFactory = new ModelFactory();

            FileImportLogList result = modelFactory.ConvertFrom(importLogs);

            this.VerifyFileImportLogList(importLogs, result);
        }

        [Fact]
        public void ModelFactory_ConvertFrom_FileImportLogList_WithNoFiles_IsConverted()
        {
            List<FIleProcessor.Models.FileImportLog> importLogs = TestData.FileImportLogModels;

            foreach (FileImportLog fileImportLog in importLogs)
            {
                fileImportLog.Files = new List<ImportLogFile>();
            }

            ModelFactory modelFactory = new ModelFactory();

            FileImportLogList result = modelFactory.ConvertFrom(importLogs);

            this.VerifyFileImportLogList(importLogs, result);
        }

        [Fact]
        public void ModelFactory_ConvertFrom_FileImportLog_IsConverted()
        {
            FIleProcessor.Models.FileImportLog importLog = TestData.FileImportLogModel1;

            ModelFactory modelFactory = new ModelFactory();

            var result = modelFactory.ConvertFrom(importLog);

            this.VerifyFileImportLog(importLog, result);
        }

        [Fact]
        public void ModelFactory_ConvertFrom_FileImportLog_WithNoFiles_IsConverted()
        {
            FIleProcessor.Models.FileImportLog importLog = TestData.FileImportLogModel1;
            importLog.Files = new List<ImportLogFile>();

            ModelFactory modelFactory = new ModelFactory();

            var result = modelFactory.ConvertFrom(importLog);

            this.VerifyFileImportLog(importLog, result);
        }

        private void VerifyFileImportLogList(List<FIleProcessor.Models.FileImportLog> source, FileImportLogList fileImportLogList)
        {
            fileImportLogList.ShouldNotBeNull();
            fileImportLogList.FileImportLogs.ShouldNotBeNull();
            fileImportLogList.FileImportLogs.Count().ShouldBe(source.Count);

            foreach (FileImportLog fileImportLog in source)
            {
                DataTransferObjects.Responses.FileImportLog? foundFileImportLog = fileImportLogList.FileImportLogs.SingleOrDefault(i => i.FileImportLogId == fileImportLog.FileImportLogId);
                this.VerifyFileImportLog(fileImportLog, foundFileImportLog);
            }
        }

        private void VerifyFileImportLog(FIleProcessor.Models.FileImportLog source,
                                     DataTransferObjects.Responses.FileImportLog fileImportLog)
        {
            fileImportLog.ShouldNotBeNull();
            fileImportLog.Files.Count.ShouldBe(source.Files.Count);
            fileImportLog.ImportLogDateTime.ShouldBe(source.FileImportLogDateTime);

            foreach (ImportLogFile importLogFile in source.Files)
            {
                var foundFile = fileImportLog.Files.SingleOrDefault(f => f.FileId == importLogFile.FileId);
                foundFile.ShouldNotBeNull();
                foundFile.FileId.ShouldBe(importLogFile.FileId);
                foundFile.FileImportLogId.ShouldBe(fileImportLog.FileImportLogId);
                foundFile.MerchantId.ShouldBe(importLogFile.MerchantId);
                foundFile.OriginalFileName.ShouldBe(importLogFile.OriginalFileName);
                foundFile.FilePath.ShouldBe(importLogFile.FilePath);
                foundFile.FileProfileId.ShouldBe(importLogFile.FileProfileId);
                foundFile.FileUploadedDateTime.ShouldBe(importLogFile.UploadedDateTime);
                foundFile.UserId.ShouldBe(importLogFile.UserId);
            }
        }
    }
}
