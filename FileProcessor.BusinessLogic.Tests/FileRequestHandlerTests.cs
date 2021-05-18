using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileProcessor.BusinessLogic.Tests
{
    using System.IO;
    using System.IO.Abstractions;
    using System.IO.Abstractions.TestingHelpers;
    using System.Threading;
    using EstateManagement.Client;
    using FileAggregate;
    using FileFormatHandlers;
    using FileImportLogAggregate;
    using Managers;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using RequestHandlers;
    using Requests;
    using SecurityService.Client;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EventStore.Aggregate;
    using Shared.Exceptions;
    using Shared.General;
    using Shared.Logger;
    using Shouldly;
    using Testing;
    using TransactionProcessor.Client;
    using TransactionProcessor.DataTransferObjects;
    using Xunit;

    public class FileRequestHandlerTests
    {
        [Fact]
        public async Task FileRequestHandler_UploadFileRequest_RequestIsHandled()
        {
            Mock<IFileProcessorManager> fileProcessorManager = new Mock<IFileProcessorManager>();
            fileProcessorManager.Setup(f => f.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.FileProfileSafaricom);

            Mock<IAggregateRepository<FileImportLogAggregate, DomainEventRecord.DomainEvent>> fileImportLogAggregateRepository =
                new Mock<IAggregateRepository<FileImportLogAggregate, DomainEventRecord.DomainEvent>>();
            fileImportLogAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetEmptyFileImportLogAggregate);
            Mock<IAggregateRepository<FileAggregate, DomainEventRecord.DomainEvent>> fileAggregateRepository =
                new Mock<IAggregateRepository<FileAggregate, DomainEventRecord.DomainEvent>>();
            Mock<ITransactionProcessorClient> transactionProcessorClient = new Mock<ITransactionProcessorClient>();
            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();
            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();
            Mock<IFileFormatHandler> fileFormatHandler = new Mock<IFileFormatHandler>();
            Func<String, IFileFormatHandler> fileFormatHandlerResolver = (format) =>
                                                                         {
                                                                             return fileFormatHandler.Object;
                                                                         };

            MockFileSystem fileSystem = new MockFileSystem();
            fileSystem.AddFile(TestData.FilePathWithName, new MockFileData("D,1,1,1"));
            fileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom");

            FileRequestHandler fileRequestHandler = new FileRequestHandler(fileProcessorManager.Object,
                                                                           fileImportLogAggregateRepository.Object,
                                                                           fileAggregateRepository.Object,
                                                                           transactionProcessorClient.Object,
                                                                           estateClient.Object,
                                                                           securityServiceClient.Object,
                                                                           fileFormatHandlerResolver,
                                                                           fileSystem);

            UploadFileRequest uploadFileRequest =
                new UploadFileRequest(TestData.EstateId, TestData.MerchantId, TestData.UserId, TestData.FilePathWithName, TestData.FileProfileId, TestData.FileUploadedDateTime);

            Should.NotThrow(async () =>
                            {
                                await fileRequestHandler.Handle(uploadFileRequest, CancellationToken.None);
                            });
        }

        [Fact]
        public async Task FileRequestHandler_UploadFileRequest_ImportLogAlreadyCreated_RequestIsHandled()
        {
            Mock<IFileProcessorManager> fileProcessorManager = new Mock<IFileProcessorManager>();
            fileProcessorManager.Setup(f => f.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.FileProfileSafaricom);

            Mock<IAggregateRepository<FileImportLogAggregate, DomainEventRecord.DomainEvent>> fileImportLogAggregateRepository =
                new Mock<IAggregateRepository<FileImportLogAggregate, DomainEventRecord.DomainEvent>>();
            fileImportLogAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetCreatedFileImportLogAggregate);
            Mock<IAggregateRepository<FileAggregate, DomainEventRecord.DomainEvent>> fileAggregateRepository =
                new Mock<IAggregateRepository<FileAggregate, DomainEventRecord.DomainEvent>>();
            Mock<ITransactionProcessorClient> transactionProcessorClient = new Mock<ITransactionProcessorClient>();
            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();
            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();
            Mock<IFileFormatHandler> fileFormatHandler = new Mock<IFileFormatHandler>();
            Func<String, IFileFormatHandler> fileFormatHandlerResolver = (format) =>
            {
                return fileFormatHandler.Object;
            };

            MockFileSystem fileSystem = new MockFileSystem();
            fileSystem.AddFile(TestData.FilePathWithName, new MockFileData("D,1,1,1"));
            fileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom");

            FileRequestHandler fileRequestHandler = new FileRequestHandler(fileProcessorManager.Object,
                                                                           fileImportLogAggregateRepository.Object,
                                                                           fileAggregateRepository.Object,
                                                                           transactionProcessorClient.Object,
                                                                           estateClient.Object,
                                                                           securityServiceClient.Object,
                                                                           fileFormatHandlerResolver,
                                                                           fileSystem);

            UploadFileRequest uploadFileRequest =
                new UploadFileRequest(TestData.EstateId, TestData.MerchantId, TestData.UserId, TestData.FilePathWithName, TestData.FileProfileId, TestData.FileUploadedDateTime);

            Should.NotThrow(async () =>
            {
                await fileRequestHandler.Handle(uploadFileRequest, CancellationToken.None);
            });
        }

        [Fact]
        public async Task FileRequestHandler_UploadFileRequest_NoFileProfiles_RequestIsHandled()
        {
            Mock<IFileProcessorManager> fileProcessorManager = new Mock<IFileProcessorManager>();
            fileProcessorManager.Setup(f => f.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.FileProfileNull);

            Mock<IAggregateRepository<FileImportLogAggregate, DomainEventRecord.DomainEvent>> fileImportLogAggregateRepository =
                new Mock<IAggregateRepository<FileImportLogAggregate, DomainEventRecord.DomainEvent>>();
            fileImportLogAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetCreatedFileImportLogAggregate);

            Mock<IAggregateRepository<FileAggregate, DomainEventRecord.DomainEvent>> fileAggregateRepository =
                new Mock<IAggregateRepository<FileAggregate, DomainEventRecord.DomainEvent>>();
            Mock<ITransactionProcessorClient> transactionProcessorClient = new Mock<ITransactionProcessorClient>();
            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();
            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();
            Mock<IFileFormatHandler> fileFormatHandler = new Mock<IFileFormatHandler>();
            Func<String, IFileFormatHandler> fileFormatHandlerResolver = (format) =>
            {
                return fileFormatHandler.Object;
            };

            MockFileSystem fileSystem = new MockFileSystem();

            FileRequestHandler fileRequestHandler = new FileRequestHandler(fileProcessorManager.Object,
                                                                           fileImportLogAggregateRepository.Object,
                                                                           fileAggregateRepository.Object,
                                                                           transactionProcessorClient.Object,
                                                                           estateClient.Object,
                                                                           securityServiceClient.Object,
                                                                           fileFormatHandlerResolver,
                                                                           fileSystem);

            UploadFileRequest uploadFileRequest =
                new UploadFileRequest(TestData.EstateId, TestData.MerchantId, TestData.UserId, TestData.FilePathWithName, TestData.FileProfileId, TestData.FileUploadedDateTime);

            Should.Throw<NotFoundException>(async () =>
            {
                await fileRequestHandler.Handle(uploadFileRequest, CancellationToken.None);
            });
        }

        [Fact]
        public async Task FileRequestHandler_UploadFileRequest_FileNotFound_RequestIsHandled()
        {
            Mock<IFileProcessorManager> fileProcessorManager = new Mock<IFileProcessorManager>();
            fileProcessorManager.Setup(f => f.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.FileProfileSafaricom);
            
            Mock<IAggregateRepository<FileImportLogAggregate, DomainEventRecord.DomainEvent>> fileImportLogAggregateRepository =
                new Mock<IAggregateRepository<FileImportLogAggregate, DomainEventRecord.DomainEvent>>();
            fileImportLogAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetCreatedFileImportLogAggregate);

            Mock<IAggregateRepository<FileAggregate, DomainEventRecord.DomainEvent>> fileAggregateRepository =
                new Mock<IAggregateRepository<FileAggregate, DomainEventRecord.DomainEvent>>();
            Mock<ITransactionProcessorClient> transactionProcessorClient = new Mock<ITransactionProcessorClient>();
            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();
            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();
            Mock<IFileFormatHandler> fileFormatHandler = new Mock<IFileFormatHandler>();
            Func<String, IFileFormatHandler> fileFormatHandlerResolver = (format) =>
            {
                return fileFormatHandler.Object;
            };

            MockFileSystem fileSystem = new MockFileSystem();

            FileRequestHandler fileRequestHandler = new FileRequestHandler(fileProcessorManager.Object,
                                                                           fileImportLogAggregateRepository.Object,
                                                                           fileAggregateRepository.Object,
                                                                           transactionProcessorClient.Object,
                                                                           estateClient.Object,
                                                                           securityServiceClient.Object,
                                                                           fileFormatHandlerResolver,
                                                                           fileSystem);

            UploadFileRequest uploadFileRequest =
                new UploadFileRequest(TestData.EstateId, TestData.MerchantId, TestData.UserId, TestData.FilePathWithName, TestData.FileProfileId, TestData.FileUploadedDateTime);

            Should.Throw<FileNotFoundException>(async () =>
                                                {
                                                    await fileRequestHandler.Handle(uploadFileRequest, CancellationToken.None);
                                                });
        }

        [Fact]
        public async Task FileRequestHandler_UploadFileRequest_DestinationDirectoryNotFound_RequestIsHandled()
        {
            Mock<IFileProcessorManager> fileProcessorManager = new Mock<IFileProcessorManager>();
            fileProcessorManager.Setup(f => f.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.FileProfileSafaricom);

            Mock<IAggregateRepository<FileImportLogAggregate, DomainEventRecord.DomainEvent>> fileImportLogAggregateRepository =
                new Mock<IAggregateRepository<FileImportLogAggregate, DomainEventRecord.DomainEvent>>();
            fileImportLogAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetCreatedFileImportLogAggregate);

            Mock<IAggregateRepository<FileAggregate, DomainEventRecord.DomainEvent>> fileAggregateRepository =
                new Mock<IAggregateRepository<FileAggregate, DomainEventRecord.DomainEvent>>();
            Mock<ITransactionProcessorClient> transactionProcessorClient = new Mock<ITransactionProcessorClient>();
            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();
            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();
            Mock<IFileFormatHandler> fileFormatHandler = new Mock<IFileFormatHandler>();
            Func<String, IFileFormatHandler> fileFormatHandlerResolver = (format) =>
            {
                return fileFormatHandler.Object;
            };

            MockFileSystem fileSystem = new MockFileSystem();
            fileSystem.AddFile(TestData.FilePathWithName, new MockFileData("D,1,1,1"));

            FileRequestHandler fileRequestHandler = new FileRequestHandler(fileProcessorManager.Object,
                                                                           fileImportLogAggregateRepository.Object,
                                                                           fileAggregateRepository.Object,
                                                                           transactionProcessorClient.Object,
                                                                           estateClient.Object,
                                                                           securityServiceClient.Object,
                                                                           fileFormatHandlerResolver,
                                                                           fileSystem);

            UploadFileRequest uploadFileRequest =
                new UploadFileRequest(TestData.EstateId, TestData.MerchantId, TestData.UserId, TestData.FilePathWithName, TestData.FileProfileId, TestData.FileUploadedDateTime);

            Should.Throw<DirectoryNotFoundException>(async () =>
            {
                await fileRequestHandler.Handle(uploadFileRequest, CancellationToken.None);
            });
        }

        [Fact]
        public void FileRequestHandler_ProcessUploadedFileRequest_RequestIsHandled()
        {
            Mock<IFileProcessorManager> fileProcessorManager = new Mock<IFileProcessorManager>();
            Mock<IAggregateRepository<FileImportLogAggregate, DomainEventRecord.DomainEvent>> fileImportLogAggregateRepository =
                new Mock<IAggregateRepository<FileImportLogAggregate, DomainEventRecord.DomainEvent>>();
            Mock<IAggregateRepository<FileAggregate, DomainEventRecord.DomainEvent>> fileAggregateRepository =
                new Mock<IAggregateRepository<FileAggregate, DomainEventRecord.DomainEvent>>();

            fileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetEmptyFileAggregate);
            Mock<ITransactionProcessorClient> transactionProcessorClient = new Mock<ITransactionProcessorClient>();
            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();
            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();
            Mock<IFileFormatHandler> fileFormatHandler = new Mock<IFileFormatHandler>();
            Func<String, IFileFormatHandler> fileFormatHandlerResolver = (format) =>
            {
                return fileFormatHandler.Object;
            };

            MockFileSystem fileSystem = new MockFileSystem();
            FileRequestHandler fileRequestHandler = new FileRequestHandler(fileProcessorManager.Object,
                                                                           fileImportLogAggregateRepository.Object,
                                                                           fileAggregateRepository.Object,
                                                                           transactionProcessorClient.Object,
                                                                           estateClient.Object,
                                                                           securityServiceClient.Object,
                                                                           fileFormatHandlerResolver,
                                                                           fileSystem);
            ProcessUploadedFileRequest processUploadedFileRequest =
                new ProcessUploadedFileRequest(TestData.EstateId, TestData.MerchantId, TestData.FileImportLogId, TestData.FileId, TestData.UserId, TestData.FilePath, TestData.FileProfileId, TestData.FileUploadedDateTime);

            Should.NotThrow(async () =>
            {
                await fileRequestHandler.Handle(processUploadedFileRequest, CancellationToken.None);
            });
        }


        [Fact]
        public void FileRequestHandler_SafaricomTopupRequest_RequestIsHandled()
        {
            Mock<IFileProcessorManager> fileProcessorManager = new Mock<IFileProcessorManager>();
            fileProcessorManager.Setup(f => f.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.FileProfileSafaricom);
            Mock<IAggregateRepository<FileImportLogAggregate, DomainEventRecord.DomainEvent>> fileImportLogAggregateRepository =
                new Mock<IAggregateRepository<FileImportLogAggregate, DomainEventRecord.DomainEvent>>();
            Mock<IAggregateRepository<FileAggregate, DomainEventRecord.DomainEvent>> fileAggregateRepository =
                new Mock<IAggregateRepository<FileAggregate, DomainEventRecord.DomainEvent>>();
            
            fileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetCreatedFileAggregate);
            Mock<ITransactionProcessorClient> transactionProcessorClient = new Mock<ITransactionProcessorClient>();
            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();
            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();
            Mock<IFileFormatHandler> fileFormatHandler = new Mock<IFileFormatHandler>();
            Func<String, IFileFormatHandler> fileFormatHandlerResolver = (format) =>
            {
                return fileFormatHandler.Object;
            };

            MockFileSystem fileSystem = new MockFileSystem();
            fileSystem.AddFile(TestData.FilePathWithName, new MockFileData("D,1,1,1"));

            fileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom/inprogress");
            fileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom/processed");
            fileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom/failed");

            FileRequestHandler fileRequestHandler = new FileRequestHandler(fileProcessorManager.Object,
                                                                           fileImportLogAggregateRepository.Object,
                                                                           fileAggregateRepository.Object,
                                                                           transactionProcessorClient.Object,
                                                                           estateClient.Object,
                                                                           securityServiceClient.Object,
                                                                           fileFormatHandlerResolver,
                                                                           fileSystem);
            SafaricomTopupRequest safaricomTopupRequest =
                new SafaricomTopupRequest(TestData.FileId, TestData.FilePathWithName, TestData.FileProfileId);

            Should.NotThrow(async () =>
            {
                await fileRequestHandler.Handle(safaricomTopupRequest, CancellationToken.None);
            });
        }

        [Fact]
        public void FileRequestHandler_SafaricomTopupRequest_FileAggregateNotCreated_RequestIsHandled()
        {
            Mock<IFileProcessorManager> fileProcessorManager = new Mock<IFileProcessorManager>();
            fileProcessorManager.Setup(f => f.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.FileProfileSafaricom);
            Mock<IAggregateRepository<FileImportLogAggregate, DomainEventRecord.DomainEvent>> fileImportLogAggregateRepository =
                new Mock<IAggregateRepository<FileImportLogAggregate, DomainEventRecord.DomainEvent>>();
            Mock<IAggregateRepository<FileAggregate, DomainEventRecord.DomainEvent>> fileAggregateRepository =
                new Mock<IAggregateRepository<FileAggregate, DomainEventRecord.DomainEvent>>();

            fileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetEmptyFileAggregate);
            Mock<ITransactionProcessorClient> transactionProcessorClient = new Mock<ITransactionProcessorClient>();
            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();
            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();
            Mock<IFileFormatHandler> fileFormatHandler = new Mock<IFileFormatHandler>();
            Func<String, IFileFormatHandler> fileFormatHandlerResolver = (format) =>
            {
                return fileFormatHandler.Object;
            };

            MockFileSystem fileSystem = new MockFileSystem();
            fileSystem.AddFile(TestData.FilePathWithName, new MockFileData("D,1,1,1"));

            fileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom/inprogress");
            fileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom/processed");
            fileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom/failed");

            FileRequestHandler fileRequestHandler = new FileRequestHandler(fileProcessorManager.Object,
                                                                           fileImportLogAggregateRepository.Object,
                                                                           fileAggregateRepository.Object,
                                                                           transactionProcessorClient.Object,
                                                                           estateClient.Object,
                                                                           securityServiceClient.Object,
                                                                           fileFormatHandlerResolver,
                                                                           fileSystem);
            SafaricomTopupRequest safaricomTopupRequest =
                new SafaricomTopupRequest(TestData.FileId, TestData.FilePathWithName, TestData.FileProfileId);

            Should.NotThrow(async () =>
            {
                await fileRequestHandler.Handle(safaricomTopupRequest, CancellationToken.None);
            });
        }

        [Fact]
        public void FileRequestHandler_SafaricomTopupRequest_FileNotFound_RequestIsHandled()
        {
            Mock<IFileProcessorManager> fileProcessorManager = new Mock<IFileProcessorManager>();
            fileProcessorManager.Setup(f => f.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.FileProfileSafaricom);
            Mock<IAggregateRepository<FileImportLogAggregate, DomainEventRecord.DomainEvent>> fileImportLogAggregateRepository =
                new Mock<IAggregateRepository<FileImportLogAggregate, DomainEventRecord.DomainEvent>>();
            Mock<IAggregateRepository<FileAggregate, DomainEventRecord.DomainEvent>> fileAggregateRepository =
                new Mock<IAggregateRepository<FileAggregate, DomainEventRecord.DomainEvent>>();

            fileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetCreatedFileAggregate);
            Mock<ITransactionProcessorClient> transactionProcessorClient = new Mock<ITransactionProcessorClient>();
            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();
            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();
            Mock<IFileFormatHandler> fileFormatHandler = new Mock<IFileFormatHandler>();
            Func<String, IFileFormatHandler> fileFormatHandlerResolver = (format) =>
            {
                return fileFormatHandler.Object;
            };

            MockFileSystem fileSystem = new MockFileSystem();

            FileRequestHandler fileRequestHandler = new FileRequestHandler(fileProcessorManager.Object,
                                                                           fileImportLogAggregateRepository.Object,
                                                                           fileAggregateRepository.Object,
                                                                           transactionProcessorClient.Object,
                                                                           estateClient.Object,
                                                                           securityServiceClient.Object,
                                                                           fileFormatHandlerResolver,
                                                                           fileSystem);
            SafaricomTopupRequest safaricomTopupRequest =
                new SafaricomTopupRequest(TestData.FileId, TestData.FilePathWithName, TestData.FileProfileId);

            Should.Throw<FileNotFoundException>(async () =>
                            {
                                await fileRequestHandler.Handle(safaricomTopupRequest, CancellationToken.None);
                            });
        }

        [Fact]
        public async Task FileRequestHandler_SafaricomTopupRequest_NoFileProfiles_RequestIsHandled()
        {
            Mock<IFileProcessorManager> fileProcessorManager = new Mock<IFileProcessorManager>();
            fileProcessorManager.Setup(f => f.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.FileProfileNull);
            Mock<IAggregateRepository<FileImportLogAggregate, DomainEventRecord.DomainEvent>> fileImportLogAggregateRepository =
                new Mock<IAggregateRepository<FileImportLogAggregate, DomainEventRecord.DomainEvent>>();
            fileImportLogAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                                            .ReturnsAsync(TestData.GetEmptyFileImportLogAggregate);
            Mock<IAggregateRepository<FileAggregate, DomainEventRecord.DomainEvent>> fileAggregateRepository =
                new Mock<IAggregateRepository<FileAggregate, DomainEventRecord.DomainEvent>>();
            fileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetCreatedFileAggregate);
            Mock<ITransactionProcessorClient> transactionProcessorClient = new Mock<ITransactionProcessorClient>();
            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();
            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();
            Mock<IFileFormatHandler> fileFormatHandler = new Mock<IFileFormatHandler>();
            Func<String, IFileFormatHandler> fileFormatHandlerResolver = (format) =>
            {
                return fileFormatHandler.Object;
            };

            MockFileSystem fileSystem = new MockFileSystem();
            fileSystem.AddFile(TestData.FilePathWithName, new MockFileData("D,1,1,1"));

            FileRequestHandler fileRequestHandler = new FileRequestHandler(fileProcessorManager.Object,
                                                                           fileImportLogAggregateRepository.Object,
                                                                           fileAggregateRepository.Object,
                                                                           transactionProcessorClient.Object,
                                                                           estateClient.Object,
                                                                           securityServiceClient.Object,
                                                                           fileFormatHandlerResolver,
                                                                           fileSystem);

            SafaricomTopupRequest safaricomTopupRequest =
                new SafaricomTopupRequest(TestData.FileId, TestData.FilePathWithName, TestData.FileProfileId);

            Should.Throw<NotFoundException>(async () =>
            {
                await fileRequestHandler.Handle(safaricomTopupRequest, CancellationToken.None);
            });
        }

        [Fact]
        public async Task FileRequestHandler_SafaricomTopupRequest_InProgressDirectoryNotFound_RequestIsHandled()
        {
            Mock<IFileProcessorManager> fileProcessorManager = new Mock<IFileProcessorManager>();
            fileProcessorManager.Setup(f => f.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.FileProfileSafaricom);
            Mock<IAggregateRepository<FileImportLogAggregate, DomainEventRecord.DomainEvent>> fileImportLogAggregateRepository =
                new Mock<IAggregateRepository<FileImportLogAggregate, DomainEventRecord.DomainEvent>>();
            Mock<IAggregateRepository<FileAggregate, DomainEventRecord.DomainEvent>> fileAggregateRepository =
                new Mock<IAggregateRepository<FileAggregate, DomainEventRecord.DomainEvent>>();

            fileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetCreatedFileAggregate);
            Mock<ITransactionProcessorClient> transactionProcessorClient = new Mock<ITransactionProcessorClient>();
            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();
            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();
            Mock<IFileFormatHandler> fileFormatHandler = new Mock<IFileFormatHandler>();
            Func<String, IFileFormatHandler> fileFormatHandlerResolver = (format) =>
            {
                return fileFormatHandler.Object;
            };

            MockFileSystem fileSystem = new MockFileSystem();
            fileSystem.AddFile(TestData.FilePathWithName, new MockFileData("D,1,1,1"));

            fileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom/processed");
            fileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom/failed");

            FileRequestHandler fileRequestHandler = new FileRequestHandler(fileProcessorManager.Object,
                                                                           fileImportLogAggregateRepository.Object,
                                                                           fileAggregateRepository.Object,
                                                                           transactionProcessorClient.Object,
                                                                           estateClient.Object,
                                                                           securityServiceClient.Object,
                                                                           fileFormatHandlerResolver,
                                                                           fileSystem);
            SafaricomTopupRequest safaricomTopupRequest =
                new SafaricomTopupRequest(TestData.FileId, TestData.FilePathWithName, TestData.FileProfileId);

            Should.Throw<DirectoryNotFoundException>(async () =>
            {
                await fileRequestHandler.Handle(safaricomTopupRequest, CancellationToken.None);
            });
        }

        [Fact]
        public async Task FileRequestHandler_SafaricomTopupRequest_ProcessedDirectoryNotFound_RequestIsHandled()
        {
            Mock<IFileProcessorManager> fileProcessorManager = new Mock<IFileProcessorManager>();
            fileProcessorManager.Setup(f => f.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.FileProfileSafaricom);
            Mock<IAggregateRepository<FileImportLogAggregate, DomainEventRecord.DomainEvent>> fileImportLogAggregateRepository =
                new Mock<IAggregateRepository<FileImportLogAggregate, DomainEventRecord.DomainEvent>>();
            Mock<IAggregateRepository<FileAggregate, DomainEventRecord.DomainEvent>> fileAggregateRepository =
                new Mock<IAggregateRepository<FileAggregate, DomainEventRecord.DomainEvent>>();

            fileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetCreatedFileAggregate);
            Mock<ITransactionProcessorClient> transactionProcessorClient = new Mock<ITransactionProcessorClient>();
            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();
            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();
            Mock<IFileFormatHandler> fileFormatHandler = new Mock<IFileFormatHandler>();
            Func<String, IFileFormatHandler> fileFormatHandlerResolver = (format) =>
            {
                return fileFormatHandler.Object;
            };

            MockFileSystem fileSystem = new MockFileSystem();
            fileSystem.AddFile(TestData.FilePathWithName, new MockFileData("D,1,1,1"));

            fileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom/inprogress");
            fileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom/failed");

            FileRequestHandler fileRequestHandler = new FileRequestHandler(fileProcessorManager.Object,
                                                                           fileImportLogAggregateRepository.Object,
                                                                           fileAggregateRepository.Object,
                                                                           transactionProcessorClient.Object,
                                                                           estateClient.Object,
                                                                           securityServiceClient.Object,
                                                                           fileFormatHandlerResolver,
                                                                           fileSystem);
            SafaricomTopupRequest safaricomTopupRequest =
                new SafaricomTopupRequest(TestData.FileId, TestData.FilePathWithName, TestData.FileProfileId);

            Should.Throw<DirectoryNotFoundException>(async () =>
            {
                await fileRequestHandler.Handle(safaricomTopupRequest, CancellationToken.None);
            });
        }

        [Fact]
        public async Task FileRequestHandler_SafaricomTopupRequest_FailedDirectoryNotFound_RequestIsHandled()
        {
            Mock<IFileProcessorManager> fileProcessorManager = new Mock<IFileProcessorManager>();
            fileProcessorManager.Setup(f => f.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.FileProfileSafaricom);
            Mock<IAggregateRepository<FileImportLogAggregate, DomainEventRecord.DomainEvent>> fileImportLogAggregateRepository =
                new Mock<IAggregateRepository<FileImportLogAggregate, DomainEventRecord.DomainEvent>>();
            Mock<IAggregateRepository<FileAggregate, DomainEventRecord.DomainEvent>> fileAggregateRepository =
                new Mock<IAggregateRepository<FileAggregate, DomainEventRecord.DomainEvent>>();

            fileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetCreatedFileAggregate);
            Mock<ITransactionProcessorClient> transactionProcessorClient = new Mock<ITransactionProcessorClient>();
            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();
            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();
            Mock<IFileFormatHandler> fileFormatHandler = new Mock<IFileFormatHandler>();
            Func<String, IFileFormatHandler> fileFormatHandlerResolver = (format) =>
            {
                return fileFormatHandler.Object;
            };

            MockFileSystem fileSystem = new MockFileSystem();
            fileSystem.AddFile(TestData.FilePathWithName, new MockFileData("D,1,1,1"));

            fileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom/inprogress");
            fileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom/processed");

            FileRequestHandler fileRequestHandler = new FileRequestHandler(fileProcessorManager.Object,
                                                                           fileImportLogAggregateRepository.Object,
                                                                           fileAggregateRepository.Object,
                                                                           transactionProcessorClient.Object,
                                                                           estateClient.Object,
                                                                           securityServiceClient.Object,
                                                                           fileFormatHandlerResolver,
                                                                           fileSystem);
            SafaricomTopupRequest safaricomTopupRequest =
                new SafaricomTopupRequest(TestData.FileId, TestData.FilePathWithName, TestData.FileProfileId);

            Should.Throw<DirectoryNotFoundException>(async () =>
            {
                await fileRequestHandler.Handle(safaricomTopupRequest, CancellationToken.None);
            });
        }

        [Fact]
        public void FileRequestHandler_SafaricomTopupRequest_FileIsEmpty_RequestIsHandled()
        {
            Mock<IFileProcessorManager> fileProcessorManager = new Mock<IFileProcessorManager>();
            fileProcessorManager.Setup(f => f.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.FileProfileSafaricom);
            Mock<IAggregateRepository<FileImportLogAggregate, DomainEventRecord.DomainEvent>> fileImportLogAggregateRepository =
                new Mock<IAggregateRepository<FileImportLogAggregate, DomainEventRecord.DomainEvent>>();
            Mock<IAggregateRepository<FileAggregate, DomainEventRecord.DomainEvent>> fileAggregateRepository =
                new Mock<IAggregateRepository<FileAggregate, DomainEventRecord.DomainEvent>>();

            fileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetCreatedFileAggregate);
            Mock<ITransactionProcessorClient> transactionProcessorClient = new Mock<ITransactionProcessorClient>();
            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();
            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();
            Mock<IFileFormatHandler> fileFormatHandler = new Mock<IFileFormatHandler>();
            Func<String, IFileFormatHandler> fileFormatHandlerResolver = (format) =>
            {
                return fileFormatHandler.Object;
            };

            MockFileSystem fileSystem = new MockFileSystem();
            fileSystem.AddFile(TestData.FilePathWithName, new MockFileData(String.Empty));

            fileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom/inprogress");
            fileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom/processed");
            fileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom/failed");

            FileRequestHandler fileRequestHandler = new FileRequestHandler(fileProcessorManager.Object,
                                                                           fileImportLogAggregateRepository.Object,
                                                                           fileAggregateRepository.Object,
                                                                           transactionProcessorClient.Object,
                                                                           estateClient.Object,
                                                                           securityServiceClient.Object,
                                                                           fileFormatHandlerResolver,
                                                                           fileSystem);
            SafaricomTopupRequest safaricomTopupRequest =
                new SafaricomTopupRequest(TestData.FileId, TestData.FilePathWithName, TestData.FileProfileId);

            Should.NotThrow(async () =>
            {
                await fileRequestHandler.Handle(safaricomTopupRequest, CancellationToken.None);
            });

            fileAggregateRepository.Verify(f => f.SaveChanges(It.IsAny<FileAggregate>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public void FileRequestHandler_ProcessTransactionForFileLineRequest_RequestIsHandled()
        {
            Mock<IFileProcessorManager> fileProcessorManager = new Mock<IFileProcessorManager>();
            fileProcessorManager.Setup(f => f.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.FileProfileSafaricom);
            Mock<IAggregateRepository<FileImportLogAggregate, DomainEventRecord.DomainEvent>> fileImportLogAggregateRepository =
                new Mock<IAggregateRepository<FileImportLogAggregate, DomainEventRecord.DomainEvent>>();

            Mock<IAggregateRepository<FileAggregate, DomainEventRecord.DomainEvent>> fileAggregateRepository =
                new Mock<IAggregateRepository<FileAggregate, DomainEventRecord.DomainEvent>>();
            fileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetFileAggregateWithLines);
            
            Mock<ITransactionProcessorClient> transactionProcessorClient = new Mock<ITransactionProcessorClient>();
            transactionProcessorClient.Setup(t => t.PerformTransaction(It.IsAny<String>(), It.IsAny<SerialisedMessage>(), It.IsAny<CancellationToken>()))
                                      .ReturnsAsync(TestData.SerialisedMessageResponseSale);
            
            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithOperator);

            estateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantContractsResponse);
            
            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse());
            
            Mock<IFileFormatHandler> fileFormatHandler = new Mock<IFileFormatHandler>();
            fileFormatHandler.Setup(f => f.FileLineCanBeIgnored(It.IsAny<String>())).Returns(false);
            fileFormatHandler.Setup(f => f.ParseFileLine(It.IsAny<String>())).Returns(TestData.TransactionMetadata);
            Func<String, IFileFormatHandler> fileFormatHandlerResolver = (format) =>
                                                                         {
                                                                             return fileFormatHandler.Object;
                                                                         };

            MockFileSystem fileSystem = new MockFileSystem();

            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);
            Logger.Initialise(NullLogger.Instance);

            FileRequestHandler fileRequestHandler = new FileRequestHandler(fileProcessorManager.Object,
                                                                           fileImportLogAggregateRepository.Object,
                                                                           fileAggregateRepository.Object,
                                                                           transactionProcessorClient.Object,
                                                                           estateClient.Object,
                                                                           securityServiceClient.Object,
                                                                           fileFormatHandlerResolver,
                                                                           fileSystem);

            ProcessTransactionForFileLineRequest processTransactionForFileLineRequest =
                new ProcessTransactionForFileLineRequest(TestData.FileId, TestData.LineNumber, TestData.FileLine);

            Should.NotThrow(async () =>
                            {
                                await fileRequestHandler.Handle(processTransactionForFileLineRequest, CancellationToken.None);
                            });
        }

        [Fact]
        public void FileRequestHandler_ProcessTransactionLineForFileRequest_FileAggregateNotFound_RequestHandled()
        {
            Mock<IFileProcessorManager> fileProcessorManager = new Mock<IFileProcessorManager>();
            Mock<IAggregateRepository<FileImportLogAggregate, DomainEventRecord.DomainEvent>> fileImportLogAggregateRepository =
                new Mock<IAggregateRepository<FileImportLogAggregate, DomainEventRecord.DomainEvent>>();
            Mock<IAggregateRepository<FileAggregate, DomainEventRecord.DomainEvent>> fileAggregateRepository =
                new Mock<IAggregateRepository<FileAggregate, DomainEventRecord.DomainEvent>>();
            fileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetEmptyFileAggregate);

            Mock<ITransactionProcessorClient> transactionProcessorClient = new Mock<ITransactionProcessorClient>();

            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();

            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();
            Mock<IFileFormatHandler> fileFormatHandler = new Mock<IFileFormatHandler>();
            Func<String, IFileFormatHandler> fileFormatHandlerResolver = (format) =>
            {
                return fileFormatHandler.Object;
            };

            MockFileSystem fileSystem = new MockFileSystem();

            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);
            Logger.Initialise(NullLogger.Instance);

            FileRequestHandler fileRequestHandler = new FileRequestHandler(fileProcessorManager.Object,
                                                                           fileImportLogAggregateRepository.Object,
                                                                           fileAggregateRepository.Object,
                                                                           transactionProcessorClient.Object,
                                                                           estateClient.Object,
                                                                           securityServiceClient.Object,
                                                                           fileFormatHandlerResolver,
                                                                           fileSystem);

            ProcessTransactionForFileLineRequest processTransactionForFileLineRequest =
                new ProcessTransactionForFileLineRequest(TestData.FileId, TestData.LineNumber, TestData.FileLine);

            Should.Throw<NotSupportedException>(async () =>
            {
                await fileRequestHandler.Handle(processTransactionForFileLineRequest, CancellationToken.None);
            });
        }

        [Fact]
        public void FileRequestHandler_ProcessTransactionLineForFileRequest_FileAggregateWithNoLines_RequestHandled()
        {
            Mock<IFileProcessorManager> fileProcessorManager = new Mock<IFileProcessorManager>();
            Mock<IAggregateRepository<FileImportLogAggregate, DomainEventRecord.DomainEvent>> fileImportLogAggregateRepository =
                new Mock<IAggregateRepository<FileImportLogAggregate, DomainEventRecord.DomainEvent>>();
            Mock<IAggregateRepository<FileAggregate, DomainEventRecord.DomainEvent>> fileAggregateRepository =
                new Mock<IAggregateRepository<FileAggregate, DomainEventRecord.DomainEvent>>();
            fileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetCreatedFileAggregate);

            Mock<ITransactionProcessorClient> transactionProcessorClient = new Mock<ITransactionProcessorClient>();

            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();

            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();
            Mock<IFileFormatHandler> fileFormatHandler = new Mock<IFileFormatHandler>();
            Func<String, IFileFormatHandler> fileFormatHandlerResolver = (format) =>
            {
                return fileFormatHandler.Object;
            };

            MockFileSystem fileSystem = new MockFileSystem();

            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);
            Logger.Initialise(NullLogger.Instance);

            FileRequestHandler fileRequestHandler = new FileRequestHandler(fileProcessorManager.Object,
                                                                           fileImportLogAggregateRepository.Object,
                                                                           fileAggregateRepository.Object,
                                                                           transactionProcessorClient.Object,
                                                                           estateClient.Object,
                                                                           securityServiceClient.Object,
                                                                           fileFormatHandlerResolver,
                                                                           fileSystem);

            ProcessTransactionForFileLineRequest processTransactionForFileLineRequest =
                new ProcessTransactionForFileLineRequest(TestData.FileId, TestData.LineNumber, TestData.FileLine);

            Should.Throw<NotSupportedException>(async () =>
            {
                await fileRequestHandler.Handle(processTransactionForFileLineRequest, CancellationToken.None);
            });
        }

        [Fact]
        public void FileRequestHandler_ProcessTransactionLineForFileRequest_LineInRequestNotFoundInFileAggregate_RequestHandled()
        {
            Mock<IFileProcessorManager> fileProcessorManager = new Mock<IFileProcessorManager>();
            Mock<IAggregateRepository<FileImportLogAggregate, DomainEventRecord.DomainEvent>> fileImportLogAggregateRepository =
                new Mock<IAggregateRepository<FileImportLogAggregate, DomainEventRecord.DomainEvent>>();
            Mock<IAggregateRepository<FileAggregate, DomainEventRecord.DomainEvent>> fileAggregateRepository =
                new Mock<IAggregateRepository<FileAggregate, DomainEventRecord.DomainEvent>>();
            fileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetFileAggregateWithLines);

            Mock<ITransactionProcessorClient> transactionProcessorClient = new Mock<ITransactionProcessorClient>();

            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();

            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();
            Mock<IFileFormatHandler> fileFormatHandler = new Mock<IFileFormatHandler>();
            Func<String, IFileFormatHandler> fileFormatHandlerResolver = (format) =>
            {
                return fileFormatHandler.Object;
            };

            MockFileSystem fileSystem = new MockFileSystem();

            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);
            Logger.Initialise(NullLogger.Instance);

            FileRequestHandler fileRequestHandler = new FileRequestHandler(fileProcessorManager.Object,
                                                                           fileImportLogAggregateRepository.Object,
                                                                           fileAggregateRepository.Object,
                                                                           transactionProcessorClient.Object,
                                                                           estateClient.Object,
                                                                           securityServiceClient.Object,
                                                                           fileFormatHandlerResolver,
                                                                           fileSystem);

            ProcessTransactionForFileLineRequest processTransactionForFileLineRequest =
                new ProcessTransactionForFileLineRequest(TestData.FileId, TestData.NotFoundLineNumber, TestData.FileLine);

            Should.Throw<NotFoundException>(async () =>
            {
                await fileRequestHandler.Handle(processTransactionForFileLineRequest, CancellationToken.None);
            });
        }

        [Fact]
        public void FileRequestHandler_ProcessTransactionLineForFileRequest_LineInRequestAlreadyProcessed_RequestHandled()
        {
            Mock<IFileProcessorManager> fileProcessorManager = new Mock<IFileProcessorManager>();
            Mock<IAggregateRepository<FileImportLogAggregate, DomainEventRecord.DomainEvent>> fileImportLogAggregateRepository =
                new Mock<IAggregateRepository<FileImportLogAggregate, DomainEventRecord.DomainEvent>>();
            Mock<IAggregateRepository<FileAggregate, DomainEventRecord.DomainEvent>> fileAggregateRepository =
                new Mock<IAggregateRepository<FileAggregate, DomainEventRecord.DomainEvent>>();
            fileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetFileAggregateWithLinesAlreadyProcessed);

            Mock<ITransactionProcessorClient> transactionProcessorClient = new Mock<ITransactionProcessorClient>();

            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();

            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();
            Mock<IFileFormatHandler> fileFormatHandler = new Mock<IFileFormatHandler>();
            Func<String, IFileFormatHandler> fileFormatHandlerResolver = (format) =>
            {
                return fileFormatHandler.Object;
            };

            MockFileSystem fileSystem = new MockFileSystem();

            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);
            Logger.Initialise(NullLogger.Instance);

            FileRequestHandler fileRequestHandler = new FileRequestHandler(fileProcessorManager.Object,
                                                                           fileImportLogAggregateRepository.Object,
                                                                           fileAggregateRepository.Object,
                                                                           transactionProcessorClient.Object,
                                                                           estateClient.Object,
                                                                           securityServiceClient.Object,
                                                                           fileFormatHandlerResolver,
                                                                           fileSystem);

            ProcessTransactionForFileLineRequest processTransactionForFileLineRequest =
                new ProcessTransactionForFileLineRequest(TestData.FileId, TestData.LineNumber, TestData.FileLine);

            Should.NotThrow(async () =>
            {
                await fileRequestHandler.Handle(processTransactionForFileLineRequest, CancellationToken.None);
            });
        }

        [Fact]
        public void FileRequestHandler_ProcessTransactionForFileLineRequest_FileProfileNotFound_RequestIsHandled()
        {
            Mock<IFileProcessorManager> fileProcessorManager = new Mock<IFileProcessorManager>();
            Mock<IAggregateRepository<FileImportLogAggregate, DomainEventRecord.DomainEvent>> fileImportLogAggregateRepository =
                new Mock<IAggregateRepository<FileImportLogAggregate, DomainEventRecord.DomainEvent>>();

            Mock<IAggregateRepository<FileAggregate, DomainEventRecord.DomainEvent>> fileAggregateRepository =
                new Mock<IAggregateRepository<FileAggregate, DomainEventRecord.DomainEvent>>();
            fileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetFileAggregateWithLines);

            Mock<ITransactionProcessorClient> transactionProcessorClient = new Mock<ITransactionProcessorClient>();

            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();

            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();

            Mock<IFileFormatHandler> fileFormatHandler = new Mock<IFileFormatHandler>();
            Func<String, IFileFormatHandler> fileFormatHandlerResolver = (format) =>
            {
                return fileFormatHandler.Object;
            };

            MockFileSystem fileSystem = new MockFileSystem();

            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);
            Logger.Initialise(NullLogger.Instance);

            FileRequestHandler fileRequestHandler = new FileRequestHandler(fileProcessorManager.Object,
                                                                           fileImportLogAggregateRepository.Object,
                                                                           fileAggregateRepository.Object,
                                                                           transactionProcessorClient.Object,
                                                                           estateClient.Object,
                                                                           securityServiceClient.Object,
                                                                           fileFormatHandlerResolver,
                                                                           fileSystem);

            ProcessTransactionForFileLineRequest processTransactionForFileLineRequest =
                new ProcessTransactionForFileLineRequest(TestData.FileId, TestData.LineNumber, TestData.FileLine);

            Should.Throw<NotFoundException>(async () =>
            {
                await fileRequestHandler.Handle(processTransactionForFileLineRequest, CancellationToken.None);
            });
        }

        [Fact]
        public void FileRequestHandler_ProcessTransactionForFileLineRequest_FileLineIgnored_RequestIsHandled()
        {
            Mock<IFileProcessorManager> fileProcessorManager = new Mock<IFileProcessorManager>();
            fileProcessorManager.Setup(f => f.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.FileProfileSafaricom);
            Mock<IAggregateRepository<FileImportLogAggregate, DomainEventRecord.DomainEvent>> fileImportLogAggregateRepository =
                new Mock<IAggregateRepository<FileImportLogAggregate, DomainEventRecord.DomainEvent>>();

            Mock<IAggregateRepository<FileAggregate, DomainEventRecord.DomainEvent>> fileAggregateRepository =
                new Mock<IAggregateRepository<FileAggregate, DomainEventRecord.DomainEvent>>();
            fileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetFileAggregateWithLines);

            Mock<ITransactionProcessorClient> transactionProcessorClient = new Mock<ITransactionProcessorClient>();
            transactionProcessorClient.Setup(t => t.PerformTransaction(It.IsAny<String>(), It.IsAny<SerialisedMessage>(), It.IsAny<CancellationToken>()))
                                      .ReturnsAsync(TestData.SerialisedMessageResponseSale);

            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithOperator);

            estateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantContractsResponse);

            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse());

            Mock<IFileFormatHandler> fileFormatHandler = new Mock<IFileFormatHandler>();
            fileFormatHandler.Setup(f => f.FileLineCanBeIgnored(It.IsAny<String>())).Returns(true);
            
            Func<String, IFileFormatHandler> fileFormatHandlerResolver = (format) =>
            {
                return fileFormatHandler.Object;
            };

            MockFileSystem fileSystem = new MockFileSystem();

            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);
            Logger.Initialise(NullLogger.Instance);

            FileRequestHandler fileRequestHandler = new FileRequestHandler(fileProcessorManager.Object,
                                                                           fileImportLogAggregateRepository.Object,
                                                                           fileAggregateRepository.Object,
                                                                           transactionProcessorClient.Object,
                                                                           estateClient.Object,
                                                                           securityServiceClient.Object,
                                                                           fileFormatHandlerResolver,
                                                                           fileSystem);

            ProcessTransactionForFileLineRequest processTransactionForFileLineRequest =
                new ProcessTransactionForFileLineRequest(TestData.FileId, TestData.LineNumber, TestData.FileLine);

            Should.NotThrow(async () =>
            {
                await fileRequestHandler.Handle(processTransactionForFileLineRequest, CancellationToken.None);
            });
            fileFormatHandler.Verify(f => f.ParseFileLine(It.IsAny<String>()), Times.Never);
        }

        [Fact]
        public void FileRequestHandler_ProcessTransactionForFileLineRequest_MerchantNotFound_RequestIsHandled()
        {
            Mock<IFileProcessorManager> fileProcessorManager = new Mock<IFileProcessorManager>();
            fileProcessorManager.Setup(f => f.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.FileProfileSafaricom);
            Mock<IAggregateRepository<FileImportLogAggregate, DomainEventRecord.DomainEvent>> fileImportLogAggregateRepository =
                new Mock<IAggregateRepository<FileImportLogAggregate, DomainEventRecord.DomainEvent>>();

            Mock<IAggregateRepository<FileAggregate, DomainEventRecord.DomainEvent>> fileAggregateRepository =
                new Mock<IAggregateRepository<FileAggregate, DomainEventRecord.DomainEvent>>();
            fileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetFileAggregateWithLines);

            Mock<ITransactionProcessorClient> transactionProcessorClient = new Mock<ITransactionProcessorClient>();
            transactionProcessorClient.Setup(t => t.PerformTransaction(It.IsAny<String>(), It.IsAny<SerialisedMessage>(), It.IsAny<CancellationToken>()))
                                      .ReturnsAsync(TestData.SerialisedMessageResponseSale);

            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();
            
            estateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantContractsResponse);

            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse());

            Mock<IFileFormatHandler> fileFormatHandler = new Mock<IFileFormatHandler>();
            fileFormatHandler.Setup(f => f.FileLineCanBeIgnored(It.IsAny<String>())).Returns(false);
            fileFormatHandler.Setup(f => f.ParseFileLine(It.IsAny<String>())).Returns(TestData.TransactionMetadata);
            Func<String, IFileFormatHandler> fileFormatHandlerResolver = (format) =>
            {
                return fileFormatHandler.Object;
            };

            MockFileSystem fileSystem = new MockFileSystem();

            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);
            Logger.Initialise(NullLogger.Instance);

            FileRequestHandler fileRequestHandler = new FileRequestHandler(fileProcessorManager.Object,
                                                                           fileImportLogAggregateRepository.Object,
                                                                           fileAggregateRepository.Object,
                                                                           transactionProcessorClient.Object,
                                                                           estateClient.Object,
                                                                           securityServiceClient.Object,
                                                                           fileFormatHandlerResolver,
                                                                           fileSystem);

            ProcessTransactionForFileLineRequest processTransactionForFileLineRequest =
                new ProcessTransactionForFileLineRequest(TestData.FileId, TestData.LineNumber, TestData.FileLine);

            Should.Throw<NotFoundException>(async () =>
            {
                await fileRequestHandler.Handle(processTransactionForFileLineRequest, CancellationToken.None);
            });
        }

        [Fact]
        public void FileRequestHandler_ProcessTransactionForFileLineRequest_NoMerchantContractsFound_RequestIsHandled()
        {
            Mock<IFileProcessorManager> fileProcessorManager = new Mock<IFileProcessorManager>();
            fileProcessorManager.Setup(f => f.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.FileProfileSafaricom);
            Mock<IAggregateRepository<FileImportLogAggregate, DomainEventRecord.DomainEvent>> fileImportLogAggregateRepository =
                new Mock<IAggregateRepository<FileImportLogAggregate, DomainEventRecord.DomainEvent>>();

            Mock<IAggregateRepository<FileAggregate, DomainEventRecord.DomainEvent>> fileAggregateRepository =
                new Mock<IAggregateRepository<FileAggregate, DomainEventRecord.DomainEvent>>();
            fileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetFileAggregateWithLines);

            Mock<ITransactionProcessorClient> transactionProcessorClient = new Mock<ITransactionProcessorClient>();
            transactionProcessorClient.Setup(t => t.PerformTransaction(It.IsAny<String>(), It.IsAny<SerialisedMessage>(), It.IsAny<CancellationToken>()))
                                      .ReturnsAsync(TestData.SerialisedMessageResponseSale);

            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithOperator);

            estateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetEmptyMerchantContractsResponse);

            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse());

            Mock<IFileFormatHandler> fileFormatHandler = new Mock<IFileFormatHandler>();
            fileFormatHandler.Setup(f => f.FileLineCanBeIgnored(It.IsAny<String>())).Returns(false);
            fileFormatHandler.Setup(f => f.ParseFileLine(It.IsAny<String>())).Returns(TestData.TransactionMetadata);
            Func<String, IFileFormatHandler> fileFormatHandlerResolver = (format) =>
            {
                return fileFormatHandler.Object;
            };

            MockFileSystem fileSystem = new MockFileSystem();

            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);
            Logger.Initialise(NullLogger.Instance);

            FileRequestHandler fileRequestHandler = new FileRequestHandler(fileProcessorManager.Object,
                                                                           fileImportLogAggregateRepository.Object,
                                                                           fileAggregateRepository.Object,
                                                                           transactionProcessorClient.Object,
                                                                           estateClient.Object,
                                                                           securityServiceClient.Object,
                                                                           fileFormatHandlerResolver,
                                                                           fileSystem);

            ProcessTransactionForFileLineRequest processTransactionForFileLineRequest =
                new ProcessTransactionForFileLineRequest(TestData.FileId, TestData.LineNumber, TestData.FileLine);

            Should.Throw<NotFoundException>(async () =>
                                            {
                                                await fileRequestHandler.Handle(processTransactionForFileLineRequest, CancellationToken.None);
                                            });
        }

        [Fact]
        public void FileRequestHandler_ProcessTransactionForFileLineRequest_NoMerchantContractForFileOperatorFound_RequestIsHandled()
        {
            Mock<IFileProcessorManager> fileProcessorManager = new Mock<IFileProcessorManager>();
            fileProcessorManager.Setup(f => f.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.FileProfileSafaricom);
            Mock<IAggregateRepository<FileImportLogAggregate, DomainEventRecord.DomainEvent>> fileImportLogAggregateRepository =
                new Mock<IAggregateRepository<FileImportLogAggregate, DomainEventRecord.DomainEvent>>();

            Mock<IAggregateRepository<FileAggregate, DomainEventRecord.DomainEvent>> fileAggregateRepository =
                new Mock<IAggregateRepository<FileAggregate, DomainEventRecord.DomainEvent>>();
            fileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetFileAggregateWithLines);

            Mock<ITransactionProcessorClient> transactionProcessorClient = new Mock<ITransactionProcessorClient>();
            transactionProcessorClient.Setup(t => t.PerformTransaction(It.IsAny<String>(), It.IsAny<SerialisedMessage>(), It.IsAny<CancellationToken>()))
                                      .ReturnsAsync(TestData.SerialisedMessageResponseSale);

            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithOperator);

            estateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantContractsNoFileOperatorResponse);

            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse());

            Mock<IFileFormatHandler> fileFormatHandler = new Mock<IFileFormatHandler>();
            fileFormatHandler.Setup(f => f.FileLineCanBeIgnored(It.IsAny<String>())).Returns(false);
            fileFormatHandler.Setup(f => f.ParseFileLine(It.IsAny<String>())).Returns(TestData.TransactionMetadata);
            Func<String, IFileFormatHandler> fileFormatHandlerResolver = (format) =>
            {
                return fileFormatHandler.Object;
            };

            MockFileSystem fileSystem = new MockFileSystem();

            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);
            Logger.Initialise(NullLogger.Instance);

            FileRequestHandler fileRequestHandler = new FileRequestHandler(fileProcessorManager.Object,
                                                                           fileImportLogAggregateRepository.Object,
                                                                           fileAggregateRepository.Object,
                                                                           transactionProcessorClient.Object,
                                                                           estateClient.Object,
                                                                           securityServiceClient.Object,
                                                                           fileFormatHandlerResolver,
                                                                           fileSystem);

            ProcessTransactionForFileLineRequest processTransactionForFileLineRequest =
                new ProcessTransactionForFileLineRequest(TestData.FileId, TestData.LineNumber, TestData.FileLine);

            Should.Throw<NotFoundException>(async () =>
            {
                await fileRequestHandler.Handle(processTransactionForFileLineRequest, CancellationToken.None);
            });
        }

        [Fact]
        public void FileRequestHandler_ProcessTransactionForFileLineRequest_MerchantContractProductNotFound_RequestIsHandled()
        {
            Mock<IFileProcessorManager> fileProcessorManager = new Mock<IFileProcessorManager>();
            fileProcessorManager.Setup(f => f.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.FileProfileSafaricom);
            Mock<IAggregateRepository<FileImportLogAggregate, DomainEventRecord.DomainEvent>> fileImportLogAggregateRepository =
                new Mock<IAggregateRepository<FileImportLogAggregate, DomainEventRecord.DomainEvent>>();

            Mock<IAggregateRepository<FileAggregate, DomainEventRecord.DomainEvent>> fileAggregateRepository =
                new Mock<IAggregateRepository<FileAggregate, DomainEventRecord.DomainEvent>>();
            fileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetFileAggregateWithLines);

            Mock<ITransactionProcessorClient> transactionProcessorClient = new Mock<ITransactionProcessorClient>();
            transactionProcessorClient.Setup(t => t.PerformTransaction(It.IsAny<String>(), It.IsAny<SerialisedMessage>(), It.IsAny<CancellationToken>()))
                                      .ReturnsAsync(TestData.SerialisedMessageResponseSale);

            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithOperator);

            estateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantContractsResponseNoNullValueProduct);

            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse());

            Mock<IFileFormatHandler> fileFormatHandler = new Mock<IFileFormatHandler>();
            fileFormatHandler.Setup(f => f.FileLineCanBeIgnored(It.IsAny<String>())).Returns(false);
            fileFormatHandler.Setup(f => f.ParseFileLine(It.IsAny<String>())).Returns(TestData.TransactionMetadata);
            Func<String, IFileFormatHandler> fileFormatHandlerResolver = (format) =>
            {
                return fileFormatHandler.Object;
            };

            MockFileSystem fileSystem = new MockFileSystem();

            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);
            Logger.Initialise(NullLogger.Instance);

            FileRequestHandler fileRequestHandler = new FileRequestHandler(fileProcessorManager.Object,
                                                                           fileImportLogAggregateRepository.Object,
                                                                           fileAggregateRepository.Object,
                                                                           transactionProcessorClient.Object,
                                                                           estateClient.Object,
                                                                           securityServiceClient.Object,
                                                                           fileFormatHandlerResolver,
                                                                           fileSystem);

            ProcessTransactionForFileLineRequest processTransactionForFileLineRequest =
                new ProcessTransactionForFileLineRequest(TestData.FileId, TestData.LineNumber, TestData.FileLine);

            Should.Throw<NotFoundException>(async () =>
            {
                await fileRequestHandler.Handle(processTransactionForFileLineRequest, CancellationToken.None);
            });
        }

        [Fact]
        public void FileRequestHandler_ProcessTransactionForFileLineRequest_TransactionNotSuccessful_RequestIsHandled()
        {
            Mock<IFileProcessorManager> fileProcessorManager = new Mock<IFileProcessorManager>();
            fileProcessorManager.Setup(f => f.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.FileProfileSafaricom);
            Mock<IAggregateRepository<FileImportLogAggregate, DomainEventRecord.DomainEvent>> fileImportLogAggregateRepository =
                new Mock<IAggregateRepository<FileImportLogAggregate, DomainEventRecord.DomainEvent>>();

            Mock<IAggregateRepository<FileAggregate, DomainEventRecord.DomainEvent>> fileAggregateRepository =
                new Mock<IAggregateRepository<FileAggregate, DomainEventRecord.DomainEvent>>();
            fileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetFileAggregateWithLines);

            Mock<ITransactionProcessorClient> transactionProcessorClient = new Mock<ITransactionProcessorClient>();
            transactionProcessorClient.Setup(t => t.PerformTransaction(It.IsAny<String>(), It.IsAny<SerialisedMessage>(), It.IsAny<CancellationToken>()))
                                      .ReturnsAsync(TestData.SerialisedMessageResponseFailedSale);

            Mock<IEstateClient> estateClient = new Mock<IEstateClient>();
            estateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantResponseWithOperator);

            estateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(TestData.GetMerchantContractsResponse);

            Mock<ISecurityServiceClient> securityServiceClient = new Mock<ISecurityServiceClient>();
            securityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse());

            Mock<IFileFormatHandler> fileFormatHandler = new Mock<IFileFormatHandler>();
            fileFormatHandler.Setup(f => f.FileLineCanBeIgnored(It.IsAny<String>())).Returns(false);
            fileFormatHandler.Setup(f => f.ParseFileLine(It.IsAny<String>())).Returns(TestData.TransactionMetadata);
            Func<String, IFileFormatHandler> fileFormatHandlerResolver = (format) =>
            {
                return fileFormatHandler.Object;
            };

            MockFileSystem fileSystem = new MockFileSystem();

            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);
            Logger.Initialise(NullLogger.Instance);

            FileRequestHandler fileRequestHandler = new FileRequestHandler(fileProcessorManager.Object,
                                                                           fileImportLogAggregateRepository.Object,
                                                                           fileAggregateRepository.Object,
                                                                           transactionProcessorClient.Object,
                                                                           estateClient.Object,
                                                                           securityServiceClient.Object,
                                                                           fileFormatHandlerResolver,
                                                                           fileSystem);

            ProcessTransactionForFileLineRequest processTransactionForFileLineRequest =
                new ProcessTransactionForFileLineRequest(TestData.FileId, TestData.LineNumber, TestData.FileLine);

            Should.NotThrow(async () =>
            {
                await fileRequestHandler.Handle(processTransactionForFileLineRequest, CancellationToken.None);
            });
        }
    }
}
