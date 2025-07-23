using FileProcessor.Models;
using SimpleResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionProcessor.Database.Contexts;
using TransactionProcessor.Database.Entities;

namespace FileProcessor.BusinessLogic.Tests
{
    using Common;
    using FileAggregate;
    using FileProcessor.Common;
    using FileProcessor.Models;
    using Managers;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using Microsoft.EntityFrameworkCore.Internal;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EntityFramework;
    using Shared.EventStore.Aggregate;
    using Shared.Exceptions;
    using Shouldly;
    using System.Threading;
    using Testing;
    using Xunit;
    using FileImportLog = FileProcessor.Models.FileImportLog;
    using FileLine = FileProcessor.Models.FileLine;

    public class FileProcessingManagerTests {
        private Mock<IAggregateRepository<FileAggregate, DomainEvent>> FileAggregateRepository;
        private FileProcessorManager Manager;
        private Mock<IDbContextResolver<EstateManagementContext>> DbContextFactory;
        private EstateManagementContext Context;
        public FileProcessingManagerTests() {
            List<FileProfile> fileProfiles = TestData.FileProfiles;
            this.DbContextFactory = new Mock<IDbContextResolver<EstateManagementContext>>();
            this.Context = this.GetContext(Guid.NewGuid().ToString("N"));
            ServiceCollection services = new ServiceCollection();
            services.AddTransient<EstateManagementContext>(_ => this.Context);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            IServiceScope scope = serviceProvider.CreateScope();
            this.DbContextFactory.Setup(d => d.Resolve(It.IsAny<String>(), It.IsAny<String>())).Returns(new ResolvedDbContext<EstateManagementContext>(scope));

            var modelFactory= new Common.ModelFactory();
            this.FileAggregateRepository = new Mock<IAggregateRepository<FileAggregate, DomainEvent>>();
            this.Manager = new FileProcessorManager(fileProfiles, this.DbContextFactory.Object, modelFactory, this.FileAggregateRepository.Object);
        }

