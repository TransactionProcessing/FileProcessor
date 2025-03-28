﻿using FileProcessor.Models;
using TransactionProcessor.Database.Entities;

namespace FileProcessor.BusinessLogic.Tests{
    using System.Collections.Generic;
    using System.Linq;
    using Common;
    using Shouldly;
    using Testing;
    using Xunit;

    public class ModelFactoryTests{
        #region Methods

        [Fact]
        public void ModelFactory_ConvertFrom_FileImportLog_IsConverted(){
            IModelFactory modelFactory = new ModelFactory();
            List<(FileImportLogFile, TransactionProcessor.Database.Entities.File, Merchant)> files = new List<(FileImportLogFile, TransactionProcessor.Database.Entities.File, Merchant)>();
            files.Add((TestData.FileImportLog1Files[0], TestData.Files1[0], TestData.Merchant));
            FileProcessor.Models.FileImportLog result = modelFactory.ConvertFrom(TestData.EstateId, TestData.FileImportLogs.First(), files);
            this.VerifyImportLog(TestData.FileImportLogs.First(), files, result);
        }

        [Fact]
        public void ModelFactory_ConvertFrom_FileImportLog_NoFiles_IsConverted(){
            IModelFactory modelFactory = new ModelFactory();
            List<(FileImportLogFile, TransactionProcessor.Database.Entities.File, Merchant)> files = new List<(FileImportLogFile, TransactionProcessor.Database.Entities.File, Merchant)>();
            FileProcessor.Models.FileImportLog result = modelFactory.ConvertFrom(TestData.EstateId, TestData.FileImportLogs.First(), files);

            this.VerifyImportLog(TestData.FileImportLogs.First(), files, result);
        }

        [Fact]
        public void ModelFactory_ConvertFrom_FileImportLog_NoImportLogs_IsConverted(){
            IModelFactory modelFactory = new ModelFactory();
            List<TransactionProcessor.Database.Entities.FileImportLog> fileImportLogs = new List<TransactionProcessor.Database.Entities.FileImportLog>();
            List<(FileImportLogFile, TransactionProcessor.Database.Entities.File, Merchant)> files = new List<(FileImportLogFile, TransactionProcessor.Database.Entities.File, Merchant)>();

            List<FileProcessor.Models.FileImportLog> result = modelFactory.ConvertFrom(TestData.EstateId, fileImportLogs, files);

            result.ShouldNotBeNull();
            result.ShouldBeEmpty();
        }

        [Fact]
        public void ModelFactory_ConvertFrom_FileImportLogs_IsConverted(){
            IModelFactory modelFactory = new ModelFactory();
            List<(FileImportLogFile, TransactionProcessor.Database.Entities.File, Merchant)> files = new List<(FileImportLogFile, TransactionProcessor.Database.Entities.File, Merchant)>();
            files.Add((TestData.FileImportLog1Files[0], TestData.Files1[0], TestData.Merchant));
            files.Add((TestData.FileImportLog1Files[1], TestData.Files1[1], TestData.Merchant));
            List<FileProcessor.Models.FileImportLog> result = modelFactory.ConvertFrom(TestData.EstateId, TestData.FileImportLogs, files);

            this.VerifyImportLogs(TestData.FileImportLogs, files, result);
        }

        [Fact]
        public void ModelFactory_ConvertFrom_FileImportLogs_NoFiles_IsConverted(){
            IModelFactory modelFactory = new ModelFactory();
            List<(FileImportLogFile, TransactionProcessor.Database.Entities.File, Merchant)> files = new List<(FileImportLogFile, TransactionProcessor.Database.Entities.File, Merchant)>();
            List<FileProcessor.Models.FileImportLog> result = modelFactory.ConvertFrom(TestData.EstateId, TestData.FileImportLogs, files);

            this.VerifyImportLogs(TestData.FileImportLogs, files, result);
        }

        private void VerifyImportLog(TransactionProcessor.Database.Entities.FileImportLog sourceImportLog, List<(TransactionProcessor.Database.Entities.FileImportLogFile, TransactionProcessor.Database.Entities.File, Merchant)> sourceFiles, FileProcessor.Models.FileImportLog importLog){
            importLog.ShouldNotBeNull();
            importLog.FileImportLogDateTime.ShouldBe(sourceImportLog.ImportLogDateTime);
            importLog.Files.Count.ShouldBe(sourceFiles.Count);

            foreach ((FileImportLogFile, TransactionProcessor.Database.Entities.File, Merchant) sourceFile in sourceFiles){
                ImportLogFile file = importLog.Files.SingleOrDefault(impfile => impfile.FileId == sourceFile.Item2.FileId);
                file.ShouldNotBeNull();
                file.MerchantId.ShouldBe(sourceFile.Item3.MerchantId);
                file.FilePath.ShouldBe(sourceFile.Item1.FilePath);
                file.FileProfileId.ShouldBe(sourceFile.Item1.FileProfileId);
                file.OriginalFileName.ShouldBe(sourceFile.Item1.OriginalFileName);
                file.UserId.ShouldBe(sourceFile.Item1.UserId);
            }
        }

        private void VerifyImportLogs(List<TransactionProcessor.Database.Entities.FileImportLog> sourceImportLogs, List<(TransactionProcessor.Database.Entities.FileImportLogFile, TransactionProcessor.Database.Entities.File, Merchant)> sourceImportLogFiles, List<FileProcessor.Models.FileImportLog> importLogs){
            importLogs.ShouldNotBeNull();
            importLogs.ShouldNotBeEmpty();
            importLogs.Count.ShouldBe(TestData.FileImportLogs.Count);
            foreach (TransactionProcessor.Database.Entities.FileImportLog fileImportLog in sourceImportLogs){
                FileProcessor.Models.FileImportLog importLog = importLogs.SingleOrDefault(i => i.FileImportLogId == fileImportLog.FileImportLogId);
                List<(FileImportLogFile, TransactionProcessor.Database.Entities.File, Merchant)> sourceFiles = sourceImportLogFiles.Where(s => s.Item1.FileImportLogId == fileImportLog.FileImportLogId).ToList();
                this.VerifyImportLog(fileImportLog, sourceFiles, importLog);
            }
        }

        #endregion
    }
}