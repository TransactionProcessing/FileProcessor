using EstateManagement.DataTransferObjects.Responses.Operator;
using SimpleResults;

namespace FileProcessor.BusinessLogic.Tests;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Threading;
using System.Threading.Tasks;
using EstateManagement.Client;
using EstateManagement.DataTransferObjects.Responses.Contract;
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
        IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
        ConfigurationReader.Initialise(configurationRoot);

        Logger.Initialise(NullLogger.Instance);

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

        this.FileImportLogAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetEmptyFileImportLogAggregate()));
        this.FileImportLogAggregateRepository.Setup(f => f.SaveChanges(It.IsAny<FileImportLogAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success());

        this.FileSystem.AddFile(TestData.FilePathWithName, new MockFileData("D,1,1,1"));
        this.FileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom");
        
        Result<Guid> result = await this.FileProcessorDomainService.UploadFile(TestData.UploadFileCommand, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public async Task FileRequestHandler_UploadFileRequest_SaveFailed_RequestIsHandled()
    {
        this.FileProcessorManager.Setup(f => f.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.FileProfileSafaricom);

        this.FileImportLogAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetEmptyFileImportLogAggregate()));
        this.FileImportLogAggregateRepository.Setup(f => f.SaveChanges(It.IsAny<FileImportLogAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Failure);

        this.FileSystem.AddFile(TestData.FilePathWithName, new MockFileData("D,1,1,1"));
        this.FileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom");

        Result<Guid> result = await this.FileProcessorDomainService.UploadFile(TestData.UploadFileCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task FileRequestHandler_UploadFileRequest_ExceptionThrown_RequestIsHandled()
    {
        this.FileProcessorManager.Setup(f => f.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.FileProfileSafaricom);

        this.FileImportLogAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetEmptyFileImportLogAggregate()));
        this.FileImportLogAggregateRepository.Setup(f => f.SaveChanges(It.IsAny<FileImportLogAggregate>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception());

        this.FileSystem.AddFile(TestData.FilePathWithName, new MockFileData("D,1,1,1"));
        this.FileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom");

        Result<Guid> result = await this.FileProcessorDomainService.UploadFile(TestData.UploadFileCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task FileRequestHandler_UploadFileRequest_MerchantIdNotProvided_ErrorThrown() {
        FileCommands.UploadFileCommand command = TestData.UploadFileCommand with { MerchantId = Guid.Empty };
        Result<Guid> result = await this.FileProcessorDomainService.UploadFile(command, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.Invalid);
    }

    [Fact]
    public async Task FileRequestHandler_UploadFileRequest_FileProfileIdNotProvided_ErrorThrown()
    {
        FileCommands.UploadFileCommand command = TestData.UploadFileCommand with { FileProfileId = Guid.Empty };
        Result<Guid> result = await this.FileProcessorDomainService.UploadFile(command, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.Invalid);
    }

    [Fact]
    public async Task FileRequestHandler_UploadFileRequest_UserIdNotProvided_ErrorThrown()
    {
        FileCommands.UploadFileCommand command = TestData.UploadFileCommand with { UserId = Guid.Empty };
        Result<Guid> result = await this.FileProcessorDomainService.UploadFile(command, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.Invalid);
    }

    [Fact]
    public async Task FileRequestHandler_UploadFileRequest_ImportLogAlreadyCreated_RequestIsHandled()
    {
        this.FileProcessorManager.Setup(f => f.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.FileProfileSafaricom);

        this.FileImportLogAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetCreatedFileImportLogAggregate()));
        this.FileImportLogAggregateRepository.Setup(f => f.SaveChanges(It.IsAny<FileImportLogAggregate>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success());

        this.FileSystem.AddFile(TestData.FilePathWithName, new MockFileData("D,1,1,1"));
        this.FileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom");

        Result<Guid> result = await this.FileProcessorDomainService.UploadFile(TestData.UploadFileCommand, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
        result.Data.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public async Task FileRequestHandler_UploadFileRequest_NoFileProfiles_RequestIsHandled()
    {
        this.FileProcessorManager.Setup(f => f.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.FileProfileNull);

        this.FileImportLogAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetCreatedFileImportLogAggregate()));

        Result<Guid> result = await this.FileProcessorDomainService.UploadFile(TestData.UploadFileCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.NotFound);
    }

    [Fact]
    public async Task FileRequestHandler_UploadFileRequest_FileNotFound_RequestIsHandled()
    {
        this.FileProcessorManager.Setup(f => f.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.FileProfileSafaricom);

        this.FileImportLogAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetCreatedFileImportLogAggregate()));

        Result<Guid> result = await this.FileProcessorDomainService.UploadFile(TestData.UploadFileCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.NotFound);
    }

    [Fact]
    public async Task FileRequestHandler_UploadFileRequest_DestinationDirectoryNotFound_RequestIsHandled()
    {
        this.FileProcessorManager.Setup(f => f.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.FileProfileSafaricom);

        this.FileImportLogAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetCreatedFileImportLogAggregate()));

        this.FileSystem.AddFile(TestData.FilePathWithName, new MockFileData("D,1,1,1"));

        Logger.Initialise(NullLogger.Instance);

        Result<Guid> result = await this.FileProcessorDomainService.UploadFile(TestData.UploadFileCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.NotFound);
    }
    
    [Fact]
    public async Task FileRequestHandler_ProcessUploadedFileRequest_RequestIsHandled()
    {
        this.FileProcessorManager.Setup(f => f.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.FileProfileSafaricom);
        this.FileAggregateRepository.SetupSequence(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetEmptyFileAggregate()))
            .ReturnsAsync(Result.Success(TestData.GetCreatedFileAggregate()));
        this.FileAggregateRepository.Setup(f => f.SaveChanges(It.IsAny<FileAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        this.FileSystem.AddFile(TestData.FilePathWithName, new MockFileData("D,1,1,1"));

        this.FileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom/inprogress");
        this.FileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom/processed");
        this.FileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom/failed");

        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.EstateClient.Setup(e => e.GetOperators(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.OperatorList);
        
        Result result = await this.FileProcessorDomainService.ProcessUploadedFile(TestData.ProcessUploadedFileCommand, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }
    
    [Fact]
    public async Task FileRequestHandler_ProcessUploadedFileRequest_FileNotFound_RequestIsHandled()
    {
        this.FileProcessorManager.Setup(f => f.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.FileProfileSafaricom);

        this.FileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetCreatedFileAggregate()));
        this.FileAggregateRepository.Setup(f => f.SaveChanges(It.IsAny<FileAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        this.FileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom/inprogress");
        this.FileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom/processed");
        this.FileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom/failed");

        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.EstateClient.Setup(e => e.GetOperators(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.OperatorList);

        Result result = await this.FileProcessorDomainService.ProcessUploadedFile(TestData.ProcessUploadedFileCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.NotFound);
    }
    
    [Fact]
    public async Task FileRequestHandler_ProcessUploadedFileRequest_NoFileProfiles_RequestIsHandled()
    {
        this.FileProcessorManager.Setup(f => f.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.NotFound());

        this.FileImportLogAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.GetEmptyFileImportLogAggregate()));
        this.FileAggregateRepository.Setup(f => f.SaveChanges(It.IsAny<FileAggregate>(), It.IsAny<CancellationToken>()))
       .ReturnsAsync(Result.Success);
        this.FileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetCreatedFileAggregate()));

        this.FileSystem.AddFile(TestData.FilePathWithName, new MockFileData("D,1,1,1"));

        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.EstateClient.Setup(e => e.GetOperators(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.OperatorList);

        Result result = await this.FileProcessorDomainService.ProcessUploadedFile(TestData.ProcessUploadedFileCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.NotFound);
    }

    [Fact]
    public async Task FileRequestHandler_ProcessUploadedFileRequest_GetFileProfileFailed_RequestIsHandled()
    {
        this.FileProcessorManager.SetupSequence(f => f.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.FileProfileSafaricom))
            .ReturnsAsync(Result.Failure());

        this.FileImportLogAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.GetEmptyFileImportLogAggregate()));
        this.FileAggregateRepository.Setup(f => f.SaveChanges(It.IsAny<FileAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);
        this.FileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetCreatedFileAggregate()));

        this.FileSystem.AddFile(TestData.FilePathWithName, new MockFileData("D,1,1,1"));

        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.EstateClient.Setup(e => e.GetOperators(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.OperatorList);

        Result result = await this.FileProcessorDomainService.ProcessUploadedFile(TestData.ProcessUploadedFileCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.Failure);
    }


    [Fact]
    public async Task FileRequestHandler_ProcessUploadedFileRequest_NoOperatorsFound_RequestIsHandled()
    {
        this.FileProcessorManager.Setup(f => f.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.FileProfileSafaricom);

        this.FileImportLogAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.GetEmptyFileImportLogAggregate()));

        this.FileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetCreatedFileAggregate()));
        this.FileAggregateRepository.Setup(f => f.SaveChanges(It.IsAny<FileAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);
        this.FileSystem.AddFile(TestData.FilePathWithName, new MockFileData("D,1,1,1"));

        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.EstateClient.Setup(e => e.GetOperators(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.NotFound());

        Result result = await this.FileProcessorDomainService.ProcessUploadedFile(TestData.ProcessUploadedFileCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.NotFound);
    }

    [Fact]
    public async Task FileRequestHandler_ProcessUploadedFileRequest_ProfileOperatorNotFound_RequestIsHandled()
    {
        this.FileProcessorManager.Setup(f => f.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.FileProfileSafaricom);

        this.FileImportLogAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.GetEmptyFileImportLogAggregate()));

        this.FileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetCreatedFileAggregate()));
        this.FileAggregateRepository.Setup(f => f.SaveChanges(It.IsAny<FileAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);
        this.FileSystem.AddFile(TestData.FilePathWithName, new MockFileData("D,1,1,1"));

        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.EstateClient.Setup(e => e.GetOperators(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(new List<OperatorResponse>()));

        Result result = await this.FileProcessorDomainService.ProcessUploadedFile(TestData.ProcessUploadedFileCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.NotFound);
    }

    [Fact]
    public async Task FileRequestHandler_ProcessUploadedFileRequest_NullOperatorList_RequestIsHandled()
    {
        this.FileProcessorManager.Setup(f => f.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.FileProfileSafaricom);

        this.FileImportLogAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.GetEmptyFileImportLogAggregate()));

        this.FileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetCreatedFileAggregate()));

        this.FileSystem.AddFile(TestData.FilePathWithName, new MockFileData("D,1,1,1"));

        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.EstateClient.Setup(e => e.GetOperators(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.NotFound());

        Result result = await this.FileProcessorDomainService.ProcessUploadedFile(TestData.ProcessUploadedFileCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.NotFound);
    }
    
    [Fact]
    public async Task FileRequestHandler_ProcessUploadedFileRequest_ProcessedDirectoryNotFound_RequestIsHandled()
    {
        this.FileProcessorManager.Setup(f => f.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.FileProfileSafaricom);

        this.FileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetCreatedFileAggregate()));
        this.FileAggregateRepository.Setup(f => f.SaveChanges(It.IsAny<FileAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        this.FileSystem.AddFile(TestData.FilePathWithName, new MockFileData("D,1,1,1"));

        this.FileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom/inprogress");
        this.FileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom/failed");

        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.EstateClient.Setup(e => e.GetOperators(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.OperatorList);
        Result result = await this.FileProcessorDomainService.ProcessUploadedFile(TestData.ProcessUploadedFileCommand, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();

        this.VerifyFileProcessing("home/txnproc/bulkfiles/safaricom/processed");
    }

    [Fact]
    public async Task FileRequestHandler_ProcessUploadedFileRequest_FailedDirectoryNotFound_RequestIsHandled()
    {
        this.FileProcessorManager.Setup(f => f.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.FileProfileSafaricom);

        this.FileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetCreatedFileAggregate()));
        this.FileAggregateRepository.Setup(f => f.SaveChanges(It.IsAny<FileAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        this.FileSystem.AddFile(TestData.FilePathWithName, new MockFileData("D,1,1,1"));

        this.FileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom/inprogress");
        this.FileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom/processed");

        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.EstateClient.Setup(e => e.GetOperators(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.OperatorList);

        Result result = await this.FileProcessorDomainService.ProcessUploadedFile(TestData.ProcessUploadedFileCommand, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
        this.VerifyFileProcessing("home/txnproc/bulkfiles/safaricom/processed");
    }

    
    [Fact]
    public async Task FileRequestHandler_ProcessUploadedFileRequest_FileIsEmpty_RequestIsHandled()
    {
        this.FileProcessorManager.Setup(f => f.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.FileProfileSafaricom);

        this.FileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetCreatedFileAggregate()));
        this.FileAggregateRepository.Setup(f => f.SaveChanges(It.IsAny<FileAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        this.FileSystem.AddFile(TestData.FilePathWithName, new MockFileData(String.Empty));

        this.FileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom/inprogress");
        this.FileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom/processed");
        this.FileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom/failed");

        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.EstateClient.Setup(e => e.GetOperators(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.OperatorList);

        Result result = await this.FileProcessorDomainService.ProcessUploadedFile(TestData.ProcessUploadedFileCommand, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();

        this.FileAggregateRepository.Verify(f => f.SaveChanges(It.IsAny<FileAggregate>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        this.VerifyFileProcessing("home/txnproc/bulkfiles/safaricom/processed");
    }

    [Fact]
    public async Task FileRequestHandler_ProcessUploadedFileRequest_FileIsInFailedFolder_RequestIsHandled()
    {
        this.FileProcessorManager.Setup(f => f.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.FileProfileSafaricom);

        this.FileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetCreatedFileAggregate()));
        this.FileAggregateRepository.Setup(f => f.SaveChanges(It.IsAny<FileAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        this.FileSystem.AddFile(TestData.FailedSafaricomFilePathWithName, new MockFileData(String.Empty));

        this.FileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom/inprogress");
        this.FileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom/processed");
        this.FileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom/failed");

        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.EstateClient.Setup(e => e.GetOperators(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.OperatorList);

        Result result = await this.FileProcessorDomainService.ProcessUploadedFile(TestData.ProcessUploadedFileCommand, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();

        this.FileAggregateRepository.Verify(f => f.SaveChanges(It.IsAny<FileAggregate>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        this.VerifyFileProcessing("home/txnproc/bulkfiles/safaricom/processed");
    }


    [Theory]
    [InlineData("Safaricom")]
    [InlineData("Voucher")]
    public async Task FileRequestHandler_ProcessTransactionForFileLineRequest_RequestIsHandled(String operatorName)
    {
        this.FileProcessorManager.Setup(f => f.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetFileProfile(operatorName));

        this.FileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetFileAggregateWithLines()));
        this.FileAggregateRepository.Setup(f => f.SaveChanges(It.IsAny<FileAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        this.TransactionProcessorClient.Setup(t => t.PerformTransaction(It.IsAny<String>(), It.IsAny<SerialisedMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.SerialisedMessageResponseSale);
            
        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantResponseWithOperator);

        this.EstateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.GetMerchantContractsResponse()));
            
        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.FileFormatHandler.Setup(f => f.FileLineCanBeIgnored(It.IsAny<String>())).Returns(false);
        this.FileFormatHandler.Setup(f => f.ParseFileLine(It.IsAny<String>())).Returns(TestData.TransactionMetadata);
           
        Result result = await this.FileProcessorDomainService.ProcessTransactionForFileLine(TestData.ProcessTransactionForFileLineCommand, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Theory]
    [InlineData("Safaricom")]
    [InlineData("Voucher")]
    public async Task FileRequestHandler_ProcessTransactionForFileLineRequest_SaveFailed_RequestIsHandled(String operatorName)
    {
        this.FileProcessorManager.Setup(f => f.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetFileProfile(operatorName));

        this.FileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetFileAggregateWithLines()));
        this.FileAggregateRepository.Setup(f => f.SaveChanges(It.IsAny<FileAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure);

        this.TransactionProcessorClient.Setup(t => t.PerformTransaction(It.IsAny<String>(), It.IsAny<SerialisedMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.SerialisedMessageResponseSale);

        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantResponseWithOperator);

        this.EstateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.GetMerchantContractsResponse()));

        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.FileFormatHandler.Setup(f => f.FileLineCanBeIgnored(It.IsAny<String>())).Returns(false);
        this.FileFormatHandler.Setup(f => f.ParseFileLine(It.IsAny<String>())).Returns(TestData.TransactionMetadata);

        Result result = await this.FileProcessorDomainService.ProcessTransactionForFileLine(TestData.ProcessTransactionForFileLineCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Theory]
    [InlineData("Safaricom")]
    [InlineData("Voucher")]
    public async Task FileRequestHandler_ProcessTransactionForFileLineRequest_ExceptionThrown_RequestIsHandled(String operatorName)
    {
        this.FileProcessorManager.Setup(f => f.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.GetFileProfile(operatorName));

        this.FileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetFileAggregateWithLines()));
        this.FileAggregateRepository.Setup(f => f.SaveChanges(It.IsAny<FileAggregate>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception());

        this.TransactionProcessorClient.Setup(t => t.PerformTransaction(It.IsAny<String>(), It.IsAny<SerialisedMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.SerialisedMessageResponseSale);

        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantResponseWithOperator);

        this.EstateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.GetMerchantContractsResponse()));

        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.FileFormatHandler.Setup(f => f.FileLineCanBeIgnored(It.IsAny<String>())).Returns(false);
        this.FileFormatHandler.Setup(f => f.ParseFileLine(It.IsAny<String>())).Returns(TestData.TransactionMetadata);

        Result result = await this.FileProcessorDomainService.ProcessTransactionForFileLine(TestData.ProcessTransactionForFileLineCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }


    [Fact]
    public async Task FileRequestHandler_ProcessTransactionForFileLineRequest_WithOperatorName_RequestIsHandled()
    {
        this.FileProcessorManager.Setup(f => f.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.FileProfileVoucher);

        this.FileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetFileAggregateWithLines()));
        this.FileAggregateRepository.Setup(f => f.SaveChanges(It.IsAny<FileAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);

        this.TransactionProcessorClient.Setup(t => t.PerformTransaction(It.IsAny<String>(), It.IsAny<SerialisedMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.SerialisedMessageResponseSale);

        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantResponseWithOperator);

        this.EstateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.GetMerchantContractsResponse()));

        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.FileFormatHandler.Setup(f => f.FileLineCanBeIgnored(It.IsAny<String>())).Returns(false);
        this.FileFormatHandler.Setup(f => f.ParseFileLine(It.IsAny<String>())).Returns(TestData.TransactionMetadataWithOperatorName);

        Result result = await this.FileProcessorDomainService.ProcessTransactionForFileLine(TestData.ProcessTransactionForFileLineCommand, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    
    [Fact]
    public async Task FileRequestHandler_ProcessTransactionLineForFileRequest_FileAggregateNotFound_RequestHandled()
    {
        this.FileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.NotFound());
           
        var result = await this.FileProcessorDomainService.ProcessTransactionForFileLine(TestData.ProcessTransactionForFileLineCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.NotFound);
    }
    
    [Fact]
    public async Task  FileRequestHandler_ProcessTransactionLineForFileRequest_FileAggregateWithNoLines_RequestHandled()
    {
        this.FileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetCreatedFileAggregate()));

        var result = await this.FileProcessorDomainService.ProcessTransactionForFileLine(TestData.ProcessTransactionForFileLineCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.Invalid);
    }
    
    [Fact]
    public async Task FileRequestHandler_ProcessTransactionLineForFileRequest_LineInRequestNotFoundInFileAggregate_RequestHandled()
    {
        this.FileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetFileAggregateWithLines()));
            
        IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
        ConfigurationReader.Initialise(configurationRoot);
        Logger.Initialise(NullLogger.Instance);

        var command = TestData.ProcessTransactionForFileLineCommand with { LineNumber = TestData.NotFoundLineNumber };
        var result = await this.FileProcessorDomainService.ProcessTransactionForFileLine(command, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.NotFound);
    }
    
    [Fact]
    public async Task FileRequestHandler_ProcessTransactionLineForFileRequest_LineInRequestAlreadyProcessed_RequestHandled()
    {
        this.FileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetFileAggregateWithLinesAlreadyProcessed()));
        this.FileAggregateRepository.Setup(f => f.SaveChanges(It.IsAny<FileAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);
        FileCommands.ProcessTransactionForFileLineCommand processTransactionForFileLineRequest1 =
            new (TestData.FileId, 1, TestData.FileLine1);
        FileCommands.ProcessTransactionForFileLineCommand processTransactionForFileLineRequest2 =
            new (TestData.FileId, 1, TestData.FileLine2);
        FileCommands.ProcessTransactionForFileLineCommand processTransactionForFileLineRequest3 =
            new (TestData.FileId, 3, TestData.FileLine3);
        FileCommands.ProcessTransactionForFileLineCommand processTransactionForFileLineRequest4 =
            new (TestData.FileId, 4, TestData.FileLine4);

        var result1 = await this.FileProcessorDomainService.ProcessTransactionForFileLine(processTransactionForFileLineRequest1, CancellationToken.None);
        var result2 = await this.FileProcessorDomainService.ProcessTransactionForFileLine(processTransactionForFileLineRequest2, CancellationToken.None);
        var result3 = await this.FileProcessorDomainService.ProcessTransactionForFileLine(processTransactionForFileLineRequest3, CancellationToken.None);
        var result4 =
            await this.FileProcessorDomainService.ProcessTransactionForFileLine(processTransactionForFileLineRequest4,
                CancellationToken.None);
        result1.IsSuccess.ShouldBeTrue();
        result2.IsSuccess.ShouldBeTrue();
        result3.IsSuccess.ShouldBeTrue();
        result4.IsSuccess.ShouldBeTrue();
    }
    
    [Fact]
    public async Task FileRequestHandler_ProcessTransactionForFileLineRequest_FileProfileNotFound_RequestIsHandled()
    {
        this.FileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetFileAggregateWithLines()));
        this.FileProcessorManager.Setup(fpm => fpm.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.NotFound());
        var result = await this.FileProcessorDomainService.ProcessTransactionForFileLine(
                TestData.ProcessTransactionForFileLineCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.NotFound);
    }
    
    [Fact]
    public async Task FileRequestHandler_ProcessTransactionForFileLineRequest_FileLineIgnored_RequestIsHandled()
    {
        this.FileProcessorManager.Setup(f => f.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.FileProfileSafaricom);

        this.FileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetFileAggregateWithLines()));
        this.FileAggregateRepository.Setup(f => f.SaveChanges(It.IsAny<FileAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);
        this.TransactionProcessorClient.Setup(t => t.PerformTransaction(It.IsAny<String>(), It.IsAny<SerialisedMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.SerialisedMessageResponseSale);

        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantResponseWithOperator);

        this.EstateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.GetMerchantContractsResponse()));

        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.FileFormatHandler.Setup(f => f.FileLineCanBeIgnored(It.IsAny<String>())).Returns(true);
            
        Result result = await this.FileProcessorDomainService.ProcessTransactionForFileLine(TestData.ProcessTransactionForFileLineCommand, CancellationToken.None);
                        result.IsSuccess.ShouldBeTrue();
        this.FileFormatHandler.Verify(f => f.ParseFileLine(It.IsAny<String>()), Times.Never);
    }
    
    [Fact]
    public async Task FileRequestHandler_ProcessTransactionForFileLineRequest_EmptyFileLineIgnored_RequestIsHandled()
    {
        this.FileProcessorManager.Setup(f => f.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.FileProfileSafaricom);

        this.FileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetFileAggregateWithBlankLine()));
        
        this.TransactionProcessorClient.Setup(t => t.PerformTransaction(It.IsAny<String>(), It.IsAny<SerialisedMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.SerialisedMessageResponseSale);

        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantResponseWithOperator);

        this.EstateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success( TestData.GetMerchantContractsResponse()));

        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));
