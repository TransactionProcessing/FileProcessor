using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileProcessor.BusinessLogic.Tests
{
    using System.Threading;
    using Common;
    using EstateReporting.Database;
    using EstateReporting.Database.Entities;
    using FIleProcessor.Models;
    using Managers;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using Moq;
    using Shared.EntityFramework;
    using Shouldly;
    using Testing;
    using Xunit;
    using FileImportLog = EstateReporting.Database.Entities.FileImportLog;

    public class FileProcessingManagerTests
    {
        [Fact]
        public async Task FileProcessingManager_GetAllFileProfiles_AllFileProfilesReturned()
        {
            var fileProfiles = TestData.FileProfiles;
            var contextFactory = this.CreateMockContextFactory();
            Mock<IModelFactory> modelFactory = new Mock<IModelFactory>();
            FileProcessorManager manager = new FileProcessorManager(fileProfiles, contextFactory.Object,modelFactory.Object);

            var allFileProfiles = await manager.GetAllFileProfiles(CancellationToken.None);
            allFileProfiles.ShouldNotBeNull();
            allFileProfiles.ShouldNotBeEmpty();
        }

        [Fact]
        public async Task FileProcessingManager_GetFileProfile_FIleProfileReturned()
        {
            var fileProfiles = TestData.FileProfiles;
            var contextFactory = this.CreateMockContextFactory();
            Mock<IModelFactory> modelFactory = new Mock<IModelFactory>();
            FileProcessorManager manager = new FileProcessorManager(fileProfiles, contextFactory.Object, modelFactory.Object);

            var fileProfile = await manager.GetFileProfile(TestData.SafaricomFileProfileId, CancellationToken.None);
            fileProfile.ShouldNotBeNull();
            fileProfile.FileProfileId.ShouldBe(TestData.SafaricomFileProfileId);
        }

        [Fact]
        public async Task FileProcessingManager_GetFileImportLogs_NoMerchantId_ImportLogsReturned()
        {
            var fileProfiles = TestData.FileProfiles;
            var context = await this.GetContext(Guid.NewGuid().ToString("N"));
            var contextFactory = this.CreateMockContextFactory();
            contextFactory.Setup(c => c.GetContext(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(context);
            IModelFactory modelFactory = new ModelFactory();
            
            context.FileImportLogs.AddRange(TestData.FileImportLogs);
            context.FileImportLogFiles.AddRange(TestData.FileImportLog1Files);
            context.FileImportLogFiles.AddRange(TestData.FileImportLog2Files);
            context.SaveChanges();

            FileProcessorManager manager = new FileProcessorManager(fileProfiles, contextFactory.Object, modelFactory);

            var importLogs = await manager.GetFileImportLogs(TestData.EstateId, TestData.ImportLogStartDate, TestData.ImportLogEndDate, null, CancellationToken.None);

            this.VerifyImportLogs(importLogs);
        }

        [Fact]
        public async Task FileProcessingManager_GetFileImportLogs_WithMerchantId_ImportLogsReturned()
        {
            var fileProfiles = TestData.FileProfiles;
            var context = await this.GetContext(Guid.NewGuid().ToString("N"));
            var contextFactory = this.CreateMockContextFactory();
            contextFactory.Setup(c => c.GetContext(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(context);
            IModelFactory modelFactory = new ModelFactory();

            context.FileImportLogs.AddRange(TestData.FileImportLogs);
            context.FileImportLogFiles.AddRange(TestData.FileImportLog1Files);
            context.FileImportLogFiles.AddRange(TestData.FileImportLog2Files);
            context.SaveChanges();

            FileProcessorManager manager = new FileProcessorManager(fileProfiles, contextFactory.Object, modelFactory);

            var importLogs = await manager.GetFileImportLogs(TestData.EstateId, TestData.ImportLogStartDate, TestData.ImportLogEndDate, TestData.MerchantId, CancellationToken.None);

            this.VerifyImportLogs(importLogs, TestData.MerchantId);
        }

        private void VerifyImportLogs(List<FIleProcessor.Models.FileImportLog> importLogs, Guid? merchantId = null)
        {
            importLogs.ShouldNotBeNull();
            importLogs.ShouldNotBeEmpty();
            importLogs.Count.ShouldBe(TestData.FileImportLogs.Count);
            foreach (FileImportLog fileImportLog in TestData.FileImportLogs)
            {
                var importLog = importLogs.SingleOrDefault(i => i.FileImportLogId == fileImportLog.FileImportLogId);
                importLog.ShouldNotBeNull();
                importLog.FileImportLogDateTime.ShouldBe(fileImportLog.ImportLogDateTime);
                importLog.Files.Count.ShouldBe(importLog.Files.Count);

                List<ImportLogFile> filesToVerify = importLog.Files;
                if (merchantId.HasValue)
                {
                    filesToVerify = filesToVerify.Where(f => f.MerchantId == merchantId.Value).ToList();
                }

                foreach (ImportLogFile importLogFile in filesToVerify)
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

        //[Fact]
        //public async Task FileProcessingManager_GetFileImportLogs_WithMerchantId_ImportLogsReturned()
        //{
        //    var fileProfiles = TestData.FileProfiles;
        //    var contextFactory = this.CreateMockContextFactory();
        //    IModelFactory modelFactory = new ModelFactory();
        //    FileProcessorManager manager = new FileProcessorManager(fileProfiles, contextFactory.Object, modelFactory);

        //    var importLogs = await manager.GetFileImportLogs(TestData.EstateId, TestData.ImportLogStartDate, TestData.ImportLogEndDate, TestData.MerchantId, CancellationToken.None);

        //    importLogs.ShouldNotBeNull();
        //    importLogs.ShouldNotBeEmpty();
        //    importLogs.Count.ShouldBe(2);


        //}

        private Mock<Shared.EntityFramework.IDbContextFactory<EstateReportingContext>> CreateMockContextFactory()
        {
            return new Mock<Shared.EntityFramework.IDbContextFactory<EstateReportingContext>>();
        }

        private async Task<EstateReportingContext> GetContext(String databaseName)
        {
            EstateReportingContext context = null;
            
            DbContextOptionsBuilder<EstateReportingContext> builder = new DbContextOptionsBuilder<EstateReportingContext>()
                                                                      .UseInMemoryDatabase(databaseName)
                                                                      .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning));
            context = new EstateReportingContext(builder.Options);
        

            return context;
        }
    }
}
