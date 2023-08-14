namespace FileProcessor.BusinessLogic.Tests{
    using System.Collections.Generic;
    using System.Linq;
    using Common;
    using EstateManagement.Database.Entities;
    using FIleProcessor.Models;
    using Shouldly;
    using Testing;
    using Xunit;
    using FileImportLog = EstateManagement.Database.Entities.FileImportLog;

    public class ModelFactoryTests{
        #region Methods

        [Fact]
        public void ModelFactory_ConvertFrom_FileImportLog_IsConverted(){
            IModelFactory modelFactory = new ModelFactory();
            List<(FileImportLogFile, File, Merchant)> files = new List<(FileImportLogFile, File, Merchant)>();
            files.Add((TestData.FileImportLog1Files[0], TestData.Files1[0], TestData.Merchant));
            FIleProcessor.Models.FileImportLog result = modelFactory.ConvertFrom(TestData.EstateId, TestData.FileImportLogs.First(), files);
            this.VerifyImportLog(TestData.FileImportLogs.First(), files, result);
        }

        [Fact]
        public void ModelFactory_ConvertFrom_FileImportLog_NoFiles_IsConverted(){
            IModelFactory modelFactory = new ModelFactory();
            List<(FileImportLogFile, File, Merchant)> files = new List<(FileImportLogFile, File, Merchant)>();
            FIleProcessor.Models.FileImportLog result = modelFactory.ConvertFrom(TestData.EstateId, TestData.FileImportLogs.First(), files);

            this.VerifyImportLog(TestData.FileImportLogs.First(), files, result);
        }

        [Fact]
        public void ModelFactory_ConvertFrom_FileImportLog_NoImportLogs_IsConverted(){
            IModelFactory modelFactory = new ModelFactory();
            List<FileImportLog> fileImportLogs = new List<FileImportLog>();
            List<(FileImportLogFile, File, Merchant)> files = new List<(FileImportLogFile, File, Merchant)>();

            List<FIleProcessor.Models.FileImportLog> result = modelFactory.ConvertFrom(TestData.EstateId, fileImportLogs, files);

            result.ShouldNotBeNull();
            result.ShouldBeEmpty();
        }

        [Fact]
        public void ModelFactory_ConvertFrom_FileImportLogs_IsConverted(){
            IModelFactory modelFactory = new ModelFactory();
            List<(FileImportLogFile, File, Merchant)> files = new List<(FileImportLogFile, File, Merchant)>();
            files.Add((TestData.FileImportLog1Files[0], TestData.Files1[0], TestData.Merchant));
            files.Add((TestData.FileImportLog1Files[1], TestData.Files1[1], TestData.Merchant));
            List<FIleProcessor.Models.FileImportLog> result = modelFactory.ConvertFrom(TestData.EstateId, TestData.FileImportLogs, files);

            this.VerifyImportLogs(TestData.FileImportLogs, files, result);
        }

        [Fact]
        public void ModelFactory_ConvertFrom_FileImportLogs_NoFiles_IsConverted(){
            IModelFactory modelFactory = new ModelFactory();
            List<(FileImportLogFile, File, Merchant)> files = new List<(FileImportLogFile, File, Merchant)>();
            List<FIleProcessor.Models.FileImportLog> result = modelFactory.ConvertFrom(TestData.EstateId, TestData.FileImportLogs, files);

            this.VerifyImportLogs(TestData.FileImportLogs, files, result);
        }

        private void VerifyImportLog(FileImportLog sourceImportLog, List<(FileImportLogFile, File, Merchant)> sourceFiles, FIleProcessor.Models.FileImportLog importLog){
            importLog.ShouldNotBeNull();
            importLog.FileImportLogDateTime.ShouldBe(sourceImportLog.ImportLogDateTime);
            importLog.Files.Count.ShouldBe(sourceFiles.Count);

            foreach ((FileImportLogFile, File, Merchant) sourceFile in sourceFiles){
                ImportLogFile file = importLog.Files.SingleOrDefault(impfile => impfile.FileId == sourceFile.Item2.FileId);
                file.ShouldNotBeNull();
                file.MerchantId.ShouldBe(sourceFile.Item3.MerchantId);
                file.FilePath.ShouldBe(sourceFile.Item1.FilePath);
                file.FileProfileId.ShouldBe(sourceFile.Item1.FileProfileId);
                file.OriginalFileName.ShouldBe(sourceFile.Item1.OriginalFileName);
                file.UserId.ShouldBe(sourceFile.Item1.UserId);
            }
        }

        private void VerifyImportLogs(List<FileImportLog> sourceImportLogs, List<(FileImportLogFile, File, Merchant)> sourceImportLogFiles, List<FIleProcessor.Models.FileImportLog> importLogs){
            importLogs.ShouldNotBeNull();
            importLogs.ShouldNotBeEmpty();
            importLogs.Count.ShouldBe(TestData.FileImportLogs.Count);
            foreach (FileImportLog fileImportLog in sourceImportLogs){
                FIleProcessor.Models.FileImportLog importLog = importLogs.SingleOrDefault(i => i.FileImportLogId == fileImportLog.FileImportLogId);
                List<(FileImportLogFile, File, Merchant)> sourceFiles = sourceImportLogFiles.Where(s => s.Item1.FileImportLogReportingId == fileImportLog.FileImportLogReportingId).ToList();
                this.VerifyImportLog(fileImportLog, sourceFiles, importLog);
            }
        }

        #endregion
    }
}