var result =                             await this.FileProcessorDomainService.ProcessTransactionForFileLine(TestData.ProcessTransactionForFileLineCommand, CancellationToken.None);
        this.FileFormatHandler.Verify(f => f.ParseFileLine(It.IsAny<String>()), Times.Never);
    }

    [Fact]
    public async Task FileRequestHandler_ProcessTransactionForFileLineRequest_FileParsingFailed_RequestIsHandled()
    {
        this.FileProcessorManager.Setup(f => f.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.FileProfileSafaricom);

        this.FileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetFileAggregateWithLines()));
        this.FileAggregateRepository.Setup(f => f.SaveChanges(It.IsAny<FileAggregate>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success);
        this.TransactionProcessorClient.Setup(t => t.PerformTransaction(It.IsAny<String>(), It.IsAny<SerialisedMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.SerialisedMessageResponseSale);

        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantResponseWithOperator);

        this.EstateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.GetMerchantContractsResponse()));

        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));
        
        this.FileFormatHandler.Setup(f => f.FileLineCanBeIgnored(It.IsAny<String>())).Returns(false);
        this.FileFormatHandler.Setup(f => f.ParseFileLine(It.IsAny<String>())).Throws<InvalidDataException>();

        var result = await this.FileProcessorDomainService.ProcessTransactionForFileLine(TestData.ProcessTransactionForFileLineCommand, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
        this.EstateClient.Verify(f => f.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task FileRequestHandler_ProcessTransactionForFileLineRequest_MerchantNotFound_RequestIsHandled()
    {
        this.FileProcessorManager.Setup(f => f.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.FileProfileSafaricom);

        this.FileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetFileAggregateWithLines()));

        this.TransactionProcessorClient.Setup(t => t.PerformTransaction(It.IsAny<String>(), It.IsAny<SerialisedMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.SerialisedMessageResponseSale);

        this.EstateClient
            .Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(),
                It.IsAny<CancellationToken>())).ReturnsAsync(Result.NotFound());
        this.EstateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.GetMerchantContractsResponse()));

        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.FileFormatHandler.Setup(f => f.FileLineCanBeIgnored(It.IsAny<String>())).Returns(false);
        this.FileFormatHandler.Setup(f => f.ParseFileLine(It.IsAny<String>())).Returns(TestData.TransactionMetadata);
        
        var result =await this.FileProcessorDomainService.ProcessTransactionForFileLine(TestData.ProcessTransactionForFileLineCommand, CancellationToken.None);
                                        result.IsFailed.ShouldBeTrue();
                                        result.Status.ShouldBe(ResultStatus.NotFound);
    }

    [Fact]
    public async Task FileRequestHandler_ProcessTransactionForFileLineRequest_NoMerchantContractsFound_RequestIsHandled()
    {
        this.FileProcessorManager.Setup(f => f.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.FileProfileSafaricom);

        this.FileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetFileAggregateWithLines()));

        this.TransactionProcessorClient.Setup(t => t.PerformTransaction(It.IsAny<String>(), It.IsAny<SerialisedMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.SerialisedMessageResponseSale);

        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantResponseWithOperator);

        this.EstateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.NotFound());

        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.FileFormatHandler.Setup(f => f.FileLineCanBeIgnored(It.IsAny<String>())).Returns(false);
        this.FileFormatHandler.Setup(f => f.ParseFileLine(It.IsAny<String>())).Returns(TestData.TransactionMetadata);

        var result = await this.FileProcessorDomainService.ProcessTransactionForFileLine(TestData.ProcessTransactionForFileLineCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.NotFound);
    }

    [Fact]
    public async Task FileRequestHandler_ProcessTransactionForFileLineRequest_EmptyMerchantContractsArray_RequestIsHandled()
    {
        this.FileProcessorManager.Setup(f => f.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.FileProfileSafaricom);

        this.FileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetFileAggregateWithLines()));

        this.TransactionProcessorClient.Setup(t => t.PerformTransaction(It.IsAny<String>(), It.IsAny<SerialisedMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.SerialisedMessageResponseSale);

        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantResponseWithOperator);

        this.EstateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new List<ContractResponse>()));

        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.FileFormatHandler.Setup(f => f.FileLineCanBeIgnored(It.IsAny<String>())).Returns(false);
        this.FileFormatHandler.Setup(f => f.ParseFileLine(It.IsAny<String>())).Returns(TestData.TransactionMetadata);

        var result = await this.FileProcessorDomainService.ProcessTransactionForFileLine(TestData.ProcessTransactionForFileLineCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.NotFound);
    }

    [Fact]
    public async Task FileRequestHandler_ProcessTransactionForFileLineRequest_ContractNotFoundInMerchantContracts_RequestIsHandled()
    {
        this.FileProcessorManager.Setup(f => f.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.FileProfileSafaricom);

        this.FileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetFileAggregateWithLines()));

        this.TransactionProcessorClient.Setup(t => t.PerformTransaction(It.IsAny<String>(), It.IsAny<SerialisedMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.SerialisedMessageResponseSale);

        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantResponseWithOperator);

        this.EstateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new List<ContractResponse> {
                new ContractResponse {
                    OperatorName = "Other Operator"
                }
            }));

        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.FileFormatHandler.Setup(f => f.FileLineCanBeIgnored(It.IsAny<String>())).Returns(false);
        this.FileFormatHandler.Setup(f => f.ParseFileLine(It.IsAny<String>())).Returns(TestData.TransactionMetadata);

        var result = await this.FileProcessorDomainService.ProcessTransactionForFileLine(TestData.ProcessTransactionForFileLineCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.NotFound);
    }

    [Fact]
    public async Task FileRequestHandler_ProcessTransactionForFileLineRequest_NoMerchantContractForFileOperatorFound_RequestIsHandled()
    {
        this.FileProcessorManager.Setup(f => f.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.FileProfileSafaricom);

        this.FileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetFileAggregateWithLines()));

        this.TransactionProcessorClient.Setup(t => t.PerformTransaction(It.IsAny<String>(), It.IsAny<SerialisedMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.SerialisedMessageResponseSale);

        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantResponseWithOperator);

        this.EstateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.NotFound());

        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.FileFormatHandler.Setup(f => f.FileLineCanBeIgnored(It.IsAny<String>())).Returns(false);
        this.FileFormatHandler.Setup(f => f.ParseFileLine(It.IsAny<String>())).Returns(TestData.TransactionMetadata);

        var result = await this.FileProcessorDomainService.ProcessTransactionForFileLine(TestData.ProcessTransactionForFileLineCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.NotFound);
    }

    [Fact]
    public async Task FileRequestHandler_ProcessTransactionForFileLineRequest_MerchantContractProductNotFound_RequestIsHandled()
    {
        this.FileProcessorManager.Setup(f => f.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.FileProfileSafaricom);

        this.FileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetFileAggregateWithLines()));

        this.TransactionProcessorClient.Setup(t => t.PerformTransaction(It.IsAny<String>(), It.IsAny<SerialisedMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.SerialisedMessageResponseSale);

        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantResponseWithOperator);

        this.EstateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.GetMerchantContractsResponseNoNullValueProduct()));

        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.FileFormatHandler.Setup(f => f.FileLineCanBeIgnored(It.IsAny<String>())).Returns(false);
        this.FileFormatHandler.Setup(f => f.ParseFileLine(It.IsAny<String>())).Returns(TestData.TransactionMetadata);

        var result = await this.FileProcessorDomainService.ProcessTransactionForFileLine(TestData.ProcessTransactionForFileLineCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.NotFound);
    }

    [Fact]
    public async Task FileRequestHandler_ProcessTransactionForFileLineRequest_TransactionNotSuccessfulResult_RequestIsHandled()
    {
        this.FileProcessorManager.Setup(f => f.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.FileProfileSafaricom);

        this.FileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetFileAggregateWithLines()));

        this.TransactionProcessorClient.Setup(t => t.PerformTransaction(It.IsAny<String>(), It.IsAny<SerialisedMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure());

        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantResponseWithOperator);

        this.EstateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.GetMerchantContractsResponse()));

        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.FileFormatHandler.Setup(f => f.FileLineCanBeIgnored(It.IsAny<String>())).Returns(false);
        this.FileFormatHandler.Setup(f => f.ParseFileLine(It.IsAny<String>())).Returns(TestData.TransactionMetadata);

        var result = await this.FileProcessorDomainService.ProcessTransactionForFileLine(TestData.ProcessTransactionForFileLineCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.Failure);
    }

    [Fact]
    public async Task FileRequestHandler_ProcessTransactionForFileLineRequest_TransactionNotSuccessful_RequestIsHandled()
    {
        this.FileProcessorManager.Setup(f => f.GetFileProfile(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestData.FileProfileSafaricom);

        this.FileAggregateRepository.Setup(f => f.GetLatestVersion(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.GetFileAggregateWithLines()));

        this.TransactionProcessorClient.Setup(t => t.PerformTransaction(It.IsAny<String>(), It.IsAny<SerialisedMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.SerialisedMessageResponseFailedSale);

        this.EstateClient.Setup(e => e.GetMerchant(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.GetMerchantResponseWithOperator);

        this.EstateClient.Setup(e => e.GetMerchantContracts(It.IsAny<String>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.GetMerchantContractsResponse()));

        this.SecurityServiceClient.Setup(s => s.GetToken(It.IsAny<String>(), It.IsAny<String>(), It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.FileFormatHandler.Setup(f => f.FileLineCanBeIgnored(It.IsAny<String>())).Returns(false);
        this.FileFormatHandler.Setup(f => f.ParseFileLine(It.IsAny<String>())).Returns(TestData.TransactionMetadata);

        var result = await this.FileProcessorDomainService.ProcessTransactionForFileLine(TestData.ProcessTransactionForFileLineCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.Failure);
    }
    

    private void VerifyFileProcessing(String filePath)
    {
        IDirectoryInfo directoryInfo = this.FileSystem.DirectoryInfo.New(filePath);
        directoryInfo.GetFiles("*.*").Length.ShouldBe(1);
    }
}