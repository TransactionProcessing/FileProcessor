using System.Text;
using SimpleResults;
using TransactionProcessor.DataTransferObjects.Responses.Contract;
using TransactionProcessor.DataTransferObjects.Responses.Operator;

namespace FileProcessor.BusinessLogic.Tests;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Threading;
using System.Threading.Tasks;
using FileAggregate;
using FileFormatHandlers;
using FileImportLogAggregate;
using Managers;
using Microsoft.Extensions.Configuration;
using Imposter.Abstractions;
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
    private IFileProcessorManagerImposter FileProcessorManager;

    private IAggregateRepositoryImposter<FileImportLogAggregate, DomainEvent> FileImportLogAggregateRepository;

    private IAggregateRepositoryImposter<FileAggregate, DomainEvent> FileAggregateRepository;

    private ITransactionProcessorClientImposter TransactionProcessorClient;
    
    private ISecurityServiceClientImposter SecurityServiceClient;

    private IFileFormatHandlerImposter FileFormatHandler;

    private FileProcessorDomainService FileProcessorDomainService;

    private MockFileSystem FileSystem;
    public FileProcessorDomainServiceTests()
    {
        IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
        ConfigurationReader.Initialise(configurationRoot);

        Logger.Initialise(NullLogger.Instance);

        this.FileProcessorManager = new IFileProcessorManagerImposter();
        this.FileImportLogAggregateRepository =
            new IAggregateRepositoryImposter<FileImportLogAggregate, DomainEvent>();
        this.FileAggregateRepository =
            new IAggregateRepositoryImposter<FileAggregate, DomainEvent>();
        this.TransactionProcessorClient = new ITransactionProcessorClientImposter();
        this.SecurityServiceClient = new ISecurityServiceClientImposter();
        this.FileFormatHandler = new IFileFormatHandlerImposter();
        this.FileSystem = new MockFileSystem();

        Func<String, IFileFormatHandler> fileFormatHandlerResolver = (format) =>
                                                                     {
                                                                         return this.FileFormatHandler.Instance();
                                                                     };

        this.FileProcessorDomainService = new FileProcessorDomainService(this.FileProcessorManager.Instance(),
                                                                         this.FileImportLogAggregateRepository.Instance(),
                                                                         this.FileAggregateRepository.Instance(),
                                                                         this.TransactionProcessorClient.Instance(),
                                                                         this.SecurityServiceClient.Instance(),
                                                                         fileFormatHandlerResolver,
                                                                         this.FileSystem);
        Logger.Initialise(NullLogger.Instance);
    }

    [Fact]
    public async Task FileRequestHandler_UploadFileRequest_RequestIsHandled()
    {
        this.FileProcessorManager.GetFileProfile(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(TestData.FileProfileSafaricom);

        this.FileImportLogAggregateRepository.GetLatestVersion(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetEmptyFileImportLogAggregate()));
        this.FileImportLogAggregateRepository.SaveChanges(Arg<FileImportLogAggregate>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success());

        var m = new MockFileData("D,1,1,1");
        var fileId = FileProcessorDomainService.CreateGuidFromFileData(Encoding.UTF8.GetString(m.Contents));
        this.FileSystem.AddFile(TestData.FilePathWithName, m);
        this.FileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom");
        
        var command = TestData.UploadFileCommand with { FileId = fileId };

        Result result = await this.FileProcessorDomainService.UploadFile(command, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task FileRequestHandler_UploadFileRequest_SaveFailed_RequestIsHandled()
    {
        this.FileProcessorManager.GetFileProfile(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(TestData.FileProfileSafaricom);

        this.FileImportLogAggregateRepository.GetLatestVersion(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetEmptyFileImportLogAggregate()));
        this.FileImportLogAggregateRepository.SaveChanges(Arg<FileImportLogAggregate>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Failure());

        this.FileSystem.AddFile(TestData.FilePathWithName, new MockFileData("D,1,1,1"));
        this.FileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom");

        Result<Guid> result = await this.FileProcessorDomainService.UploadFile(TestData.UploadFileCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task FileRequestHandler_UploadFileRequest_ExceptionThrown_RequestIsHandled()
    {
        this.FileProcessorManager.GetFileProfile(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(TestData.FileProfileSafaricom);

        this.FileImportLogAggregateRepository.GetLatestVersion(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetEmptyFileImportLogAggregate()));
        this.FileImportLogAggregateRepository.SaveChanges(Arg<FileImportLogAggregate>.Any(), Arg<CancellationToken>.Any()).ThrowsAsync(new Exception());

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
        this.FileProcessorManager.GetFileProfile(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(TestData.FileProfileSafaricom);

        this.FileImportLogAggregateRepository.GetLatestVersion(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetCreatedFileImportLogAggregate()));
        this.FileImportLogAggregateRepository.SaveChanges(Arg<FileImportLogAggregate>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success());

        var m = new MockFileData("D,1,1,1");
        var fileId = FileProcessorDomainService.CreateGuidFromFileData(Encoding.UTF8.GetString(m.Contents));
        this.FileSystem.AddFile(TestData.FilePathWithName, m);
        this.FileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom");

        var command = TestData.UploadFileCommand with { FileId = fileId };

        Result result = await this.FileProcessorDomainService.UploadFile(command, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task FileRequestHandler_UploadFileRequest_NoFileProfiles_RequestIsHandled()
    {
        this.FileProcessorManager.GetFileProfile(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(TestData.FileProfileNull);

        this.FileImportLogAggregateRepository.GetLatestVersion(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetCreatedFileImportLogAggregate()));

        Result<Guid> result = await this.FileProcessorDomainService.UploadFile(TestData.UploadFileCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.NotFound);
    }

    [Fact]
    public async Task FileRequestHandler_UploadFileRequest_FileNotFound_RequestIsHandled()
    {
        this.FileProcessorManager.GetFileProfile(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(TestData.FileProfileSafaricom);

        this.FileImportLogAggregateRepository.GetLatestVersion(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetCreatedFileImportLogAggregate()));

        Result<Guid> result = await this.FileProcessorDomainService.UploadFile(TestData.UploadFileCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.NotFound);
    }

    [Fact]
    public async Task FileRequestHandler_UploadFileRequest_DestinationDirectoryNotFound_RequestIsHandled()
    {
        this.FileProcessorManager.GetFileProfile(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(TestData.FileProfileSafaricom);

        this.FileImportLogAggregateRepository.GetLatestVersion(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetCreatedFileImportLogAggregate()));

        this.FileSystem.AddFile(TestData.FilePathWithName, new MockFileData("D,1,1,1"));

        Logger.Initialise(NullLogger.Instance);

        Result<Guid> result = await this.FileProcessorDomainService.UploadFile(TestData.UploadFileCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.NotFound);
    }
    
    [Fact]
    public async Task FileRequestHandler_ProcessUploadedFileRequest_RequestIsHandled()
    {
        this.FileProcessorManager.GetFileProfile(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(TestData.FileProfileSafaricom);
        this.FileAggregateRepository.GetLatestVersion(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetEmptyFileAggregate()))
            .Then().ReturnsAsync(Result.Success(TestData.GetCreatedFileAggregate()));
        this.FileAggregateRepository.SaveChanges(Arg<FileAggregate>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success());

        this.FileSystem.AddFile(TestData.FilePathWithName, new MockFileData("D,1,1,1"));

        this.FileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom/inprogress");
        this.FileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom/processed");
        this.FileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom/failed");

        this.SecurityServiceClient.GetToken(Arg<String>.Any(), Arg<String>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.TransactionProcessorClient.GetOperators(Arg<String>.Any(), Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(TestData.OperatorList);
        
        Result result = await this.FileProcessorDomainService.ProcessUploadedFile(TestData.ProcessUploadedFileCommand, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }
    
    [Fact]
    public async Task FileRequestHandler_ProcessUploadedFileRequest_FileNotFound_RequestIsHandled()
    {
        this.FileProcessorManager.GetFileProfile(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(TestData.FileProfileSafaricom);

        this.FileAggregateRepository.GetLatestVersion(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetCreatedFileAggregate()));
        this.FileAggregateRepository.SaveChanges(Arg<FileAggregate>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success());

        this.FileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom/inprogress");
        this.FileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom/processed");
        this.FileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom/failed");

        this.SecurityServiceClient.GetToken(Arg<String>.Any(), Arg<String>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.TransactionProcessorClient.GetOperators(Arg<String>.Any(), Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(TestData.OperatorList);

        Result result = await this.FileProcessorDomainService.ProcessUploadedFile(TestData.ProcessUploadedFileCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.NotFound);
    }
    
    [Fact]
    public async Task FileRequestHandler_ProcessUploadedFileRequest_NoFileProfiles_RequestIsHandled()
    {
        this.FileProcessorManager.GetFileProfile(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.NotFound());

        this.FileImportLogAggregateRepository.GetLatestVersion(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success(TestData.GetEmptyFileImportLogAggregate()));
        this.FileAggregateRepository.SaveChanges(Arg<FileAggregate>.Any(), Arg<CancellationToken>.Any())
       .ReturnsAsync(Result.Success());
        this.FileAggregateRepository.GetLatestVersion(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetCreatedFileAggregate()));

        this.FileSystem.AddFile(TestData.FilePathWithName, new MockFileData("D,1,1,1"));

        this.SecurityServiceClient.GetToken(Arg<String>.Any(), Arg<String>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.TransactionProcessorClient.GetOperators(Arg<String>.Any(), Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(TestData.OperatorList);

        Result result = await this.FileProcessorDomainService.ProcessUploadedFile(TestData.ProcessUploadedFileCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.NotFound);
    }

    [Fact]
    public async Task FileRequestHandler_ProcessUploadedFileRequest_GetFileProfileFailed_RequestIsHandled()
    {
        this.FileProcessorManager.GetFileProfile(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.FileProfileSafaricom))
            .Then().ReturnsAsync(Result.Failure());

        this.FileImportLogAggregateRepository.GetLatestVersion(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success(TestData.GetEmptyFileImportLogAggregate()));
        this.FileAggregateRepository.SaveChanges(Arg<FileAggregate>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success());
        this.FileAggregateRepository.GetLatestVersion(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetCreatedFileAggregate()));

        this.FileSystem.AddFile(TestData.FilePathWithName, new MockFileData("D,1,1,1"));

        this.SecurityServiceClient.GetToken(Arg<String>.Any(), Arg<String>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.TransactionProcessorClient.GetOperators(Arg<String>.Any(), Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(TestData.OperatorList);

        Result result = await this.FileProcessorDomainService.ProcessUploadedFile(TestData.ProcessUploadedFileCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.Failure);
    }


    [Fact]
    public async Task FileRequestHandler_ProcessUploadedFileRequest_NoOperatorsFound_RequestIsHandled()
    {
        this.FileProcessorManager.GetFileProfile(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(TestData.FileProfileSafaricom);

        this.FileImportLogAggregateRepository.GetLatestVersion(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success(TestData.GetEmptyFileImportLogAggregate()));

        this.FileAggregateRepository.GetLatestVersion(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetCreatedFileAggregate()));
        this.FileAggregateRepository.SaveChanges(Arg<FileAggregate>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success());
        this.FileSystem.AddFile(TestData.FilePathWithName, new MockFileData("D,1,1,1"));

        this.SecurityServiceClient.GetToken(Arg<String>.Any(), Arg<String>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.TransactionProcessorClient.GetOperators(Arg<String>.Any(), Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.NotFound());

        Result result = await this.FileProcessorDomainService.ProcessUploadedFile(TestData.ProcessUploadedFileCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.NotFound);
    }

    [Fact]
    public async Task FileRequestHandler_ProcessUploadedFileRequest_ProfileOperatorNotFound_RequestIsHandled()
    {
        this.FileProcessorManager.GetFileProfile(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(TestData.FileProfileSafaricom);

        this.FileImportLogAggregateRepository.GetLatestVersion(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success(TestData.GetEmptyFileImportLogAggregate()));

        this.FileAggregateRepository.GetLatestVersion(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetCreatedFileAggregate()));
        this.FileAggregateRepository.SaveChanges(Arg<FileAggregate>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success());
        this.FileSystem.AddFile(TestData.FilePathWithName, new MockFileData("D,1,1,1"));

        this.SecurityServiceClient.GetToken(Arg<String>.Any(), Arg<String>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.TransactionProcessorClient.GetOperators(Arg<String>.Any(), Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(new List<OperatorResponse>()));

        Result result = await this.FileProcessorDomainService.ProcessUploadedFile(TestData.ProcessUploadedFileCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.NotFound);
    }

    [Fact]
    public async Task FileRequestHandler_ProcessUploadedFileRequest_NullOperatorList_RequestIsHandled()
    {
        this.FileProcessorManager.GetFileProfile(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(TestData.FileProfileSafaricom);

        this.FileImportLogAggregateRepository.GetLatestVersion(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success(TestData.GetEmptyFileImportLogAggregate()));

        this.FileAggregateRepository.GetLatestVersion(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetCreatedFileAggregate()));

        this.FileSystem.AddFile(TestData.FilePathWithName, new MockFileData("D,1,1,1"));

        this.SecurityServiceClient.GetToken(Arg<String>.Any(), Arg<String>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.TransactionProcessorClient.GetOperators(Arg<String>.Any(), Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.NotFound());

        Result result = await this.FileProcessorDomainService.ProcessUploadedFile(TestData.ProcessUploadedFileCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.NotFound);
    }
    
    [Fact]
    public async Task FileRequestHandler_ProcessUploadedFileRequest_ProcessedDirectoryNotFound_RequestIsHandled()
    {
        this.FileProcessorManager.GetFileProfile(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(TestData.FileProfileSafaricom);

        this.FileAggregateRepository.GetLatestVersion(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetCreatedFileAggregate()));
        this.FileAggregateRepository.SaveChanges(Arg<FileAggregate>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success());

        this.FileSystem.AddFile(TestData.FilePathWithName, new MockFileData("D,1,1,1"));

        this.FileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom/inprogress");
        this.FileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom/failed");

        this.SecurityServiceClient.GetToken(Arg<String>.Any(), Arg<String>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.TransactionProcessorClient.GetOperators(Arg<String>.Any(), Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(TestData.OperatorList);
        Result result = await this.FileProcessorDomainService.ProcessUploadedFile(TestData.ProcessUploadedFileCommand, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();

        this.VerifyFileProcessing("home/txnproc/bulkfiles/safaricom/processed");
    }

    [Fact]
    public async Task FileRequestHandler_ProcessUploadedFileRequest_FailedDirectoryNotFound_RequestIsHandled()
    {
        this.FileProcessorManager.GetFileProfile(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(TestData.FileProfileSafaricom);

        this.FileAggregateRepository.GetLatestVersion(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetCreatedFileAggregate()));
        this.FileAggregateRepository.SaveChanges(Arg<FileAggregate>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success());

        this.FileSystem.AddFile(TestData.FilePathWithName, new MockFileData("D,1,1,1"));

        this.FileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom/inprogress");
        this.FileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom/processed");

        this.SecurityServiceClient.GetToken(Arg<String>.Any(), Arg<String>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.TransactionProcessorClient.GetOperators(Arg<String>.Any(), Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(TestData.OperatorList);

        Result result = await this.FileProcessorDomainService.ProcessUploadedFile(TestData.ProcessUploadedFileCommand, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
        this.VerifyFileProcessing("home/txnproc/bulkfiles/safaricom/processed");
    }

    
    [Fact]
    public async Task FileRequestHandler_ProcessUploadedFileRequest_FileIsEmpty_RequestIsHandled()
    {
        this.FileProcessorManager.GetFileProfile(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(TestData.FileProfileSafaricom);

        this.FileAggregateRepository.GetLatestVersion(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetCreatedFileAggregate()));
        this.FileAggregateRepository.SaveChanges(Arg<FileAggregate>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success());

        this.FileSystem.AddFile(TestData.FilePathWithName, new MockFileData(String.Empty));

        this.FileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom/inprogress");
        this.FileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom/processed");
        this.FileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom/failed");

        this.SecurityServiceClient.GetToken(Arg<String>.Any(), Arg<String>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.TransactionProcessorClient.GetOperators(Arg<String>.Any(), Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(TestData.OperatorList);

        Result result = await this.FileProcessorDomainService.ProcessUploadedFile(TestData.ProcessUploadedFileCommand, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();

        this.FileAggregateRepository.SaveChanges(Arg<FileAggregate>.Any(), Arg<CancellationToken>.Any()).Called(Count.Exactly(2));
        this.VerifyFileProcessing("home/txnproc/bulkfiles/safaricom/processed");
    }

    [Fact]
    public async Task FileRequestHandler_ProcessUploadedFileRequest_FileIsInFailedFolder_RequestIsHandled()
    {
        this.FileProcessorManager.GetFileProfile(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(TestData.FileProfileSafaricom);

        this.FileAggregateRepository.GetLatestVersion(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetCreatedFileAggregate()));
        this.FileAggregateRepository.SaveChanges(Arg<FileAggregate>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success());

        this.FileSystem.AddFile(TestData.FailedSafaricomFilePathWithName, new MockFileData(String.Empty));

        this.FileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom/inprogress");
        this.FileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom/processed");
        this.FileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom/failed");

        this.SecurityServiceClient.GetToken(Arg<String>.Any(), Arg<String>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.TransactionProcessorClient.GetOperators(Arg<String>.Any(), Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(TestData.OperatorList);

        Result result = await this.FileProcessorDomainService.ProcessUploadedFile(TestData.ProcessUploadedFileCommand, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();

        this.FileAggregateRepository.SaveChanges(Arg<FileAggregate>.Any(), Arg<CancellationToken>.Any()).Called(Count.Exactly(2));
        this.VerifyFileProcessing("home/txnproc/bulkfiles/safaricom/processed");
    }

    [Fact]
    public async Task FileRequestHandler_ProcessUploadedFileRequest_FileIsInProcessedFolder_RequestIsHandled()
    {
        this.FileProcessorManager.GetFileProfile(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(TestData.FileProfileSafaricom);

        this.FileAggregateRepository.GetLatestVersion(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetCreatedFileAggregate()));
        this.FileAggregateRepository.SaveChanges(Arg<FileAggregate>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success());

        this.FileSystem.AddFile(TestData.ProcessedSafaricomFilePathWithName, new MockFileData(String.Empty));

        this.FileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom/inprogress");
        this.FileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom/processed");
        this.FileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom/failed");

        this.SecurityServiceClient.GetToken(Arg<String>.Any(), Arg<String>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.TransactionProcessorClient.GetOperators(Arg<String>.Any(), Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(TestData.OperatorList);

        Result result = await this.FileProcessorDomainService.ProcessUploadedFile(TestData.ProcessUploadedFileCommand, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();

        this.FileAggregateRepository.SaveChanges(Arg<FileAggregate>.Any(), Arg<CancellationToken>.Any()).Called(Count.Exactly(2));
        this.VerifyFileProcessing("home/txnproc/bulkfiles/safaricom/processed");
    }

    [Theory]
    [InlineData("Safaricom")]
    [InlineData("Voucher")]
    public async Task FileRequestHandler_ProcessTransactionForFileLineRequest_RequestIsHandled(String operatorName)
    {
        this.FileProcessorManager.GetFileProfile(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(TestData.GetFileProfile(operatorName));

        this.FileAggregateRepository.GetLatestVersion(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetFileAggregateWithLines()));
        this.FileAggregateRepository.SaveChanges(Arg<FileAggregate>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success());

        this.TransactionProcessorClient.PerformTransaction(Arg<String>.Any(), Arg<SaleTransactionRequest>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(TestData.ClientSaleTransactionResponse);
            
        this.TransactionProcessorClient.GetMerchant(Arg<String>.Any(), Arg<Guid>.Any(), Arg<Guid>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(TestData.GetMerchantResponseWithOperator);

        this.TransactionProcessorClient.GetMerchantContracts(Arg<String>.Any(), Arg<Guid>.Any(), Arg<Guid>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success(TestData.GetMerchantContractsResponse()));
            
        this.SecurityServiceClient.GetToken(Arg<String>.Any(), Arg<String>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.FileFormatHandler.FileLineCanBeIgnored(Arg<String>.Any()).Returns(false);
        this.FileFormatHandler.ParseFileLine(Arg<String>.Any()).Returns(TestData.TransactionMetadata);
           
        Result result = await this.FileProcessorDomainService.ProcessTransactionForFileLine(TestData.ProcessTransactionForFileLineCommand, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    [Theory]
    [InlineData("Safaricom")]
    [InlineData("Voucher")]
    public async Task FileRequestHandler_ProcessTransactionForFileLineRequest_SaveFailed_RequestIsHandled(String operatorName)
    {
        this.FileProcessorManager.GetFileProfile(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(TestData.GetFileProfile(operatorName));

        this.FileAggregateRepository.GetLatestVersion(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetFileAggregateWithLines()));
        this.FileAggregateRepository.SaveChanges(Arg<FileAggregate>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Failure());

        this.TransactionProcessorClient.PerformTransaction(Arg<String>.Any(), Arg<SaleTransactionRequest>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(TestData.ClientSaleTransactionResponse);

        this.TransactionProcessorClient.GetMerchant(Arg<String>.Any(), Arg<Guid>.Any(), Arg<Guid>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(TestData.GetMerchantResponseWithOperator);

        this.TransactionProcessorClient.GetMerchantContracts(Arg<String>.Any(), Arg<Guid>.Any(), Arg<Guid>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success(TestData.GetMerchantContractsResponse()));

        this.SecurityServiceClient.GetToken(Arg<String>.Any(), Arg<String>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.FileFormatHandler.FileLineCanBeIgnored(Arg<String>.Any()).Returns(false);
        this.FileFormatHandler.ParseFileLine(Arg<String>.Any()).Returns(TestData.TransactionMetadata);

        Result result = await this.FileProcessorDomainService.ProcessTransactionForFileLine(TestData.ProcessTransactionForFileLineCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Theory]
    [InlineData("Safaricom")]
    [InlineData("Voucher")]
    public async Task FileRequestHandler_ProcessTransactionForFileLineRequest_ExceptionThrown_RequestIsHandled(String operatorName)
    {
        this.FileProcessorManager.GetFileProfile(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(TestData.GetFileProfile(operatorName));

        this.FileAggregateRepository.GetLatestVersion(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetFileAggregateWithLines()));
        this.FileAggregateRepository.SaveChanges(Arg<FileAggregate>.Any(), Arg<CancellationToken>.Any())
            .ThrowsAsync(new Exception());

        this.TransactionProcessorClient.PerformTransaction(Arg<String>.Any(), Arg<SaleTransactionRequest>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(TestData.ClientSaleTransactionResponse);

        this.TransactionProcessorClient.GetMerchant(Arg<String>.Any(), Arg<Guid>.Any(), Arg<Guid>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(TestData.GetMerchantResponseWithOperator);

        this.TransactionProcessorClient.GetMerchantContracts(Arg<String>.Any(), Arg<Guid>.Any(), Arg<Guid>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success(TestData.GetMerchantContractsResponse()));

        this.SecurityServiceClient.GetToken(Arg<String>.Any(), Arg<String>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.FileFormatHandler.FileLineCanBeIgnored(Arg<String>.Any()).Returns(false);
        this.FileFormatHandler.ParseFileLine(Arg<String>.Any()).Returns(TestData.TransactionMetadata);

        Result result = await this.FileProcessorDomainService.ProcessTransactionForFileLine(TestData.ProcessTransactionForFileLineCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }


    [Fact]
    public async Task FileRequestHandler_ProcessTransactionForFileLineRequest_WithOperatorName_RequestIsHandled()
    {
        this.FileProcessorManager.GetFileProfile(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(TestData.FileProfileVoucher);

        this.FileAggregateRepository.GetLatestVersion(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetFileAggregateWithLines()));
        this.FileAggregateRepository.SaveChanges(Arg<FileAggregate>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success());

        this.TransactionProcessorClient.PerformTransaction(Arg<String>.Any(), Arg<SaleTransactionRequest>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(TestData.ClientSaleTransactionResponse);

        this.TransactionProcessorClient.GetMerchant(Arg<String>.Any(), Arg<Guid>.Any(), Arg<Guid>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(TestData.GetMerchantResponseWithOperator);

        this.TransactionProcessorClient.GetMerchantContracts(Arg<String>.Any(), Arg<Guid>.Any(), Arg<Guid>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success(TestData.GetMerchantContractsResponse()));

        this.SecurityServiceClient.GetToken(Arg<String>.Any(), Arg<String>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.FileFormatHandler.FileLineCanBeIgnored(Arg<String>.Any()).Returns(false);
        this.FileFormatHandler.ParseFileLine(Arg<String>.Any()).Returns(TestData.TransactionMetadataWithOperatorName);

        Result result = await this.FileProcessorDomainService.ProcessTransactionForFileLine(TestData.ProcessTransactionForFileLineCommand, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
    }

    
    [Fact]
    public async Task FileRequestHandler_ProcessTransactionLineForFileRequest_FileAggregateNotFound_RequestHandled()
    {
        this.FileAggregateRepository.GetLatestVersion(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.NotFound());
           
        var result = await this.FileProcessorDomainService.ProcessTransactionForFileLine(TestData.ProcessTransactionForFileLineCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.NotFound);
    }
    
    [Fact]
    public async Task  FileRequestHandler_ProcessTransactionLineForFileRequest_FileAggregateWithNoLines_RequestHandled()
    {
        this.FileAggregateRepository.GetLatestVersion(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetCreatedFileAggregate()));

        var result = await this.FileProcessorDomainService.ProcessTransactionForFileLine(TestData.ProcessTransactionForFileLineCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.Invalid);
    }
    
    [Fact]
    public async Task FileRequestHandler_ProcessTransactionLineForFileRequest_LineInRequestNotFoundInFileAggregate_RequestHandled()
    {
        this.FileAggregateRepository.GetLatestVersion(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetFileAggregateWithLines()));
            
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
        this.FileAggregateRepository.GetLatestVersion(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetFileAggregateWithLinesAlreadyProcessed()));
        this.FileAggregateRepository.SaveChanges(Arg<FileAggregate>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success());
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
        this.FileAggregateRepository.GetLatestVersion(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetFileAggregateWithLines()));
        this.FileProcessorManager.GetFileProfile(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.NotFound());
        var result = await this.FileProcessorDomainService.ProcessTransactionForFileLine(
                TestData.ProcessTransactionForFileLineCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.NotFound);
    }
    
    [Fact]
    public async Task FileRequestHandler_ProcessTransactionForFileLineRequest_FileLineIgnored_RequestIsHandled()
    {
        this.FileProcessorManager.GetFileProfile(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(TestData.FileProfileSafaricom);

        this.FileAggregateRepository.GetLatestVersion(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetFileAggregateWithLines()));
        this.FileAggregateRepository.SaveChanges(Arg<FileAggregate>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success());
        this.TransactionProcessorClient.PerformTransaction(Arg<String>.Any(), Arg<SaleTransactionRequest>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(TestData.ClientSaleTransactionResponse);

        this.TransactionProcessorClient.GetMerchant(Arg<String>.Any(), Arg<Guid>.Any(), Arg<Guid>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(TestData.GetMerchantResponseWithOperator);

        this.TransactionProcessorClient.GetMerchantContracts(Arg<String>.Any(), Arg<Guid>.Any(), Arg<Guid>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success(TestData.GetMerchantContractsResponse()));

        this.SecurityServiceClient.GetToken(Arg<String>.Any(), Arg<String>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.FileFormatHandler.FileLineCanBeIgnored(Arg<String>.Any()).Returns(true);
            
        Result result = await this.FileProcessorDomainService.ProcessTransactionForFileLine(TestData.ProcessTransactionForFileLineCommand, CancellationToken.None);
                        result.IsSuccess.ShouldBeTrue();
        this.FileFormatHandler.ParseFileLine(Arg<String>.Any()).Called(Count.Never());
    }
    
    [Fact]
    public async Task FileRequestHandler_ProcessTransactionForFileLineRequest_EmptyFileLineIgnored_RequestIsHandled()
    {
        this.FileProcessorManager.GetFileProfile(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(TestData.FileProfileSafaricom);

        this.FileAggregateRepository.GetLatestVersion(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetFileAggregateWithBlankLine()));
        
        this.TransactionProcessorClient.PerformTransaction(Arg<String>.Any(), Arg<SaleTransactionRequest>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(TestData.ClientSaleTransactionResponse);

        this.TransactionProcessorClient.GetMerchant(Arg<String>.Any(), Arg<Guid>.Any(), Arg<Guid>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(TestData.GetMerchantResponseWithOperator);

        this.TransactionProcessorClient.GetMerchantContracts(Arg<String>.Any(), Arg<Guid>.Any(), Arg<Guid>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success( TestData.GetMerchantContractsResponse()));

        this.SecurityServiceClient.GetToken(Arg<String>.Any(), Arg<String>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.TokenResponse()));
var result =                             await this.FileProcessorDomainService.ProcessTransactionForFileLine(TestData.ProcessTransactionForFileLineCommand, CancellationToken.None);
        this.FileFormatHandler.ParseFileLine(Arg<String>.Any()).Called(Count.Never());
    }

    [Fact]
    public async Task FileRequestHandler_ProcessTransactionForFileLineRequest_FileParsingFailed_RequestIsHandled()
    {
        this.FileProcessorManager.GetFileProfile(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(TestData.FileProfileSafaricom);

        this.FileAggregateRepository.GetLatestVersion(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetFileAggregateWithLines()));
        this.FileAggregateRepository.SaveChanges(Arg<FileAggregate>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success());
        this.TransactionProcessorClient.PerformTransaction(Arg<String>.Any(), Arg<SaleTransactionRequest>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(TestData.ClientSaleTransactionResponse);

        this.TransactionProcessorClient.GetMerchant(Arg<String>.Any(), Arg<Guid>.Any(), Arg<Guid>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(TestData.GetMerchantResponseWithOperator);

        this.TransactionProcessorClient.GetMerchantContracts(Arg<String>.Any(), Arg<Guid>.Any(), Arg<Guid>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success(TestData.GetMerchantContractsResponse()));

        this.SecurityServiceClient.GetToken(Arg<String>.Any(), Arg<String>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.TokenResponse()));
        
        this.FileFormatHandler.FileLineCanBeIgnored(Arg<String>.Any()).Returns(false);
        this.FileFormatHandler.ParseFileLine(Arg<String>.Any()).Throws<InvalidDataException>();

        var result = await this.FileProcessorDomainService.ProcessTransactionForFileLine(TestData.ProcessTransactionForFileLineCommand, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
        this.TransactionProcessorClient.GetMerchant(Arg<String>.Any(), Arg<Guid>.Any(), Arg<Guid>.Any(), Arg<CancellationToken>.Any()).Called(Count.Never());
    }

    [Fact]
    public async Task FileRequestHandler_ProcessTransactionForFileLineRequest_MerchantNotFound_RequestIsHandled()
    {
        this.FileProcessorManager.GetFileProfile(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(TestData.FileProfileSafaricom);

        this.FileAggregateRepository.GetLatestVersion(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetFileAggregateWithLines()));

        this.TransactionProcessorClient.PerformTransaction(Arg<String>.Any(), Arg<SaleTransactionRequest>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(TestData.ClientSaleTransactionResponse);

        this.TransactionProcessorClient
            .GetMerchant(Arg<String>.Any(), Arg<Guid>.Any(), Arg<Guid>.Any(),
                Arg<CancellationToken>.Any()).ReturnsAsync(Result.NotFound());
        this.TransactionProcessorClient.GetMerchantContracts(Arg<String>.Any(), Arg<Guid>.Any(), Arg<Guid>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success(TestData.GetMerchantContractsResponse()));

        this.SecurityServiceClient.GetToken(Arg<String>.Any(), Arg<String>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.FileFormatHandler.FileLineCanBeIgnored(Arg<String>.Any()).Returns(false);
        this.FileFormatHandler.ParseFileLine(Arg<String>.Any()).Returns(TestData.TransactionMetadata);
        
        var result =await this.FileProcessorDomainService.ProcessTransactionForFileLine(TestData.ProcessTransactionForFileLineCommand, CancellationToken.None);
                                        result.IsFailed.ShouldBeTrue();
                                        result.Status.ShouldBe(ResultStatus.NotFound);
    }

    [Fact]
    public async Task FileRequestHandler_ProcessTransactionForFileLineRequest_NoMerchantContractsFound_RequestIsHandled()
    {
        this.FileProcessorManager.GetFileProfile(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(TestData.FileProfileSafaricom);

        this.FileAggregateRepository.GetLatestVersion(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetFileAggregateWithLines()));

        this.TransactionProcessorClient.PerformTransaction(Arg<String>.Any(), Arg<SaleTransactionRequest>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(TestData.ClientSaleTransactionResponse);

        this.TransactionProcessorClient.GetMerchant(Arg<String>.Any(), Arg<Guid>.Any(), Arg<Guid>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(TestData.GetMerchantResponseWithOperator);

        this.TransactionProcessorClient.GetMerchantContracts(Arg<String>.Any(), Arg<Guid>.Any(), Arg<Guid>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.NotFound());

        this.SecurityServiceClient.GetToken(Arg<String>.Any(), Arg<String>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.FileFormatHandler.FileLineCanBeIgnored(Arg<String>.Any()).Returns(false);
        this.FileFormatHandler.ParseFileLine(Arg<String>.Any()).Returns(TestData.TransactionMetadata);

        var result = await this.FileProcessorDomainService.ProcessTransactionForFileLine(TestData.ProcessTransactionForFileLineCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.NotFound);
    }

    [Fact]
    public async Task FileRequestHandler_ProcessTransactionForFileLineRequest_EmptyMerchantContractsArray_RequestIsHandled()
    {
        this.FileProcessorManager.GetFileProfile(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(TestData.FileProfileSafaricom);

        this.FileAggregateRepository.GetLatestVersion(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetFileAggregateWithLines()));

        this.TransactionProcessorClient.PerformTransaction(Arg<String>.Any(), Arg<SaleTransactionRequest>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(TestData.ClientSaleTransactionResponse);

        this.TransactionProcessorClient.GetMerchant(Arg<String>.Any(), Arg<Guid>.Any(), Arg<Guid>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(TestData.GetMerchantResponseWithOperator);

        this.TransactionProcessorClient.GetMerchantContracts(Arg<String>.Any(), Arg<Guid>.Any(), Arg<Guid>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success(new List<ContractResponse>()));

        this.SecurityServiceClient.GetToken(Arg<String>.Any(), Arg<String>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.FileFormatHandler.FileLineCanBeIgnored(Arg<String>.Any()).Returns(false);
        this.FileFormatHandler.ParseFileLine(Arg<String>.Any()).Returns(TestData.TransactionMetadata);

        var result = await this.FileProcessorDomainService.ProcessTransactionForFileLine(TestData.ProcessTransactionForFileLineCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.NotFound);
    }

    [Fact]
    public async Task FileRequestHandler_ProcessTransactionForFileLineRequest_ContractNotFoundInMerchantContracts_RequestIsHandled()
    {
        this.FileProcessorManager.GetFileProfile(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(TestData.FileProfileSafaricom);

        this.FileAggregateRepository.GetLatestVersion(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetFileAggregateWithLines()));

        this.TransactionProcessorClient.PerformTransaction(Arg<String>.Any(), Arg<SaleTransactionRequest>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(TestData.ClientSaleTransactionResponse);

        this.TransactionProcessorClient.GetMerchant(Arg<String>.Any(), Arg<Guid>.Any(), Arg<Guid>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(TestData.GetMerchantResponseWithOperator);

        this.TransactionProcessorClient.GetMerchantContracts(Arg<String>.Any(), Arg<Guid>.Any(), Arg<Guid>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success(new List<ContractResponse> {
                new ContractResponse {
                    OperatorName = "Other Operator"
                }
            }));

        this.SecurityServiceClient.GetToken(Arg<String>.Any(), Arg<String>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.FileFormatHandler.FileLineCanBeIgnored(Arg<String>.Any()).Returns(false);
        this.FileFormatHandler.ParseFileLine(Arg<String>.Any()).Returns(TestData.TransactionMetadata);

        var result = await this.FileProcessorDomainService.ProcessTransactionForFileLine(TestData.ProcessTransactionForFileLineCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.NotFound);
    }

    [Fact]
    public async Task FileRequestHandler_ProcessTransactionForFileLineRequest_NoMerchantContractForFileOperatorFound_RequestIsHandled()
    {
        this.FileProcessorManager.GetFileProfile(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(TestData.FileProfileSafaricom);

        this.FileAggregateRepository.GetLatestVersion(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetFileAggregateWithLines()));

        this.TransactionProcessorClient.PerformTransaction(Arg<String>.Any(), Arg<SaleTransactionRequest>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(TestData.ClientSaleTransactionResponse);

        this.TransactionProcessorClient.GetMerchant(Arg<String>.Any(), Arg<Guid>.Any(), Arg<Guid>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(TestData.GetMerchantResponseWithOperator);

        this.TransactionProcessorClient.GetMerchantContracts(Arg<String>.Any(), Arg<Guid>.Any(), Arg<Guid>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.NotFound());

        this.SecurityServiceClient.GetToken(Arg<String>.Any(), Arg<String>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.FileFormatHandler.FileLineCanBeIgnored(Arg<String>.Any()).Returns(false);
        this.FileFormatHandler.ParseFileLine(Arg<String>.Any()).Returns(TestData.TransactionMetadata);

        var result = await this.FileProcessorDomainService.ProcessTransactionForFileLine(TestData.ProcessTransactionForFileLineCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.NotFound);
    }

    [Fact]
    public async Task FileRequestHandler_ProcessTransactionForFileLineRequest_MerchantContractProductNotFound_RequestIsHandled()
    {
        this.FileProcessorManager.GetFileProfile(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(TestData.FileProfileSafaricom);

        this.FileAggregateRepository.GetLatestVersion(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetFileAggregateWithLines()));

        this.TransactionProcessorClient.PerformTransaction(Arg<String>.Any(), Arg<SaleTransactionRequest>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(TestData.ClientSaleTransactionResponse);

        this.TransactionProcessorClient.GetMerchant(Arg<String>.Any(), Arg<Guid>.Any(), Arg<Guid>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(TestData.GetMerchantResponseWithOperator);

        this.TransactionProcessorClient.GetMerchantContracts(Arg<String>.Any(), Arg<Guid>.Any(), Arg<Guid>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success(TestData.GetMerchantContractsResponseNoNullValueProduct()));

        this.SecurityServiceClient.GetToken(Arg<String>.Any(), Arg<String>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.FileFormatHandler.FileLineCanBeIgnored(Arg<String>.Any()).Returns(false);
        this.FileFormatHandler.ParseFileLine(Arg<String>.Any()).Returns(TestData.TransactionMetadata);

        var result = await this.FileProcessorDomainService.ProcessTransactionForFileLine(TestData.ProcessTransactionForFileLineCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.NotFound);
    }

    [Fact]
    public async Task FileRequestHandler_ProcessTransactionForFileLineRequest_TransactionNotSuccessfulResult_RequestIsHandled()
    {
        this.FileProcessorManager.GetFileProfile(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(TestData.FileProfileSafaricom);

        this.FileAggregateRepository.GetLatestVersion(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetFileAggregateWithLines()));

        this.TransactionProcessorClient.PerformTransaction(Arg<String>.Any(), Arg<SaleTransactionRequest>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Failure());

        this.TransactionProcessorClient.GetMerchant(Arg<String>.Any(), Arg<Guid>.Any(), Arg<Guid>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(TestData.GetMerchantResponseWithOperator);

        this.TransactionProcessorClient.GetMerchantContracts(Arg<String>.Any(), Arg<Guid>.Any(), Arg<Guid>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success(TestData.GetMerchantContractsResponse()));

        this.SecurityServiceClient.GetToken(Arg<String>.Any(), Arg<String>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.FileFormatHandler.FileLineCanBeIgnored(Arg<String>.Any()).Returns(false);
        this.FileFormatHandler.ParseFileLine(Arg<String>.Any()).Returns(TestData.TransactionMetadata);

        var result = await this.FileProcessorDomainService.ProcessTransactionForFileLine(TestData.ProcessTransactionForFileLineCommand, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.Failure);
    }

    [Fact]
    public async Task FileRequestHandler_ProcessTransactionForFileLineRequest_TransactionNotSuccessful_RequestIsHandled()
    {
        this.FileProcessorManager.GetFileProfile(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(TestData.FileProfileSafaricom);

        this.FileAggregateRepository.GetLatestVersion(Arg<Guid>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.GetFileAggregateWithLines()));

        this.TransactionProcessorClient.PerformTransaction(Arg<String>.Any(), Arg<SaleTransactionRequest>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(TestData.ClientSaleTransactionFailedResponse);

        this.TransactionProcessorClient.GetMerchant(Arg<String>.Any(), Arg<Guid>.Any(), Arg<Guid>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(TestData.GetMerchantResponseWithOperator);

        this.TransactionProcessorClient.GetMerchantContracts(Arg<String>.Any(), Arg<Guid>.Any(), Arg<Guid>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success(TestData.GetMerchantContractsResponse()));

        this.SecurityServiceClient.GetToken(Arg<String>.Any(), Arg<String>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(TestData.TokenResponse()));

        this.FileFormatHandler.FileLineCanBeIgnored(Arg<String>.Any()).Returns(false);
        this.FileFormatHandler.ParseFileLine(Arg<String>.Any()).Returns(TestData.TransactionMetadata);

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