        private EstateManagementContext GetContext(String databaseName)
        {
            EstateManagementContext context = null;
            DbContextOptionsBuilder<EstateManagementContext> builder = new DbContextOptionsBuilder<EstateManagementContext>().UseInMemoryDatabase(databaseName).ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning));
            return new EstateManagementContext(builder.Options);
        }

        [Fact]
        public async Task FileProcessingManager_GetAllFileProfiles_AllFileProfilesReturned()
        {
            Result<List<FileProfile>> allFileProfiles = await this.Manager.GetAllFileProfiles(CancellationToken.None);
            allFileProfiles.ShouldNotBeNull();
            allFileProfiles.IsSuccess.ShouldBeTrue();
            allFileProfiles.Data.ShouldNotBeEmpty();
        }

        [Fact]
        public async Task FileProcessingManager_GetFileProfile_FileProfileReturned()
        {
            Result<FileProfile> fileProfile = await this.Manager.GetFileProfile(TestData.SafaricomFileProfileId, CancellationToken.None);
            fileProfile.ShouldNotBeNull();
            fileProfile.IsSuccess.ShouldBeTrue();
            fileProfile.Data.FileProfileId.ShouldBe(TestData.SafaricomFileProfileId);
        }

        [Fact]
        public async Task FileProcessingManager_GetFileImportLogs_NoMerchantId_ImportLogsReturned()
        {
            this.Context.FileImportLogs.AddRange(TestData.FileImportLogs);
            this.Context.FileImportLogFiles.AddRange(TestData.FileImportLog1Files);
            this.Context.FileImportLogFiles.AddRange(TestData.FileImportLog2Files);
            this.Context.Merchants.Add(new Merchant { MerchantId = TestData.MerchantId, Name = TestData.MerchantName });
            await this.Context.SaveChangesAsync();

            Result<List<FileImportLog>> getFileImportLogsResult = await this.Manager.GetFileImportLogs(TestData.EstateId, TestData.ImportLogStartDate, TestData.ImportLogEndDate, null, CancellationToken.None);
            getFileImportLogsResult.IsSuccess.ShouldBeTrue();
            List<FileImportLog> importLogs = getFileImportLogsResult.Data;
            this.VerifyImportLogs(TestData.FileImportLogs,importLogs);
        }

        [Fact]
        public async Task FileProcessingManager_GetFileImportLogs_WithMerchantId_ImportLogsReturned()
        {
            this.Context.FileImportLogs.AddRange(TestData.FileImportLogs);
            this.Context.FileImportLogFiles.AddRange(TestData.FileImportLog1Files);
            this.Context.FileImportLogFiles.AddRange(TestData.FileImportLog2Files);
            this.Context.Files.AddRange(TestData.Files1);
            this.Context.Merchants.Add(new Merchant { MerchantId = TestData.MerchantId, Name = TestData.MerchantName });
            await this.Context.SaveChangesAsync();

            Result<List<FileImportLog>> importLogs = await this.Manager.GetFileImportLogs(TestData.EstateId, TestData.ImportLogStartDate, TestData.ImportLogEndDate, TestData.MerchantId, CancellationToken.None);

            this.VerifyImportLogs(TestData.FileImportLogs,importLogs, TestData.MerchantId);
        }

        [Fact]
        public async Task FileProcessingManager_GetFileImportLog_NoMerchantId_ImportLogReturned()
        {
            this.Context.FileImportLogs.AddRange(TestData.FileImportLogs);
            this.Context.FileImportLogFiles.AddRange(TestData.FileImportLog1Files);
            this.Context.FileImportLogFiles.AddRange(TestData.FileImportLog2Files);
            this.Context.Merchants.Add(new Merchant { MerchantId = TestData.MerchantId, Name = TestData.MerchantName });
            this.Context.Files.AddRange(TestData.Files1);
            await this.Context.SaveChangesAsync();

            Result<FileImportLog> importLog = await this.Manager.GetFileImportLog(TestData.FileImportLogId1, TestData.EstateId, null, CancellationToken.None);

            this.VerifyImportLog(TestData.FileImportLogs.First(), importLog);
        }

        [Fact]
        public async Task FileProcessingManager_GetFileImportLog_WithMerchantId_ImportLogReturned()
        {
            this.Context.FileImportLogs.AddRange(TestData.FileImportLogs);
            this.Context.FileImportLogFiles.AddRange(TestData.FileImportLog1Files);
            this.Context.FileImportLogFiles.AddRange(TestData.FileImportLog2Files);
            this.Context.Merchants.Add(new Merchant { MerchantId = TestData.MerchantId, Name = TestData.MerchantName});
            this.Context.Files.AddRange(TestData.Files1);
            await Context.SaveChangesAsync();

            Result<FileImportLog> importLog = await this.Manager.GetFileImportLog(TestData.FileImportLogId1, TestData.EstateId, TestData.MerchantId, CancellationToken.None);

            this.VerifyImportLog(TestData.FileImportLogs.First(),importLog, TestData.MerchantId);
        }

        [Fact]
        public async Task FileProcessingManager_GetFile_FileReturned()
        {
            List<FileProfile> fileProfiles = new List<FileProfile>
            {
                TestData.FileProfile
            };
            this.FileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetFileAggregateWithLines()));
            
             Result<FileDetails> fileDetails = await this.Manager.GetFile(TestData.FileId, TestData.EstateId, CancellationToken.None);

             fileDetails.IsSuccess.ShouldBeTrue();
             this.VerifyFile(TestData.GetFileAggregateWithLines(), fileDetails);
             fileDetails.Data.MerchantName.ShouldBeNull();
             fileDetails.Data.UserEmailAddress.ShouldBeNull();
        }

        [Fact]
        public async Task FileProcessingManager_GetFile_FileAggregateFailed_ErrorReturned()
        {
            FileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure());
            
            Result<FileDetails> fileDetails = await this.Manager.GetFile(TestData.FileId, TestData.EstateId, CancellationToken.None);

            fileDetails.IsFailed.ShouldBeTrue();
        }

        [Fact]
        public async Task FileProcessingManager_GetFile_FileReturnedWithMerchantName()
        {
            this.Context.Merchants.Add(new Merchant
            {
                EstateId = TestData.EstateId,
                MerchantReportingId = TestData.MerchantReportingId,
                MerchantId = TestData.MerchantId,
                Name = TestData.MerchantName
            });
            await this.Context.SaveChangesAsync();
            FileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetFileAggregateWithLines()));
            
            Result<FileDetails> fileDetails = await this.Manager.GetFile(TestData.FileId, TestData.EstateId, CancellationToken.None);
            fileDetails.IsSuccess.ShouldBeTrue();

            this.VerifyFile(TestData.GetFileAggregateWithLines(), fileDetails);
            fileDetails.Data.MerchantName.ShouldBe(TestData.MerchantName);
        }

        [Fact]
        public async Task FileProcessingManager_GetFile_FileReturnedWithUserEmailAddress()
        {
            Context.EstateSecurityUsers.Add(new EstateSecurityUser()
            {
                EstateId = TestData.EstateId,
                SecurityUserId = TestData.UserId,
                EmailAddress = TestData.UserEmailAddress
            });
            await this.Context.SaveChangesAsync();
            
            FileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetFileAggregateWithLines()));
            
            Result<FileDetails> fileDetails = await this.Manager.GetFile(TestData.FileId, TestData.EstateId, CancellationToken.None);
            fileDetails.IsSuccess.ShouldBeTrue();
            this.VerifyFile(TestData.GetFileAggregateWithLines(), fileDetails);
            fileDetails.Data.UserEmailAddress.ShouldBe(TestData.UserEmailAddress);
        }

        [Fact]
        public async Task FileProcessingManager_GetFile_FileReturnedWithFileProfileName()
        {
            List<FileProfile> fileProfiles = new List<FileProfile>
            {
                new FileProfile(TestData.FileProfileId,                 
                    TestData.SafaricomProfileName,
                    TestData.SafaricomListeningDirectory,
                    TestData.SafaricomRequestType,
                    TestData.SafaricomOperatorIdentifier,
                    TestData.SafaricomLineTerminator,
                    TestData.SafaricomFileFormatHandler)
            };
            Context.EstateSecurityUsers.Add(new EstateSecurityUser()
            {
                EstateId = TestData.EstateId,
                SecurityUserId = TestData.UserId,
                EmailAddress = TestData.UserEmailAddress
            });
            await Context.SaveChangesAsync();
            FileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetFileAggregateWithLines()));
            
            Result<FileDetails> fileDetails = await this.Manager.GetFile(TestData.FileId, TestData.EstateId, CancellationToken.None);
            fileDetails.IsSuccess.ShouldBeTrue();
            this.VerifyFile(TestData.GetFileAggregateWithLines(), fileDetails);
            fileDetails.Data.FileProfileName.ShouldBe(TestData.SafaricomProfileName);
        }

        [Fact]
        public async Task FileProcessingManager_GetFile_FileNotFound_ErrorThrown()
        {
            FileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetEmptyFileAggregate()));
            
            Result<FileDetails> result = await this.Manager.GetFile(TestData.FileId, TestData.EstateId, CancellationToken.None);
            
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.NotFound);
        }


        private void VerifyFile(FileAggregate source, Result<FileDetails> fileDetails)
        {
            FileDetails fileModel = source.GetFile();

            fileDetails.Data.FileId.ShouldBe(fileModel.FileId);
            fileDetails.Data.FileImportLogId.ShouldBe(fileModel.FileImportLogId);
            fileDetails.Data.FileLocation.ShouldBe(fileModel.FileLocation);
            fileDetails.Data.FileProfileId.ShouldBe(fileModel.FileProfileId);
            fileDetails.Data.MerchantId.ShouldBe(fileModel.MerchantId);
            fileDetails.Data.ProcessingCompleted.ShouldBe(fileModel.ProcessingCompleted);
            fileDetails.Data.UserId.ShouldBe(fileModel.UserId);
            fileDetails.Data.EstateId.ShouldBe(fileModel.EstateId);

            fileDetails.Data.ProcessingSummary.ShouldNotBeNull();
            fileDetails.Data.ProcessingSummary.FailedLines.ShouldBe(fileModel.ProcessingSummary.FailedLines);
            fileDetails.Data.ProcessingSummary.IgnoredLines.ShouldBe(fileModel.ProcessingSummary.IgnoredLines);
            fileDetails.Data.ProcessingSummary.NotProcessedLines.ShouldBe(fileModel.ProcessingSummary.NotProcessedLines);
            fileDetails.Data.ProcessingSummary.SuccessfullyProcessedLines.ShouldBe(fileModel.ProcessingSummary.SuccessfullyProcessedLines);
            fileDetails.Data.ProcessingSummary.TotalLines.ShouldBe(fileModel.ProcessingSummary.TotalLines);

            foreach (FileLine fileModelFileLine in fileModel.FileLines)
            {
                FileLine? fileLineToVerify = fileDetails.Data.FileLines.SingleOrDefault(f => f.LineNumber == fileModelFileLine.LineNumber);
                fileLineToVerify.ShouldNotBeNull();
                fileLineToVerify.LineData.ShouldBe(fileModelFileLine.LineData);
                fileLineToVerify.TransactionId.ShouldBe(fileModelFileLine.TransactionId);
                fileLineToVerify.ProcessingResult.ShouldBe(fileModelFileLine.ProcessingResult);
            }
        }

        private void VerifyImportLogs(List<TransactionProcessor.Database.Entities.FileImportLog> source,  Result<List<FileProcessor.Models.FileImportLog>> importLogs, Guid? merchantId = null)
        {
            importLogs.Data.ShouldNotBeNull();
            importLogs.Data.ShouldNotBeEmpty();
            importLogs.Data.Count.ShouldBe(TestData.FileImportLogs.Count);
            foreach (TransactionProcessor.Database.Entities.FileImportLog fileImportLog in source)
            {
                FileImportLog importLog = importLogs.Data.SingleOrDefault(i => i.FileImportLogId == fileImportLog.FileImportLogId);
                VerifyImportLog(fileImportLog, importLog, merchantId);
            }
        }

        private void VerifyImportLog(TransactionProcessor.Database.Entities.FileImportLog source, Result<FileProcessor.Models.FileImportLog> importLog, Guid? merchantId = null)
        {
            importLog.Data.ShouldNotBeNull();
            importLog.Data.FileImportLogDateTime.ShouldBe(source.ImportLogDateTime);
            
            List<ImportLogFile> filesToVerify = importLog.Data.Files;
            if (merchantId.HasValue)
            {
                filesToVerify = filesToVerify.Where(f => f.MerchantId == merchantId.Value).ToList();
            }

            foreach (ImportLogFile importLogFile in filesToVerify)
            {
                ImportLogFile file = importLog.Data.Files.SingleOrDefault(impfile => impfile.FileId == importLogFile.FileId);
                file.ShouldNotBeNull();
                file.MerchantId.ShouldBe(importLogFile.MerchantId);
                file.FilePath.ShouldBe(importLogFile.FilePath);
                file.FileProfileId.ShouldBe(importLogFile.FileProfileId);
                file.OriginalFileName.ShouldBe(importLogFile.OriginalFileName);
                file.UserId.ShouldBe(importLogFile.UserId);
            }
        }
        
        

        //private async Task<EstateManagementContext> GetContext(String databaseName)
        //{
        //    EstateManagementContext context = null;
            
        //    DbContextOptionsBuilder<EstateManagementContext> builder = new DbContextOptionsBuilder<EstateManagementContext>()
        //                                                                      .UseInMemoryDatabase(databaseName)
        //                                                                      .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning));
        //    context = new EstateManagementContext(builder.Options);
        
        //    return context;
        //}
    }
}
