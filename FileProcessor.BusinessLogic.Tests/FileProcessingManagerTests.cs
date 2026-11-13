using FileProcessor.Models;
using TransactionProcessing.UnionResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TransactionProcessor.Database.Contexts;
using TransactionProcessor.Database.Entities;
using FileProfileModel = global::FileProcessor.Models.FileProfile;

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
    using Imposter.Abstractions;
    using TransactionProcessing.Shared.DomainDrivenDesign.EventSourcing;
    using TransactionProcessing.Shared.EntityFramework;
    using TransactionProcessing.Shared.EventStore.Aggregate;
    using TransactionProcessing.Shared.Exceptions;
    using Shouldly;
    using System.Threading;
    using Testing;
    using Xunit;
    using FileImportLog = FileProcessor.Models.FileImportLog;
    using FileLine = FileProcessor.Models.FileLine;

    public class FileProcessingManagerTests {
        private IFileProfileManagerImposter FileProfileManager;
        private IAggregateRepositoryImposter<FileAggregate, DomainEvent> FileAggregateRepository;
        private FileProcessorManager Manager;
        private IDbContextResolverImposter<EstateManagementContext> DbContextFactory;
        private EstateManagementContext Context;
        public FileProcessingManagerTests() {
            List<FileProfileModel> fileProfiles = TestData.FileProfiles;
            this.FileProfileManager = new IFileProfileManagerImposter();
            this.FileProfileManager.GetAllFileProfiles(Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(fileProfiles));
            this.FileProfileManager.GetFileProfile(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).Returns((Guid fileProfileId, CancellationToken cancellationToken) =>
            {
                FileProfileModel fileProfile = fileProfiles.SingleOrDefault(f => f.FileProfileId == fileProfileId);
                if (fileProfile == null)
                {
                    return Task.FromResult<Result<FileProfileModel>>(Result.NotFound($"No file profile found for File Profile Id {fileProfileId}"));
                }

                return Task.FromResult<Result<FileProfileModel>>(Result.Success(fileProfile));
            });
            this.DbContextFactory = new IDbContextResolverImposter<EstateManagementContext>();
            this.Context = this.GetContext(Guid.NewGuid().ToString("N"));
            ServiceCollection services = new ServiceCollection();
            services.AddTransient<EstateManagementContext>(_ => this.Context);
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            IServiceScope scope = serviceProvider.CreateScope();
            this.DbContextFactory.Resolve(Arg<String>.Any(), Arg<String>.Any()).Returns(new ResolvedDbContext<EstateManagementContext>(scope));

            var modelFactory= new Common.ModelFactory();
            this.FileAggregateRepository = new IAggregateRepositoryImposter<FileAggregate, DomainEvent>();
            this.Manager = new FileProcessorManager(this.FileProfileManager.Instance(), this.DbContextFactory.Instance(), modelFactory, this.FileAggregateRepository.Instance());
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
            Result<List<FileProfileModel>> allFileProfiles = await this.Manager.GetAllFileProfiles(CancellationToken.None);
            allFileProfiles.IsSuccess.ShouldBeTrue();
            allFileProfiles.GetValueOrThrow().ShouldNotBeEmpty();
        }

        [Fact]
        public async Task FileProcessingManager_GetFileProfile_FileProfileReturned()
        {
            Result<FileProfileModel> fileProfile = await this.Manager.GetFileProfile(TestData.SafaricomFileProfileId, CancellationToken.None);
            fileProfile.IsSuccess.ShouldBeTrue();
            fileProfile.GetValueOrThrow().FileProfileId.ShouldBe(TestData.SafaricomFileProfileId);
        }

        [Fact]
        public async Task FileProcessingManager_GetFileImportLogs_NoMerchantId_ImportLogsReturned()
        {
            this.FileAggregateRepository.GetLatestVersion(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetCreatedFileAggregate()));
            this.Context.FileImportLogs.AddRange(TestData.FileImportLogs);
            this.Context.FileImportLogFiles.AddRange(TestData.FileImportLog1Files);
            this.Context.FileImportLogFiles.AddRange(TestData.FileImportLog2Files);
            this.Context.Files.AddRange(TestData.Files1);
            this.Context.Merchants.Add(new Merchant { MerchantId = TestData.MerchantId, Name = TestData.MerchantName });
            await this.Context.SaveChangesAsync();

            Result<FileImportLogQueryResult> getFileImportLogsResult = await this.Manager.GetFileImportLogs(TestData.EstateId, DateOnly.FromDateTime(TestData.ImportLogStartDate.DateTime), DateOnly.FromDateTime(TestData.ImportLogEndDate.DateTime), null, 1, 50, false, CancellationToken.None);
            getFileImportLogsResult.IsSuccess.ShouldBeTrue();
            List<FileImportLog> importLogs = getFileImportLogsResult.GetValueOrThrow().FileImportLogs;
            this.VerifyImportLogs(TestData.FileImportLogs, Result.Success(importLogs));
            importLogs.SelectMany(log => log.Files).ShouldAllBe(file => file.FileProfileVersionId == TestData.FileProfileVersionId);
        }

        [Fact]
        public async Task FileProcessingManager_GetFileImportLogs_WithMerchantId_ImportLogsReturned()
        {
            this.FileAggregateRepository.GetLatestVersion(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetCreatedFileAggregate()));
            this.Context.FileImportLogs.AddRange(TestData.FileImportLogs);
            this.Context.FileImportLogFiles.AddRange(TestData.FileImportLog1Files);
            this.Context.FileImportLogFiles.AddRange(TestData.FileImportLog2Files);
            this.Context.Files.AddRange(TestData.Files1);
            this.Context.Merchants.Add(new Merchant { MerchantId = TestData.MerchantId, Name = TestData.MerchantName });
            await this.Context.SaveChangesAsync();

            Result<FileImportLogQueryResult> importLogsResult = await this.Manager.GetFileImportLogs(TestData.EstateId, DateOnly.FromDateTime(TestData.ImportLogStartDate.DateTime), DateOnly.FromDateTime(TestData.ImportLogEndDate.DateTime), TestData.MerchantId, 1, 50, false, CancellationToken.None);

            this.VerifyImportLogs(TestData.FileImportLogs, Result.Success(importLogsResult.GetValueOrThrow().FileImportLogs), TestData.MerchantId);
            importLogsResult.GetValueOrThrow().FileImportLogs.SelectMany(log => log.Files).ShouldAllBe(file => file.FileProfileVersionId == TestData.FileProfileVersionId);
        }

        [Fact]
        public async Task FileProcessingManager_GetFileImportLog_NoMerchantId_ImportLogReturned()
        {
            this.FileAggregateRepository.GetLatestVersion(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetCreatedFileAggregate()));
            this.Context.FileImportLogs.AddRange(TestData.FileImportLogs);
            this.Context.FileImportLogFiles.AddRange(TestData.FileImportLog1Files);
            this.Context.FileImportLogFiles.AddRange(TestData.FileImportLog2Files);
            this.Context.Merchants.Add(new Merchant { MerchantId = TestData.MerchantId, Name = TestData.MerchantName });
            this.Context.Files.AddRange(TestData.Files1);
            await this.Context.SaveChangesAsync();

            Result<FileImportLog> importLog = await this.Manager.GetFileImportLog(TestData.FileImportLogId1, TestData.EstateId, null, CancellationToken.None);

            this.VerifyImportLog(TestData.FileImportLogs.First(), importLog);
            importLog.GetValueOrThrow().Files.ShouldAllBe(file => file.FileProfileVersionId == TestData.FileProfileVersionId);
        }

        [Fact]
        public async Task FileProcessingManager_GetFileImportLog_WithMerchantId_ImportLogReturned()
        {
            this.FileAggregateRepository.GetLatestVersion(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetCreatedFileAggregate()));
            this.Context.FileImportLogs.AddRange(TestData.FileImportLogs);
            this.Context.FileImportLogFiles.AddRange(TestData.FileImportLog1Files);
            this.Context.FileImportLogFiles.AddRange(TestData.FileImportLog2Files);
            this.Context.Merchants.Add(new Merchant { MerchantId = TestData.MerchantId, Name = TestData.MerchantName});
            this.Context.Files.AddRange(TestData.Files1);
            await Context.SaveChangesAsync();

            Result<FileImportLog> importLog = await this.Manager.GetFileImportLog(TestData.FileImportLogId1, TestData.EstateId, TestData.MerchantId, CancellationToken.None);

            this.VerifyImportLog(TestData.FileImportLogs.First(),importLog, TestData.MerchantId);
            importLog.GetValueOrThrow().Files.ShouldAllBe(file => file.FileProfileVersionId == TestData.FileProfileVersionId);
        }

        [Fact]
        public async Task FileProcessingManager_GetFileImportLogs_DateOnlyRangeIsInclusiveAndPaged()
        {
            this.Context.FileImportLogs.AddRange(TestData.FileImportLogs);
            this.Context.FileImportLogFiles.AddRange(TestData.FileImportLog1Files);
            this.Context.FileImportLogFiles.AddRange(TestData.FileImportLog2Files);
            this.Context.Merchants.Add(new Merchant { MerchantId = TestData.MerchantId, Name = TestData.MerchantName });
            await this.Context.SaveChangesAsync();

            Result<FileImportLogQueryResult> result = await this.Manager.GetFileImportLogs(
                TestData.EstateId,
                DateOnly.FromDateTime(TestData.ImportLogStartDate.DateTime),
                DateOnly.FromDateTime(TestData.ImportLogEndDate.DateTime),
                null,
                1,
                1,
                false,
                CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            result.GetValueOrThrow().FileImportLogs.ShouldHaveSingleItem();
            result.GetValueOrThrow().FileImportLogs.Single().FileImportLogId.ShouldBe(TestData.FileImportLogId1);
            result.GetValueOrThrow().TotalCount.ShouldBe(2);
            result.GetValueOrThrow().HasMore.ShouldBeTrue();
        }

        [Fact]
        public async Task FileProcessingManager_GetFileImportLog_FilesAreScopedToRequestedImportLog()
        {
            this.FileAggregateRepository.GetLatestVersion(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetCreatedFileAggregate()));
            this.Context.FileImportLogs.AddRange(TestData.FileImportLogs);
            this.Context.FileImportLogFiles.AddRange(TestData.FileImportLog1Files);
            this.Context.FileImportLogFiles.AddRange(TestData.FileImportLog2Files);
            this.Context.Merchants.Add(new Merchant { MerchantId = TestData.MerchantId, Name = TestData.MerchantName });
            this.Context.Files.AddRange(TestData.Files1);
            await this.Context.SaveChangesAsync();

            Result<FileImportLog> result = await this.Manager.GetFileImportLog(TestData.FileImportLogId1, TestData.EstateId, null, CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            result.GetValueOrThrow().Files.ShouldNotBeEmpty();
            result.GetValueOrThrow().Files.ShouldAllBe(file => TestData.FileImportLog1Files.Any(expected => expected.FileId == file.FileId));
            result.GetValueOrThrow().Files.ShouldAllBe(file => file.FileProfileVersionId == TestData.FileProfileVersionId);
        }

        [Fact]
        public async Task FileProcessingManager_GetFileImportLogForFile_ReturnsEntryWithoutFileReadModelRow()
        {
            this.FileAggregateRepository.GetLatestVersion(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetCreatedFileAggregate()));
            this.Context.FileImportLogs.Add(TestData.FileImportLog1);
            this.Context.FileImportLogFiles.Add(TestData.FileImportLog1Files.First());
            await this.Context.SaveChangesAsync();

            Result<FileImportLog> importLog = await this.Manager.GetFileImportLogForFile(TestData.FileId1, TestData.EstateId, CancellationToken.None);

            importLog.IsSuccess.ShouldBeTrue();
            importLog.GetValueOrThrow().FileImportLogId.ShouldBe(TestData.FileImportLogId1);
            importLog.GetValueOrThrow().Files.ShouldHaveSingleItem();
            importLog.GetValueOrThrow().Files.Single().FileId.ShouldBe(TestData.FileId1);
            importLog.GetValueOrThrow().Files.Single().UploadedDateTime.ShouldBe(TestData.FileUploadedDateTime);
            importLog.GetValueOrThrow().Files.Single().FileProfileVersionId.ShouldBe(TestData.FileProfileVersionId);
        }

        [Fact]
        public async Task FileProcessingManager_GetFile_FileReturned()
        {
            List<FileProfileModel> fileProfiles = new List<FileProfileModel>
            {
                TestData.FileProfile
            };
            this.FileAggregateRepository.GetLatestVersion(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetFileAggregateWithLines()));
            
             Result<FileDetails> fileDetails = await this.Manager.GetFile(TestData.FileId, TestData.EstateId, CancellationToken.None);

             fileDetails.IsSuccess.ShouldBeTrue();
             this.VerifyFile(TestData.GetFileAggregateWithLines(), fileDetails);
             fileDetails.GetValueOrThrow().MerchantName.ShouldBeNull();
             fileDetails.GetValueOrThrow().UserEmailAddress.ShouldBeNull();
        }

        [Fact]
        public async Task FileProcessingManager_GetFile_FileAggregateFailed_ErrorReturned()
        {
            FileAggregateRepository.GetLatestVersion(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Failure("Failure"));
            
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
            FileAggregateRepository.GetLatestVersion(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetFileAggregateWithLines()));
            
            Result<FileDetails> fileDetails = await this.Manager.GetFile(TestData.FileId, TestData.EstateId, CancellationToken.None);
            fileDetails.IsSuccess.ShouldBeTrue();

            this.VerifyFile(TestData.GetFileAggregateWithLines(), fileDetails);
            fileDetails.GetValueOrThrow().MerchantName.ShouldBe(TestData.MerchantName);
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
            
            FileAggregateRepository.GetLatestVersion(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetFileAggregateWithLines()));
            
            Result<FileDetails> fileDetails = await this.Manager.GetFile(TestData.FileId, TestData.EstateId, CancellationToken.None);
            fileDetails.IsSuccess.ShouldBeTrue();
            this.VerifyFile(TestData.GetFileAggregateWithLines(), fileDetails);
            fileDetails.GetValueOrThrow().UserEmailAddress.ShouldBe(TestData.UserEmailAddress);
        }

        [Fact]
        public async Task FileProcessingManager_GetFile_FileReturnedWithFileProfileName()
        {
            List<FileProfileModel> fileProfiles = new List<FileProfileModel>
            {
                new FileProfileModel(TestData.FileProfileId,                 
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
            FileAggregateRepository.GetLatestVersion(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetFileAggregateWithLines()));
            
            Result<FileDetails> fileDetails = await this.Manager.GetFile(TestData.FileId, TestData.EstateId, CancellationToken.None);
            fileDetails.IsSuccess.ShouldBeTrue();
            this.VerifyFile(TestData.GetFileAggregateWithLines(), fileDetails);
            fileDetails.GetValueOrThrow().FileProfileName.ShouldBe(TestData.SafaricomProfileName);
        }

        [Fact]
        public async Task FileProcessingManager_GetFile_FileNotFound_ErrorThrown()
        {
            FileAggregateRepository.GetLatestVersion(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetEmptyFileAggregate()));
            
            Result<FileDetails> result = await this.Manager.GetFile(TestData.FileId, TestData.EstateId, CancellationToken.None);
            
            result.IsFailed.ShouldBeTrue();
            result.Errors[0].ErrorType.ShouldBe(ErrorType.NotFound);
        }


        private void VerifyFile(FileAggregate source, Result<FileDetails> fileDetails)
        {
            FileDetails fileModel = source.GetFile();

            fileDetails.GetValueOrThrow().FileId.ShouldBe(fileModel.FileId);
            fileDetails.GetValueOrThrow().FileImportLogId.ShouldBe(fileModel.FileImportLogId);
            fileDetails.GetValueOrThrow().FileLocation.ShouldBe(fileModel.FileLocation);
            fileDetails.GetValueOrThrow().FileProfileId.ShouldBe(fileModel.FileProfileId);
            fileDetails.GetValueOrThrow().MerchantId.ShouldBe(fileModel.MerchantId);
            fileDetails.GetValueOrThrow().ProcessingCompleted.ShouldBe(fileModel.ProcessingCompleted);
            fileDetails.GetValueOrThrow().UserId.ShouldBe(fileModel.UserId);
            fileDetails.GetValueOrThrow().EstateId.ShouldBe(fileModel.EstateId);

            fileDetails.GetValueOrThrow().ProcessingSummary.ShouldNotBeNull();
            fileDetails.GetValueOrThrow().ProcessingSummary.FailedLines.ShouldBe(fileModel.ProcessingSummary.FailedLines);
            fileDetails.GetValueOrThrow().ProcessingSummary.IgnoredLines.ShouldBe(fileModel.ProcessingSummary.IgnoredLines);
            fileDetails.GetValueOrThrow().ProcessingSummary.NotProcessedLines.ShouldBe(fileModel.ProcessingSummary.NotProcessedLines);
            fileDetails.GetValueOrThrow().ProcessingSummary.SuccessfullyProcessedLines.ShouldBe(fileModel.ProcessingSummary.SuccessfullyProcessedLines);
            fileDetails.GetValueOrThrow().ProcessingSummary.TotalLines.ShouldBe(fileModel.ProcessingSummary.TotalLines);

            foreach (FileLine fileModelFileLine in fileModel.FileLines)
            {
                FileLine? fileLineToVerify = fileDetails.GetValueOrThrow().FileLines.SingleOrDefault(f => f.LineNumber == fileModelFileLine.LineNumber);
                fileLineToVerify.ShouldNotBeNull();
                fileLineToVerify.LineData.ShouldBe(fileModelFileLine.LineData);
                fileLineToVerify.TransactionId.ShouldBe(fileModelFileLine.TransactionId);
                fileLineToVerify.ProcessingResult.ShouldBe(fileModelFileLine.ProcessingResult);
            }
        }

        private void VerifyImportLogs(List<TransactionProcessor.Database.Entities.FileImportLog> source,  Result<List<FileProcessor.Models.FileImportLog>> importLogs, Guid? merchantId = null)
        {
            importLogs.GetValueOrThrow().ShouldNotBeNull();
            importLogs.GetValueOrThrow().ShouldNotBeEmpty();
            importLogs.GetValueOrThrow().Count.ShouldBe(TestData.FileImportLogs.Count);
            foreach (TransactionProcessor.Database.Entities.FileImportLog fileImportLog in source)
            {
                FileImportLog importLog = importLogs.GetValueOrThrow().SingleOrDefault(i => i.FileImportLogId == fileImportLog.FileImportLogId);
                VerifyImportLog(fileImportLog, Result.Success(importLog), merchantId);
            }
        }

        private void VerifyImportLog(TransactionProcessor.Database.Entities.FileImportLog source, Result<FileProcessor.Models.FileImportLog> importLog, Guid? merchantId = null)
        {
            importLog.GetValueOrThrow().ShouldNotBeNull();
            importLog.GetValueOrThrow().FileImportLogDateTime.ShouldBe(source.ImportLogDateTime);
            
            List<ImportLogFile> filesToVerify = importLog.GetValueOrThrow().Files;
            if (merchantId.HasValue)
            {
                filesToVerify = filesToVerify.Where(f => f.MerchantId == merchantId.Value).ToList();
            }

            foreach (ImportLogFile importLogFile in filesToVerify)
            {
                ImportLogFile file = importLog.GetValueOrThrow().Files.SingleOrDefault(impfile => impfile.FileId == importLogFile.FileId);
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
