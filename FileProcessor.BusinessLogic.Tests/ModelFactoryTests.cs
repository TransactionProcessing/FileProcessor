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
    using Microsoft.EntityFrameworkCore;
    using Shouldly;
    using Testing;
    using Xunit;
    using FileImportLog = FIleProcessor.Models.FileImportLog;

    public class ModelFactoryTests
    {
        [Fact]
        public void ModelFactory_ConvertFrom_FileImportLogs_IsConverted()
        {
            IModelFactory modelFactory = new ModelFactory();
            List<FileImportLogFile> files = new List<FileImportLogFile>();
            files.AddRange(TestData.FileImportLog1Files);
            files.AddRange(TestData.FileImportLog2Files);
            List<FileImportLog> result = modelFactory.ConvertFrom(TestData.FileImportLogs, files);

            this.VerifyImportLogs(TestData.FileImportLogs, files, result);
        }

        [Fact]
        public void ModelFactory_ConvertFrom_FileImportLogs_NoFiles_IsConverted()
        {
            IModelFactory modelFactory = new ModelFactory();
            List<FileImportLogFile> files = new List<FileImportLogFile>();
            List<FileImportLog> result = modelFactory.ConvertFrom(TestData.FileImportLogs, files);

            this.VerifyImportLogs(TestData.FileImportLogs, files, result);
        }

        [Fact]
        public void ModelFactory_ConvertFrom_FileImportLog_IsConverted()
        {
            IModelFactory modelFactory = new ModelFactory();
            List<FileImportLogFile> files = new List<FileImportLogFile>();
            files.AddRange(TestData.FileImportLog1Files);
            FileImportLog result = modelFactory.ConvertFrom(TestData.FileImportLogs.First(), files);

            this.VerifyImportLog(TestData.FileImportLogs.First(), files, result);
        }

        [Fact]
        public void ModelFactory_ConvertFrom_FileImportLog_NoFiles_IsConverted()
        {
            IModelFactory modelFactory = new ModelFactory();
            List<FileImportLogFile> files = new List<FileImportLogFile>();
            FileImportLog result = modelFactory.ConvertFrom(TestData.FileImportLogs.First(), files);

            this.VerifyImportLog(TestData.FileImportLogs.First(), files, result);
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

        private void VerifyImportLogs(List<EstateReporting.Database.Entities.FileImportLog> sourceImportLogs, List<FileImportLogFile> sourceImportLogFiles, List<FIleProcessor.Models.FileImportLog> importLogs)
        {
            importLogs.ShouldNotBeNull();
            importLogs.ShouldNotBeEmpty();
            importLogs.Count.ShouldBe(TestData.FileImportLogs.Count);
            foreach (EstateReporting.Database.Entities.FileImportLog fileImportLog in sourceImportLogs)
            {
                var importLog = importLogs.SingleOrDefault(i => i.FileImportLogId == fileImportLog.FileImportLogId);
                var sourceFiles = sourceImportLogFiles.Where(s => s.FileImportLogId == fileImportLog.FileImportLogId).ToList();
                VerifyImportLog(fileImportLog, sourceFiles, importLog);
            }
        }

        private void VerifyImportLog(EstateReporting.Database.Entities.FileImportLog sourceImportLog, List<FileImportLogFile> sourceFiles, FIleProcessor.Models.FileImportLog importLog)
        {
            importLog.ShouldNotBeNull();
            importLog.FileImportLogDateTime.ShouldBe(sourceImportLog.ImportLogDateTime);
            importLog.Files.Count.ShouldBe(sourceFiles.Count);

            foreach (var sourceFile in sourceFiles)
            {
                var file = importLog.Files.SingleOrDefault(impfile => impfile.FileId == sourceFile.FileId);
                file.ShouldNotBeNull();
                file.MerchantId.ShouldBe(sourceFile.MerchantId);
                file.FilePath.ShouldBe(sourceFile.FilePath);
                file.FileProfileId.ShouldBe(sourceFile.FileProfileId);
                file.OriginalFileName.ShouldBe(sourceFile.OriginalFileName);
                file.UserId.ShouldBe(sourceFile.UserId);
            }

        }


    }
}
