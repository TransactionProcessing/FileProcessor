namespace FileProcessor.BusinessLogic.Tests;

using System;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Threading;
using System.Threading.Tasks;
using EstateManagement.Client;
using FileAggregate;
using FileFormatHandlers;
using FileImportLogAggregate;
using Managers;
using Microsoft.Extensions.Configuration;
using Moq;
using Requests;
using SecurityService.Client;
using Services;
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

public class FileProcessorDomainServiceTests
{
    private Mock<IFileProcessorManager> FileProcessorManager;

    private Mock<IAggregateRepository<FileImportLogAggregate, DomainEvent>> FileImportLogAggregateRepository;

    private Mock<IAggregateRepository<FileAggregate, DomainEvent>> FileAggregateRepository;

    private Mock<ITransactionProcessorClient> TransactionProcessorClient;

    private Mock<IEstateClient> EstateClient;

    private Mock<ISecurityServiceClient> SecurityServiceClient;

    private Mock<IFileFormatHandler> FileFormatHandler;

    private FileProcessorDomainService FileProcessorDomainService;

    private MockFileSystem FileSystem;
    public FileProcessorDomainServiceTests()
    {
        this.FileProcessorManager = new Mock<IFileProcessorManager>();
        this.FileImportLogAggregateRepository =
            new Mock<IAggregateRepository<FileImportLogAggregate, DomainEvent>>();
        this.FileAggregateRepository =
            new Mock<IAggregateRepository<FileAggregate, DomainEvent>>();
        this.TransactionProcessorClient = new Mock<ITransactionProcessorClient>();
        this.EstateClient = new Mock<IEstateClient>();
        this.SecurityServiceClient = new Mock<ISecurityServiceClient>();
        this.FileFormatHandler = new Mock<IFileFormatHandler>();
        this.FileSystem = new MockFileSystem();

        Func<String, IFileFormatHandler> fileFormatHandlerResolver = (format) =>
                                                                     {
                                                                         return this.FileFormatHandler.Object;
                                                                     };

        this.FileProcessorDomainService = new FileProcessorDomainService(this.FileProcessorManager.Object,
                                                                         this.FileImportLogAggregateRepository.Object,
                                                                         this.FileAggregateRepository.Object,
                                                                         this.TransactionProcessorClient.Object,
                                                                         this.EstateClient.Object,
                                                                         this.SecurityServiceClient.Object,
                                                                         fileFormatHandlerResolver,
                                                                         this.FileSystem);
        Logger.Initialise(NullLogger.Instance);
    }

    [Fact]
    public async Task FileRequestHandler_UploadFileRequest_RequestIsHandled()
    {
        this.FileProcessorManager.Setup(f => f.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.FileProfileSafaricom);

        this.FileImportLogAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetEmptyFileImportLogAggregate);
            
        this.FileSystem.AddFile(TestData.FilePathWithName, new MockFileData("D,1,1,1"));
        this.FileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom");

        UploadFileRequest uploadFileRequest =
            new UploadFileRequest(TestData.EstateId, TestData.MerchantId, TestData.UserId, TestData.FilePathWithName, TestData.FileProfileId, TestData.FileUploadedDateTime);

