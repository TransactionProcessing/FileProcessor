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
    using FileAggregate;
    using FIleProcessor.Models;
    using Managers;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using Moq;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EntityFramework;
    using Shared.EventStore.Aggregate;
    using Shared.Exceptions;
    using Shouldly;
    using Testing;
    using Xunit;
    using FileImportLog = EstateReporting.Database.Entities.FileImportLog;
    using FileLine = FIleProcessor.Models.FileLine;

    public class FileProcessingManagerTests
    {
        [Fact]
        public async Task FileProcessingManager_GetAllFileProfiles_AllFileProfilesReturned()
        {
            var fileProfiles = TestData.FileProfiles;
            var contextFactory = this.CreateMockContextFactory();
            Mock<IModelFactory> modelFactory = new Mock<IModelFactory>();
            Mock<IAggregateRepository<FileAggregate, DomainEvent>> fileAggregateRepository =
                new Mock<IAggregateRepository<FileAggregate, DomainEvent>>();
            FileProcessorManager manager = new FileProcessorManager(fileProfiles, contextFactory.Object,modelFactory.Object, fileAggregateRepository.Object);

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
            Mock<IAggregateRepository<FileAggregate, DomainEvent>> fileAggregateRepository =
                new Mock<IAggregateRepository<FileAggregate, DomainEvent>>();
            FileProcessorManager manager = new FileProcessorManager(fileProfiles, contextFactory.Object, modelFactory.Object, fileAggregateRepository.Object);

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

            Mock<IAggregateRepository<FileAggregate, DomainEvent>> fileAggregateRepository =
                new Mock<IAggregateRepository<FileAggregate, DomainEvent>>();
            FileProcessorManager manager = new FileProcessorManager(fileProfiles, contextFactory.Object, modelFactory, fileAggregateRepository.Object);

            var importLogs = await manager.GetFileImportLogs(TestData.EstateId, TestData.ImportLogStartDate, TestData.ImportLogEndDate, null, CancellationToken.None);

            this.VerifyImportLogs(TestData.FileImportLogs,importLogs);
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

            Mock<IAggregateRepository<FileAggregate, DomainEvent>> fileAggregateRepository =
                new Mock<IAggregateRepository<FileAggregate, DomainEvent>>();
            FileProcessorManager manager = new FileProcessorManager(fileProfiles, contextFactory.Object, modelFactory, fileAggregateRepository.Object);

            var importLogs = await manager.GetFileImportLogs(TestData.EstateId, TestData.ImportLogStartDate, TestData.ImportLogEndDate, TestData.MerchantId, CancellationToken.None);

            this.VerifyImportLogs(TestData.FileImportLogs,importLogs, TestData.MerchantId);
        }

        [Fact]
        public async Task FileProcessingManager_GetFileImportLog_NoMerchantId_ImportLogReturned()
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

            Mock<IAggregateRepository<FileAggregate, DomainEvent>> fileAggregateRepository =
                new Mock<IAggregateRepository<FileAggregate, DomainEvent>>();
            FileProcessorManager manager = new FileProcessorManager(fileProfiles, contextFactory.Object, modelFactory, fileAggregateRepository.Object);

            var importLog = await manager.GetFileImportLog(TestData.FileImportLogId1, TestData.EstateId, null, CancellationToken.None);

            this.VerifyImportLog(TestData.FileImportLogs.First(), importLog);
        }

        [Fact]
        public async Task FileProcessingManager_GetFileImportLog_WithMerchantId_ImportLogReturned()
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

            Mock<IAggregateRepository<FileAggregate, DomainEvent>> fileAggregateRepository =
                new Mock<IAggregateRepository<FileAggregate, DomainEvent>>();
            FileProcessorManager manager = new FileProcessorManager(fileProfiles, contextFactory.Object, modelFactory, fileAggregateRepository.Object);

            var importLog = await manager.GetFileImportLog(TestData.FileImportLogId1, TestData.EstateId, TestData.MerchantId, CancellationToken.None);

            this.VerifyImportLog(TestData.FileImportLogs.First(),importLog, TestData.MerchantId);
        }

        [Fact]
        public async Task FileProcessingManager_GetFile_FileReturned()
        {
            List<FileProfile> fileProfiles = new List<FileProfile>();
            var context = await this.GetContext(Guid.NewGuid().ToString("N"));
            var contextFactory = this.CreateMockContextFactory();
            contextFactory.Setup(c => c.GetContext(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(context);
            IModelFactory modelFactory = new ModelFactory();

            Mock<IAggregateRepository<FileAggregate, DomainEvent>> fileAggregateRepository =
                new Mock<IAggregateRepository<FileAggregate, DomainEvent>>();
            fileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetFileAggregateWithLines);
            FileProcessorManager manager = new FileProcessorManager(fileProfiles, contextFactory.Object, modelFactory, fileAggregateRepository.Object);

             var fileDetails = await manager.GetFile(TestData.FileId, TestData.EstateId, CancellationToken.None);

             this.VerifyFile(TestData.GetFileAggregateWithLines(), fileDetails);
             fileDetails.MerchantName.ShouldBeNull();
             fileDetails.UserEmailAddress.ShouldBeNull();
             fileDetails.FileProfileName.ShouldBeNull();
        }

        [Fact]
        public async Task FileProcessingManager_GetFile_FileReturnedWithMerchantName()
        {
            List<FileProfile> fileProfiles = new List<FileProfile>();
            var context = await this.GetContext(Guid.NewGuid().ToString("N"));
            context.Merchants.Add(new Merchant
            {
                EstateId = TestData.EstateId,
                MerchantId = TestData.MerchantId,
                Name = TestData.MerchantName
            });
            context.SaveChanges();
            var contextFactory = this.CreateMockContextFactory();
            contextFactory.Setup(c => c.GetContext(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(context);
            IModelFactory modelFactory = new ModelFactory();

            Mock<IAggregateRepository<FileAggregate, DomainEvent>> fileAggregateRepository =
                new Mock<IAggregateRepository<FileAggregate, DomainEvent>>();
            fileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetFileAggregateWithLines);
            FileProcessorManager manager = new FileProcessorManager(fileProfiles, contextFactory.Object, modelFactory, fileAggregateRepository.Object);

            var fileDetails = await manager.GetFile(TestData.FileId, TestData.EstateId, CancellationToken.None);

            this.VerifyFile(TestData.GetFileAggregateWithLines(), fileDetails);
            fileDetails.MerchantName.ShouldBe(TestData.MerchantName);
        }

        [Fact]
        public async Task FileProcessingManager_GetFile_FileReturnedWithUserEmailAddress()
        {
            List<FileProfile> fileProfiles = new List<FileProfile>();
            var context = await this.GetContext(Guid.NewGuid().ToString("N"));
            context.EstateSecurityUsers.Add(new EstateSecurityUser()
            {
                EstateId = TestData.EstateId,
                SecurityUserId = TestData.UserId,
                EmailAddress = TestData.UserEmailAddress
            });
            context.SaveChanges();
            var contextFactory = this.CreateMockContextFactory();
            contextFactory.Setup(c => c.GetContext(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(context);
            IModelFactory modelFactory = new ModelFactory();

            Mock<IAggregateRepository<FileAggregate, DomainEvent>> fileAggregateRepository =
                new Mock<IAggregateRepository<FileAggregate, DomainEvent>>();
            fileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetFileAggregateWithLines);
            FileProcessorManager manager = new FileProcessorManager(fileProfiles, contextFactory.Object, modelFactory, fileAggregateRepository.Object);

            var fileDetails = await manager.GetFile(TestData.FileId, TestData.EstateId, CancellationToken.None);

            this.VerifyFile(TestData.GetFileAggregateWithLines(), fileDetails);
            fileDetails.UserEmailAddress.ShouldBe(TestData.UserEmailAddress);
        }

        [Fact]
        public async Task FileProcessingManager_GetFile_FileReturnedWithFileProfileName()
        {
            var fileProfiles = new List<FileProfile>
            {
                new FileProfile(TestData.FileProfileId,                 
                    TestData.SafaricomProfileName,
                    TestData.SafaricomListeningDirectory,
                    TestData.SafaricomRequestType,
                    TestData.SafaricomOperatorIdentifier,
                    TestData.SafaricomLineTerminator,
                    TestData.SafaricomFileFormatHandler)
            };
            var context = await this.GetContext(Guid.NewGuid().ToString("N"));
            context.EstateSecurityUsers.Add(new EstateSecurityUser()
            {
                EstateId = TestData.EstateId,
                SecurityUserId = TestData.UserId,
                EmailAddress = TestData.UserEmailAddress
            });
            context.SaveChanges();
            var contextFactory = this.CreateMockContextFactory();
            contextFactory.Setup(c => c.GetContext(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(context);
            IModelFactory modelFactory = new ModelFactory();

            Mock<IAggregateRepository<FileAggregate, DomainEvent>> fileAggregateRepository =
                new Mock<IAggregateRepository<FileAggregate, DomainEvent>>();
            fileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetFileAggregateWithLines);
            FileProcessorManager manager = new FileProcessorManager(fileProfiles, contextFactory.Object, modelFactory, fileAggregateRepository.Object);

            var fileDetails = await manager.GetFile(TestData.FileId, TestData.EstateId, CancellationToken.None);

            this.VerifyFile(TestData.GetFileAggregateWithLines(), fileDetails);
            fileDetails.FileProfileName.ShouldBe(TestData.SafaricomProfileName);
        }

        [Fact]
        public async Task FileProcessingManager_GetFile_FileNotFound_ErrorThrown()
        {
            var fileProfiles = TestData.FileProfiles;
            var context = await this.GetContext(Guid.NewGuid().ToString("N"));
            var contextFactory = this.CreateMockContextFactory();
            contextFactory.Setup(c => c.GetContext(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(context);
            IModelFactory modelFactory = new ModelFactory();

            Mock<IAggregateRepository<FileAggregate, DomainEvent>> fileAggregateRepository =
                new Mock<IAggregateRepository<FileAggregate, DomainEvent>>();
            fileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetEmptyFileAggregate);
            FileProcessorManager manager = new FileProcessorManager(fileProfiles, contextFactory.Object, modelFactory, fileAggregateRepository.Object);

            Should.Throw<NotFoundException>(async () =>
                                            {
                                                await manager.GetFile(TestData.FileId, TestData.EstateId, CancellationToken.None);
                                            });
        }


        private void VerifyFile(FileAggregate source, FileDetails fileDetails)
        {
            var fileModel = source.GetFile();

            fileDetails.FileId.ShouldBe(fileModel.FileId);
            fileDetails.FileImportLogId.ShouldBe(fileModel.FileImportLogId);
            fileDetails.FileLocation.ShouldBe(fileModel.FileLocation);
            fileDetails.FileProfileId.ShouldBe(fileModel.FileProfileId);
            fileDetails.MerchantId.ShouldBe(fileModel.MerchantId);
            fileDetails.ProcessingCompleted.ShouldBe(fileModel.ProcessingCompleted);
            fileDetails.UserId.ShouldBe(fileModel.UserId);
            fileDetails.EstateId.ShouldBe(fileModel.EstateId);

            fileDetails.ProcessingSummary.ShouldNotBeNull();
            fileDetails.ProcessingSummary.FailedLines.ShouldBe(fileModel.ProcessingSummary.FailedLines);
            fileDetails.ProcessingSummary.IgnoredLines.ShouldBe(fileModel.ProcessingSummary.IgnoredLines);
            fileDetails.ProcessingSummary.NotProcessedLines.ShouldBe(fileModel.ProcessingSummary.NotProcessedLines);
            fileDetails.ProcessingSummary.SuccessfullyProcessedLines.ShouldBe(fileModel.ProcessingSummary.SuccessfullyProcessedLines);
            fileDetails.ProcessingSummary.TotalLines.ShouldBe(fileModel.ProcessingSummary.TotalLines);

            foreach (FileLine fileModelFileLine in fileModel.FileLines)
            {
                FileLine? fileLineToVerify = fileDetails.FileLines.SingleOrDefault(f => f.LineNumber == fileModelFileLine.LineNumber);
                fileLineToVerify.ShouldNotBeNull();
                fileLineToVerify.LineData.ShouldBe(fileModelFileLine.LineData);
                fileLineToVerify.TransactionId.ShouldBe(fileModelFileLine.TransactionId);
                fileLineToVerify.ProcessingResult.ShouldBe(fileModelFileLine.ProcessingResult);
            }
        }

        private void VerifyImportLogs(List<FileImportLog> source,  List<FIleProcessor.Models.FileImportLog> importLogs, Guid? merchantId = null)
        {
            importLogs.ShouldNotBeNull();
            importLogs.ShouldNotBeEmpty();
            importLogs.Count.ShouldBe(TestData.FileImportLogs.Count);
            foreach (FileImportLog fileImportLog in source)
            {
                var importLog = importLogs.SingleOrDefault(i => i.FileImportLogId == fileImportLog.FileImportLogId);
                VerifyImportLog(fileImportLog, importLog, merchantId);
            }
        }

        private void VerifyImportLog(FileImportLog source, FIleProcessor.Models.FileImportLog importLog, Guid? merchantId = null)
        {
            importLog.ShouldNotBeNull();
            importLog.FileImportLogDateTime.ShouldBe(source.ImportLogDateTime);
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
        
        private Mock<Shared.EntityFramework.IDbContextFactory<EstateReportingGenericContext>> CreateMockContextFactory()
        {
            return new Mock<Shared.EntityFramework.IDbContextFactory<EstateReportingGenericContext>>();
        }

        private async Task<EstateReportingGenericContext> GetContext(String databaseName)
        {
            EstateReportingGenericContext context = null;
            
            DbContextOptionsBuilder<EstateReportingGenericContext> builder = new DbContextOptionsBuilder<EstateReportingGenericContext>()
                                                                             .UseInMemoryDatabase(databaseName)
                                                                             .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning));
            context = new EstateReportingSqlServerContext(builder.Options);
        
            return context;
        }
    }
}
