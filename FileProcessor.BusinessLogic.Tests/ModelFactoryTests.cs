using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileProcessor.BusinessLogic.Tests
{
    using Common;
    using EstateManagement.Database.Entities;
    using FIleProcessor.Models;
    using Microsoft.EntityFrameworkCore;
    using Shouldly;
    using Testing;
    using Xunit;
    using FileImportLog = FIleProcessor.Models.FileImportLog;

    public class ModelFactoryTests
    {
        /*
        [Fact]
        public void ModelFactory_ConvertFrom_FileImportLogs_IsConverted()
        {
            IModelFactory modelFactory = new ModelFactory();
            List<(FileImportLogFile, File)> files = new List<(FileImportLogFile, File)>();
            files.Add((TestData.FileImportLog1Files[0], TestData.Files1[0]));
            files.Add((TestData.FileImportLog1Files[1], TestData.Files1[1]));
            List<FileImportLog> result = modelFactory.ConvertFrom(TestData.EstateId, TestData.MerchantId, TestData.FileImportLogs, files);

            this.VerifyImportLogs(TestData.FileImportLogs, files, result);
        }

        [Fact]
        public void ModelFactory_ConvertFrom_FileImportLogs_NoFiles_IsConverted()
        {
            IModelFactory modelFactory = new ModelFactory();
            List<(FileImportLogFile, File)> files = new List<(FileImportLogFile, File)>();
            List<FileImportLog> result = modelFactory.ConvertFrom(TestData.EstateId, TestData.MerchantId, TestData.FileImportLogs, files);

            this.VerifyImportLogs(TestData.FileImportLogs, files, result);
        }

        [Fact]
        public void ModelFactory_ConvertFrom_FileImportLog_IsConverted()
        {
            IModelFactory modelFactory = new ModelFactory();
            //List<FileImportLogFile> files = new List<FileImportLogFile>();
            //files.AddRange(TestData.FileImportLog1Files);
            //FileImportLog result = modelFactory.ConvertFrom(TestData.FileImportLogs.First(), files);
            List<(FileImportLogFile, File)> files = new List<(FileImportLogFile, File)>();
            files.Add((TestData.FileImportLog1Files[0], TestData.Files1[0]));
            //files.Add((TestData.FileImportLog1Files[1], TestData.Files1[1]));
            //List<FileImportLog> result = modelFactory.ConvertFrom(TestData.EstateId, TestData.MerchantId, TestData.FileImportLogs, files);
            FileImportLog result = modelFactory.ConvertFrom(TestData.EstateId, TestData.MerchantId, TestData.FileImportLogs.First(), files);
            this.VerifyImportLog(TestData.FileImportLogs.First(), files, result);
        }

        //[Fact]
        //public void ModelFactory_ConvertFrom_FileImportLog_NoFiles_IsConverted()
        //{
        //    IModelFactory modelFactory = new ModelFactory();
        //    List<FileImportLogFile> files = new List<FileImportLogFile>();
        //    FileImportLog result = modelFactory.ConvertFrom(TestData.EstateId,TestData.MerchantId, TestData.FileImportLogs.First(), files);

        //    this.VerifyImportLog(TestData.FileImportLogs.First(), files, result);
        //}

        //[Fact]
        //public void ModelFactory_ConvertFrom_FileImportLog_NoImportLogs_IsConverted()
        //{
        //    IModelFactory modelFactory = new ModelFactory();
        //    List<EstateManagement.Database.Entities.FileImportLog> fileImportLogs = new List<EstateManagement.Database.Entities.FileImportLog>();
        //    List<FileImportLogFile> files = new List<FileImportLogFile>();
        //    List<(FileImportLogFile, File)> f = new List<(FileImportLogFile, File)>();
        //    f.Add();
        //    List<FileImportLog> result = modelFactory.ConvertFrom(TestData.EstateId, TestData.MerchantId, fileImportLogs, files);

        //    result.ShouldNotBeNull();
        //    result.ShouldBeEmpty();
        //}

        private void VerifyImportLogs(List<EstateManagement.Database.Entities.FileImportLog> sourceImportLogs, List<(FileImportLogFile, File)> sourceImportLogFiles, List<FIleProcessor.Models.FileImportLog> importLogs)
        {
            importLogs.ShouldNotBeNull();
            importLogs.ShouldNotBeEmpty();
            importLogs.Count.ShouldBe(TestData.FileImportLogs.Count);
            foreach (EstateManagement.Database.Entities.FileImportLog fileImportLog in sourceImportLogs)
            {
                var importLog = importLogs.SingleOrDefault(i => i.FileImportLogId == fileImportLog.FileImportLogId);
                var sourceFiles = sourceImportLogFiles.Where(s => s.Item1.FileImportLogReportingId == fileImportLog.FileImportLogReportingId).ToList();
                VerifyImportLog(fileImportLog, sourceFiles, importLog);
            }
        }

        private void VerifyImportLog(EstateManagement.Database.Entities.FileImportLog sourceImportLog, List<(FileImportLogFile, File)> sourceFiles, FIleProcessor.Models.FileImportLog importLog)
        {
            importLog.ShouldNotBeNull();
            importLog.FileImportLogDateTime.ShouldBe(sourceImportLog.ImportLogDateTime);
            importLog.Files.Count.ShouldBe(sourceFiles.Count);

            foreach (var sourceFile in sourceFiles)
            {
                var file = importLog.Files.SingleOrDefault(impfile => impfile.FileId == sourceFile.Item2.FileId);
                file.ShouldNotBeNull();
                //file.MerchantId.ShouldBe(sourceFile.Item1..MerchantId);
                file.FilePath.ShouldBe(sourceFile.Item1.FilePath);
                file.FileProfileId.ShouldBe(sourceFile.Item1.FileProfileId);
                file.OriginalFileName.ShouldBe(sourceFile.Item1.OriginalFileName);
                file.UserId.ShouldBe(sourceFile.Item1.UserId);
            }

        }
        */

    }
}