        Should.NotThrow(async () =>
                        {
                            await this.FileProcessorDomainService.UploadFile(uploadFileRequest, CancellationToken.None);
                        });
    }

    [Fact]
    public async Task FileRequestHandler_UploadFileRequest_MerchantIdNotProvided_ErrorThrown()
    {
        UploadFileRequest uploadFileRequest =
            new UploadFileRequest(TestData.EstateId, Guid.Empty, TestData.UserId, TestData.FilePathWithName, TestData.FileProfileId, TestData.FileUploadedDateTime);

        Should.Throw<InvalidDataException>(async () =>
                                           {
                                               await this.FileProcessorDomainService.UploadFile(uploadFileRequest, CancellationToken.None);
                                           });
    }

    [Fact]
    public async Task FileRequestHandler_UploadFileRequest_FileProfileIdNotProvided_ErrorThrown()
    {
        UploadFileRequest uploadFileRequest =
            new UploadFileRequest(TestData.EstateId, TestData.MerchantId, TestData.UserId, TestData.FilePathWithName, Guid.Empty, TestData.FileUploadedDateTime);

        Should.Throw<InvalidDataException>(async () =>
                                           {
                                               await this.FileProcessorDomainService.UploadFile(uploadFileRequest, CancellationToken.None);
                                           });
    }

    [Fact]
    public async Task FileRequestHandler_UploadFileRequest_UserIdNotProvided_ErrorThrown()
    {
        UploadFileRequest uploadFileRequest =
            new UploadFileRequest(TestData.EstateId, TestData.MerchantId, Guid.Empty, TestData.FilePathWithName, TestData.FileProfileId, TestData.FileUploadedDateTime);

        Should.Throw<InvalidDataException>(async () =>
                                           {
                                               await this.FileProcessorDomainService.UploadFile(uploadFileRequest, CancellationToken.None);
                                           });
    }

    [Fact]
    public async Task FileRequestHandler_UploadFileRequest_ImportLogAlreadyCreated_RequestIsHandled()
    {
        this.FileProcessorManager.Setup(f => f.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.FileProfileSafaricom);

        this.FileImportLogAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetCreatedFileImportLogAggregate);
            
        this.FileSystem.AddFile(TestData.FilePathWithName, new MockFileData("D,1,1,1"));
        this.FileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom");
            
        UploadFileRequest uploadFileRequest =
            new UploadFileRequest(TestData.EstateId, TestData.MerchantId, TestData.UserId, TestData.FilePathWithName, TestData.FileProfileId, TestData.FileUploadedDateTime);

        Should.NotThrow(async () =>
                        {
                            await this.FileProcessorDomainService.UploadFile(uploadFileRequest, CancellationToken.None);
                        });
    }

    [Fact]
    public async Task FileRequestHandler_UploadFileRequest_NoFileProfiles_RequestIsHandled()
    {
        this.FileProcessorManager.Setup(f => f.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.FileProfileNull);

        this.FileImportLogAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetCreatedFileImportLogAggregate);
            
        UploadFileRequest uploadFileRequest =
            new UploadFileRequest(TestData.EstateId, TestData.MerchantId, TestData.UserId, TestData.FilePathWithName, TestData.FileProfileId, TestData.FileUploadedDateTime);

        Should.Throw<NotFoundException>(async () =>
                                        {
                                            await this.FileProcessorDomainService.UploadFile(uploadFileRequest, CancellationToken.None);
                                        });
    }

    [Fact]
    public async Task FileRequestHandler_UploadFileRequest_FileNotFound_RequestIsHandled()
    {
        this.FileProcessorManager.Setup(f => f.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.FileProfileSafaricom);

        this.FileImportLogAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetCreatedFileImportLogAggregate);
            
        UploadFileRequest uploadFileRequest =
            new UploadFileRequest(TestData.EstateId, TestData.MerchantId, TestData.UserId, TestData.FilePathWithName, TestData.FileProfileId, TestData.FileUploadedDateTime);

        Should.Throw<FileNotFoundException>(async () =>
                                            {
                                                await this.FileProcessorDomainService.UploadFile(uploadFileRequest, CancellationToken.None);
                                            });
    }

    [Fact]
    public async Task FileRequestHandler_UploadFileRequest_DestinationDirectoryNotFound_RequestIsHandled()
    {
        this.FileProcessorManager.Setup(f => f.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.FileProfileSafaricom);

        this.FileImportLogAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetCreatedFileImportLogAggregate);

        this.FileSystem.AddFile(TestData.FilePathWithName, new MockFileData("D,1,1,1"));

        Logger.Initialise(NullLogger.Instance);

        UploadFileRequest uploadFileRequest =
            new UploadFileRequest(TestData.EstateId, TestData.MerchantId, TestData.UserId, TestData.FilePathWithName, TestData.FileProfileId, TestData.FileUploadedDateTime);

        Should.Throw<DirectoryNotFoundException>(async () =>
                                                 {
                                                     await this.FileProcessorDomainService.UploadFile(uploadFileRequest, CancellationToken.None);
                                                 });
    }

    [Fact]
    public void FileRequestHandler_ProcessUploadedFileRequest_RequestIsHandled()
    {
        this.FileProcessorManager.Setup(f => f.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.FileProfileSafaricom);
        this.FileAggregateRepository.SetupSequence(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetEmptyFileAggregate).ReturnsAsync(TestData.GetCreatedFileAggregate);

        this.FileSystem.AddFile(TestData.FilePathWithName, new MockFileData("D,1,1,1"));

        this.FileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom/inprogress");
        this.FileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom/processed");
        this.FileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom/failed");

        ProcessUploadedFileRequest processUploadedFileRequest =
            new ProcessUploadedFileRequest(TestData.EstateId, TestData.MerchantId, TestData.FileImportLogId, TestData.FileId, TestData.UserId, TestData.FilePathWithName, TestData.FileProfileId, TestData.FileUploadedDateTime);

        Should.NotThrow(async () =>
                        {
                            await this.FileProcessorDomainService.ProcessUploadedFile(processUploadedFileRequest, CancellationToken.None);
                        });
    }

    [Fact]
    public void FileRequestHandler_ProcessUploadedFileRequest_FileNotFound_RequestIsHandled()
    {
        this.FileProcessorManager.Setup(f => f.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.FileProfileSafaricom);

        this.FileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetCreatedFileAggregate);

        this.FileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom/inprogress");
        this.FileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom/processed");
        this.FileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom/failed");

        Logger.Initialise(NullLogger.Instance);

        ProcessUploadedFileRequest processUploadedFileRequest =
            new ProcessUploadedFileRequest(TestData.EstateId, TestData.MerchantId, TestData.FileImportLogId, TestData.FileId, TestData.UserId, TestData.FilePathWithName, TestData.FileProfileId, TestData.FileUploadedDateTime);

        Should.Throw<FileNotFoundException>(async () =>
                                            {
                                                await this.FileProcessorDomainService.ProcessUploadedFile(processUploadedFileRequest, CancellationToken.None);
                                            });
    }

    [Fact]
    public async Task FileRequestHandler_ProcessUploadedFileRequest_NoFileProfiles_RequestIsHandled()
    {
        this.FileProcessorManager.Setup(f => f.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.FileProfileNull);

        this.FileImportLogAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEmptyFileImportLogAggregate);

        this.FileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetCreatedFileAggregate);

        this.FileSystem.AddFile(TestData.FilePathWithName, new MockFileData("D,1,1,1"));

        Logger.Initialise(NullLogger.Instance);

        ProcessUploadedFileRequest processUploadedFileRequest =
            new ProcessUploadedFileRequest(TestData.EstateId, TestData.MerchantId, TestData.FileImportLogId, TestData.FileId, TestData.UserId, TestData.FilePathWithName, TestData.FileProfileId, TestData.FileUploadedDateTime);

        Should.Throw<NotFoundException>(async () =>
                                        {
                                            await this.FileProcessorDomainService.ProcessUploadedFile(processUploadedFileRequest, CancellationToken.None);
                                        });
    }

    [Fact]
    public async Task FileRequestHandler_ProcessUploadedFileRequest_ProcessedDirectoryNotFound_RequestIsHandled()
    {
        this.FileProcessorManager.Setup(f => f.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.FileProfileSafaricom);

        this.FileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetCreatedFileAggregate);

        this.FileSystem.AddFile(TestData.FilePathWithName, new MockFileData("D,1,1,1"));

        this.FileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom/inprogress");
        this.FileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom/failed");

        ProcessUploadedFileRequest processUploadedFileRequest =
            new ProcessUploadedFileRequest(TestData.EstateId, TestData.MerchantId, TestData.FileImportLogId, TestData.FileId, TestData.UserId, TestData.FilePathWithName, TestData.FileProfileId, TestData.FileUploadedDateTime);

        Should.NotThrow(async () =>
                                            {
                                                await this.FileProcessorDomainService.ProcessUploadedFile(processUploadedFileRequest, CancellationToken.None);
                                            });
        this.VerifyFileProcessing("home/txnproc/bulkfiles/safaricom/processed");
    }

    [Fact]
    public async Task FileRequestHandler_ProcessUploadedFileRequest_FailedDirectoryNotFound_RequestIsHandled()
    {
        this.FileProcessorManager.Setup(f => f.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.FileProfileSafaricom);

        this.FileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetCreatedFileAggregate);

        this.FileSystem.AddFile(TestData.FilePathWithName, new MockFileData("D,1,1,1"));

        this.FileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom/inprogress");
        this.FileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom/processed");

        ProcessUploadedFileRequest processUploadedFileRequest =
            new ProcessUploadedFileRequest(TestData.EstateId, TestData.MerchantId, TestData.FileImportLogId, TestData.FileId, TestData.UserId, TestData.FilePathWithName, TestData.FileProfileId, TestData.FileUploadedDateTime);

        Should.NotThrow(async () =>
                        {
                            await this.FileProcessorDomainService.ProcessUploadedFile(processUploadedFileRequest, CancellationToken.None);
                        });
        this.VerifyFileProcessing("home/txnproc/bulkfiles/safaricom/processed");
    }

    [Fact]
    public void FileRequestHandler_ProcessUploadedFileRequest_FileIsEmpty_RequestIsHandled()
    {
        this.FileProcessorManager.Setup(f => f.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.FileProfileSafaricom);

        this.FileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetCreatedFileAggregate);

        this.FileSystem.AddFile(TestData.FilePathWithName, new MockFileData(String.Empty));

        this.FileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom/inprogress");
        this.FileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom/processed");
        this.FileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom/failed");

        ProcessUploadedFileRequest processUploadedFileRequest =
            new ProcessUploadedFileRequest(TestData.EstateId, TestData.MerchantId, TestData.FileImportLogId, TestData.FileId, TestData.UserId, TestData.FilePathWithName, TestData.FileProfileId, TestData.FileUploadedDateTime);

        Should.NotThrow(async () =>
                        {
                            await this.FileProcessorDomainService.ProcessUploadedFile(processUploadedFileRequest, CancellationToken.None);
                        });

        this.FileAggregateRepository.Verify(f => f.SaveChanges(It.IsAny<FileAggregate>(), It.IsAny<CancellationToken>()), Times.Once);
        this.VerifyFileProcessing("home/txnproc/bulkfiles/safaricom/processed");
    }


    [Theory]
    [InlineData("Safaricom")]
    [InlineData("Voucher")]
    public void FileRequestHandler_ProcessTransactionForFileLineRequest_RequestIsHandled(String operatorName)
    {
        this.FileProcessorManager.Setup(f => f.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetFileProfile(operatorName));

        this.FileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetFileAggregateWithLines);
            
        this.TransactionProcessorClient.Setup(t => t.PerformTransaction(It.IsAny<String>(), It.IsAny<SerialisedMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.SerialisedMessageResponseSale);
            
        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantResponseWithOperator);

        this.EstateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantContractsResponse);
            
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse());

        this.FileFormatHandler.Setup(f => f.FileLineCanBeIgnored(It.IsAny<String>())).Returns(false);
        this.FileFormatHandler.Setup(f => f.ParseFileLine(It.IsAny<String>())).Returns(TestData.TransactionMetadata);
           

        IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
        ConfigurationReader.Initialise(configurationRoot);
        Logger.Initialise(NullLogger.Instance);

        ProcessTransactionForFileLineRequest processTransactionForFileLineRequest =
            new ProcessTransactionForFileLineRequest(TestData.FileId, TestData.LineNumber, TestData.FileLine);

        Should.NotThrow(async () =>
                        {
                            await this.FileProcessorDomainService.ProcessTransactionForFileLine(processTransactionForFileLineRequest, CancellationToken.None);
                        });
    }

    [Fact]
    public void FileRequestHandler_ProcessTransactionForFileLineRequest_WithOperatorName_RequestIsHandled()
    {
        this.FileProcessorManager.Setup(f => f.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.FileProfileVoucher);

        this.FileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetFileAggregateWithLines);

        this.TransactionProcessorClient.Setup(t => t.PerformTransaction(It.IsAny<String>(), It.IsAny<SerialisedMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.SerialisedMessageResponseSale);

        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantResponseWithOperator);

        this.EstateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantContractsResponse);

        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse());

        this.FileFormatHandler.Setup(f => f.FileLineCanBeIgnored(It.IsAny<String>())).Returns(false);
        this.FileFormatHandler.Setup(f => f.ParseFileLine(It.IsAny<String>())).Returns(TestData.TransactionMetadataWithOperatorName);
            
        IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
        ConfigurationReader.Initialise(configurationRoot);
        Logger.Initialise(NullLogger.Instance);

        ProcessTransactionForFileLineRequest processTransactionForFileLineRequest =
            new ProcessTransactionForFileLineRequest(TestData.FileId, TestData.LineNumber, TestData.FileLine);

        Should.NotThrow(async () =>
                        {
                            await this.FileProcessorDomainService.ProcessTransactionForFileLine(processTransactionForFileLineRequest, CancellationToken.None);
                        });
    }

    [Fact]
    public void FileRequestHandler_ProcessTransactionLineForFileRequest_FileAggregateNotFound_RequestHandled()
    {
        this.FileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetEmptyFileAggregate);
            
        IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
        ConfigurationReader.Initialise(configurationRoot);
        Logger.Initialise(NullLogger.Instance);
            
        ProcessTransactionForFileLineRequest processTransactionForFileLineRequest =
            new ProcessTransactionForFileLineRequest(TestData.FileId, TestData.LineNumber, TestData.FileLine);

        Should.Throw<NotSupportedException>(async () =>
                                            {
                                                await this.FileProcessorDomainService.ProcessTransactionForFileLine(processTransactionForFileLineRequest, CancellationToken.None);
                                            });
    }

    [Fact]
    public void FileRequestHandler_ProcessTransactionLineForFileRequest_FileAggregateWithNoLines_RequestHandled()
    {
        this.FileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetCreatedFileAggregate);
            
        IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
        ConfigurationReader.Initialise(configurationRoot);
        Logger.Initialise(NullLogger.Instance);
            
        ProcessTransactionForFileLineRequest processTransactionForFileLineRequest =
            new ProcessTransactionForFileLineRequest(TestData.FileId, TestData.LineNumber, TestData.FileLine);

        Should.Throw<NotSupportedException>(async () =>
                                            {
                                                await this.FileProcessorDomainService.ProcessTransactionForFileLine(processTransactionForFileLineRequest, CancellationToken.None);
                                            });
    }

    [Fact]
    public void FileRequestHandler_ProcessTransactionLineForFileRequest_LineInRequestNotFoundInFileAggregate_RequestHandled()
    {
        this.FileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetFileAggregateWithLines);
            
        IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
        ConfigurationReader.Initialise(configurationRoot);
        Logger.Initialise(NullLogger.Instance);
            
        ProcessTransactionForFileLineRequest processTransactionForFileLineRequest =
            new ProcessTransactionForFileLineRequest(TestData.FileId, TestData.NotFoundLineNumber, TestData.FileLine);

        Should.Throw<NotFoundException>(async () =>
                                        {
                                            await this.FileProcessorDomainService.ProcessTransactionForFileLine(processTransactionForFileLineRequest, CancellationToken.None);
                                        });
    }

    [Fact]
    public void FileRequestHandler_ProcessTransactionLineForFileRequest_LineInRequestAlreadyProcessed_RequestHandled()
    {
        this.FileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetFileAggregateWithLinesAlreadyProcessed);

        IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
        ConfigurationReader.Initialise(configurationRoot);
        Logger.Initialise(NullLogger.Instance);
            
        ProcessTransactionForFileLineRequest processTransactionForFileLineRequest1 =
            new ProcessTransactionForFileLineRequest(TestData.FileId, 1, TestData.FileLine1);
        ProcessTransactionForFileLineRequest processTransactionForFileLineRequest2 =
            new ProcessTransactionForFileLineRequest(TestData.FileId, 1, TestData.FileLine2);
        ProcessTransactionForFileLineRequest processTransactionForFileLineRequest3 =
            new ProcessTransactionForFileLineRequest(TestData.FileId, 3, TestData.FileLine3);
        ProcessTransactionForFileLineRequest processTransactionForFileLineRequest4 =
            new ProcessTransactionForFileLineRequest(TestData.FileId, 4, TestData.FileLine4);

        Should.NotThrow(async () =>
                        {
                            await this.FileProcessorDomainService.ProcessTransactionForFileLine(processTransactionForFileLineRequest1, CancellationToken.None);
                            await this.FileProcessorDomainService.ProcessTransactionForFileLine(processTransactionForFileLineRequest2, CancellationToken.None);
                            await this.FileProcessorDomainService.ProcessTransactionForFileLine(processTransactionForFileLineRequest3, CancellationToken.None);
                            await this.FileProcessorDomainService.ProcessTransactionForFileLine(processTransactionForFileLineRequest4, CancellationToken.None);
                        });
    }

    [Fact]
    public void FileRequestHandler_ProcessTransactionForFileLineRequest_FileProfileNotFound_RequestIsHandled()
    {
        this.FileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetFileAggregateWithLines);
            
        IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
        ConfigurationReader.Initialise(configurationRoot);
        Logger.Initialise(NullLogger.Instance);
            
        ProcessTransactionForFileLineRequest processTransactionForFileLineRequest =
            new ProcessTransactionForFileLineRequest(TestData.FileId, TestData.LineNumber, TestData.FileLine);

        Should.Throw<NotFoundException>(async () =>
                                        {
                                            await this.FileProcessorDomainService.ProcessTransactionForFileLine(processTransactionForFileLineRequest, CancellationToken.None);
                                        });
    }

    [Fact]
    public void FileRequestHandler_ProcessTransactionForFileLineRequest_FileLineIgnored_RequestIsHandled()
    {
        this.FileProcessorManager.Setup(f => f.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.FileProfileSafaricom);

        this.FileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetFileAggregateWithLines);

        this.TransactionProcessorClient.Setup(t => t.PerformTransaction(It.IsAny<String>(), It.IsAny<SerialisedMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.SerialisedMessageResponseSale);

        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantResponseWithOperator);

        this.EstateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantContractsResponse);

        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse());

        this.FileFormatHandler.Setup(f => f.FileLineCanBeIgnored(It.IsAny<String>())).Returns(true);
            
        IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
        ConfigurationReader.Initialise(configurationRoot);
        Logger.Initialise(NullLogger.Instance);
            
        ProcessTransactionForFileLineRequest processTransactionForFileLineRequest =
            new ProcessTransactionForFileLineRequest(TestData.FileId, TestData.LineNumber, TestData.FileLine);

        Should.NotThrow(async () =>
                        {
                            await this.FileProcessorDomainService.ProcessTransactionForFileLine(processTransactionForFileLineRequest, CancellationToken.None);
                        });
        this.FileFormatHandler.Verify(f => f.ParseFileLine(It.IsAny<String>()), Times.Never);
    }

    [Fact]
    public void FileRequestHandler_ProcessTransactionForFileLineRequest_EmptyFileLineIgnored_RequestIsHandled()
    {
        this.FileProcessorManager.Setup(f => f.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.FileProfileSafaricom);

        this.FileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetFileAggregateWithBlankLine);

        this.TransactionProcessorClient.Setup(t => t.PerformTransaction(It.IsAny<String>(), It.IsAny<SerialisedMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.SerialisedMessageResponseSale);

        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantResponseWithOperator);

        this.EstateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantContractsResponse);

        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse());

        IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
        ConfigurationReader.Initialise(configurationRoot);
        Logger.Initialise(NullLogger.Instance);
            
        ProcessTransactionForFileLineRequest processTransactionForFileLineRequest =
            new ProcessTransactionForFileLineRequest(TestData.FileId, TestData.LineNumber, TestData.FileLine);

        Should.NotThrow(async () =>
                        {
                            await this.FileProcessorDomainService.ProcessTransactionForFileLine(processTransactionForFileLineRequest, CancellationToken.None);
                        });
        this.FileFormatHandler.Verify(f => f.ParseFileLine(It.IsAny<String>()), Times.Never);
    }

    [Fact]
    public void FileRequestHandler_ProcessTransactionForFileLineRequest_FileParsingFailed_RequestIsHandled()
    {
        this.FileProcessorManager.Setup(f => f.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.FileProfileSafaricom);

        this.FileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetFileAggregateWithLines);

        this.TransactionProcessorClient.Setup(t => t.PerformTransaction(It.IsAny<String>(), It.IsAny<SerialisedMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.SerialisedMessageResponseSale);

        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantResponseWithOperator);

        this.EstateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantContractsResponse);

        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse());

        //Dictionary<String, String> transactionMetadata = null;
        this.FileFormatHandler.Setup(f => f.FileLineCanBeIgnored(It.IsAny<String>())).Returns(false);
        this.FileFormatHandler.Setup(f => f.ParseFileLine(It.IsAny<String>())).Throws<InvalidDataException>();

        IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
        ConfigurationReader.Initialise(configurationRoot);
        Logger.Initialise(NullLogger.Instance);

        ProcessTransactionForFileLineRequest processTransactionForFileLineRequest =
            new ProcessTransactionForFileLineRequest(TestData.FileId, TestData.LineNumber, TestData.FileLine);

        Should.NotThrow(async () =>
                        {
                            await this.FileProcessorDomainService.ProcessTransactionForFileLine(processTransactionForFileLineRequest, CancellationToken.None);
                        });
        this.EstateClient.Verify(f => f.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public void FileRequestHandler_ProcessTransactionForFileLineRequest_MerchantNotFound_RequestIsHandled()
    {
        this.FileProcessorManager.Setup(f => f.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.FileProfileSafaricom);

        this.FileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetFileAggregateWithLines);

        this.TransactionProcessorClient.Setup(t => t.PerformTransaction(It.IsAny<String>(), It.IsAny<SerialisedMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.SerialisedMessageResponseSale);

        this.EstateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantContractsResponse);

        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse());

        this.FileFormatHandler.Setup(f => f.FileLineCanBeIgnored(It.IsAny<String>())).Returns(false);
        this.FileFormatHandler.Setup(f => f.ParseFileLine(It.IsAny<String>())).Returns(TestData.TransactionMetadata);
            
        IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
        ConfigurationReader.Initialise(configurationRoot);
        Logger.Initialise(NullLogger.Instance);
            
        ProcessTransactionForFileLineRequest processTransactionForFileLineRequest =
            new ProcessTransactionForFileLineRequest(TestData.FileId, TestData.LineNumber, TestData.FileLine);

        Should.Throw<NotFoundException>(async () =>
                                        {
                                            await this.FileProcessorDomainService.ProcessTransactionForFileLine(processTransactionForFileLineRequest, CancellationToken.None);
                                        });
    }

    [Fact]
    public void FileRequestHandler_ProcessTransactionForFileLineRequest_NoMerchantContractsFound_RequestIsHandled()
    {
        this.FileProcessorManager.Setup(f => f.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.FileProfileSafaricom);

        this.FileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetFileAggregateWithLines);

        this.TransactionProcessorClient.Setup(t => t.PerformTransaction(It.IsAny<String>(), It.IsAny<SerialisedMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.SerialisedMessageResponseSale);

        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantResponseWithOperator);

        this.EstateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetEmptyMerchantContractsResponse);

        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse());

        this.FileFormatHandler.Setup(f => f.FileLineCanBeIgnored(It.IsAny<String>())).Returns(false);
        this.FileFormatHandler.Setup(f => f.ParseFileLine(It.IsAny<String>())).Returns(TestData.TransactionMetadata);
            
        IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
        ConfigurationReader.Initialise(configurationRoot);
        Logger.Initialise(NullLogger.Instance);
            
        ProcessTransactionForFileLineRequest processTransactionForFileLineRequest =
            new ProcessTransactionForFileLineRequest(TestData.FileId, TestData.LineNumber, TestData.FileLine);

        Should.Throw<NotFoundException>(async () =>
                                        {
                                            await this.FileProcessorDomainService.ProcessTransactionForFileLine(processTransactionForFileLineRequest, CancellationToken.None);
                                        });
    }

    [Fact]
    public void FileRequestHandler_ProcessTransactionForFileLineRequest_NoMerchantContractForFileOperatorFound_RequestIsHandled()
    {
        this.FileProcessorManager.Setup(f => f.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.FileProfileSafaricom);

        this.FileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetFileAggregateWithLines);

        this.TransactionProcessorClient.Setup(t => t.PerformTransaction(It.IsAny<String>(), It.IsAny<SerialisedMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.SerialisedMessageResponseSale);

        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantResponseWithOperator);

        this.EstateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantContractsNoFileOperatorResponse);

        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse());

        this.FileFormatHandler.Setup(f => f.FileLineCanBeIgnored(It.IsAny<String>())).Returns(false);
        this.FileFormatHandler.Setup(f => f.ParseFileLine(It.IsAny<String>())).Returns(TestData.TransactionMetadata);
            
        IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
        ConfigurationReader.Initialise(configurationRoot);
        Logger.Initialise(NullLogger.Instance);

        ProcessTransactionForFileLineRequest processTransactionForFileLineRequest =
            new ProcessTransactionForFileLineRequest(TestData.FileId, TestData.LineNumber, TestData.FileLine);

        Should.Throw<NotFoundException>(async () =>
                                        {
                                            await this.FileProcessorDomainService.ProcessTransactionForFileLine(processTransactionForFileLineRequest, CancellationToken.None);
                                        });
    }

    [Fact]
    public void FileRequestHandler_ProcessTransactionForFileLineRequest_MerchantContractProductNotFound_RequestIsHandled()
    {
        this.FileProcessorManager.Setup(f => f.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.FileProfileSafaricom);

        this.FileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetFileAggregateWithLines);

        this.TransactionProcessorClient.Setup(t => t.PerformTransaction(It.IsAny<String>(), It.IsAny<SerialisedMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.SerialisedMessageResponseSale);

        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantResponseWithOperator);

        this.EstateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantContractsResponseNoNullValueProduct);

        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse());

        this.FileFormatHandler.Setup(f => f.FileLineCanBeIgnored(It.IsAny<String>())).Returns(false);
        this.FileFormatHandler.Setup(f => f.ParseFileLine(It.IsAny<String>())).Returns(TestData.TransactionMetadata);
            
        IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
        ConfigurationReader.Initialise(configurationRoot);
        Logger.Initialise(NullLogger.Instance);
            
        ProcessTransactionForFileLineRequest processTransactionForFileLineRequest =
            new ProcessTransactionForFileLineRequest(TestData.FileId, TestData.LineNumber, TestData.FileLine);

        Should.Throw<NotFoundException>(async () =>
                                        {
                                            await this.FileProcessorDomainService.ProcessTransactionForFileLine(processTransactionForFileLineRequest, CancellationToken.None);
                                        });
    }

    [Fact]
    public void FileRequestHandler_ProcessTransactionForFileLineRequest_TransactionNotSuccessful_RequestIsHandled()
    {
        this.FileProcessorManager.Setup(f => f.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.FileProfileSafaricom);

        this.FileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetFileAggregateWithLines);

        this.TransactionProcessorClient.Setup(t => t.PerformTransaction(It.IsAny<String>(), It.IsAny<SerialisedMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.SerialisedMessageResponseFailedSale);

        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantResponseWithOperator);

        this.EstateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantContractsResponse);

        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.TokenResponse());

        this.FileFormatHandler.Setup(f => f.FileLineCanBeIgnored(It.IsAny<String>())).Returns(false);
        this.FileFormatHandler.Setup(f => f.ParseFileLine(It.IsAny<String>())).Returns(TestData.TransactionMetadata);
            
        IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
        ConfigurationReader.Initialise(configurationRoot);
        Logger.Initialise(NullLogger.Instance);

        ProcessTransactionForFileLineRequest processTransactionForFileLineRequest =
            new ProcessTransactionForFileLineRequest(TestData.FileId, TestData.LineNumber, TestData.FileLine);

        Should.NotThrow(async () =>
                        {
                            await this.FileProcessorDomainService.ProcessTransactionForFileLine(processTransactionForFileLineRequest, CancellationToken.None);
                        });
    }

    private void VerifyFileProcessing(String filePath)
    {
        IDirectoryInfo directoryInfo = this.FileSystem.DirectoryInfo.FromDirectoryName(filePath);
        directoryInfo.GetFiles("*.*").Length.ShouldBe(1);
    }
}