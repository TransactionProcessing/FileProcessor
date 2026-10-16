using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Threading;
using System.Threading.Tasks;
using FileProcessor.BusinessLogic.Managers;
using FileProcessor.BusinessLogic.Requests;
using FileProcessor.BusinessLogic.Services;
using FileProcessor.Models;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using Imposter.Abstractions;
using Shared.Results;
using Shouldly;
using SimpleResults;
using FileProcessor.Testing;
using FileProfileModel = global::FileProcessor.Models.FileProfile;
using FileImportLogModel = global::FileProcessor.Models.FileImportLog;
using Xunit;

namespace FileProcessor.BusinessLogic.Tests;

public class FileProfileDirectoryRecoveryServiceTests
{
    [Fact]
    public async Task RecoverInProgressFilesAsync_ReplaysFilesFoundInInProgressDirectory()
    {
        Shared.Logger.Logger.Initialise(Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance);

        IFileProcessorManagerImposter fileProcessorManager = new();
        IMediatorImposter mediator = new();
        MockFileSystem fileSystem = new();

        FileProfileModel fileProfile = TestData.FileProfile;
        string inProgressDirectory = $"{fileProfile.ListeningDirectory}/inprogress";
        string inProgressFilePath = $"{inProgressDirectory}/{TestData.EstateId:N}-{TestData.FileId:N}";

        fileSystem.AddDirectory(inProgressDirectory);
        fileSystem.AddFile(inProgressFilePath, new MockFileData("D,1,1,1"));

        fileProcessorManager
            .GetAllFileProfileVersions(Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success(new List<FileProfileModel> { fileProfile }));

        fileProcessorManager
            .GetFile(TestData.FileId, TestData.EstateId, Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success(TestData.GetCreatedFileDetails()));

        mediator
            .Send(Arg<IRequest<Result>>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success());

        FileProfileDirectoryRecoveryService service = new(
            fileProcessorManager.Instance(),
            mediator.Instance(),
            fileSystem);

        Result result = await service.RecoverInProgressFilesAsync(CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        mediator.Send(
            Arg<IRequest<Result>>.Is(request => request is FileCommands.ProcessUploadedFileCommand command &&
                command.FileId == TestData.FileId &&
                Path.GetFileName(command.FilePath) == Path.GetFileName(inProgressFilePath) &&
                command.FileProfileId == TestData.FileProfileId &&
                command.FileProfileVersionId == TestData.FileProfileVersionId &&
                command.EstateId == TestData.EstateId),
            Arg<CancellationToken>.Any()).Called(Count.Once());
    }

    [Fact]
    public async Task RecoverInProgressFilesAsync_WhenFileAggregateIsMissing_ReplaysUsingImportLogDetails()
    {
        Shared.Logger.Logger.Initialise(Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance);

        IFileProcessorManagerImposter fileProcessorManager = new();
        IMediatorImposter mediator = new();
        MockFileSystem fileSystem = new();

        FileProfileModel fileProfile = TestData.FileProfile;
        string inProgressDirectory = $"{fileProfile.ListeningDirectory}/inprogress";
        string inProgressFilePath = $"{inProgressDirectory}/{TestData.EstateId:N}-{TestData.FileId:N}";

        fileSystem.AddDirectory(inProgressDirectory);
        fileSystem.AddFile(inProgressFilePath, new MockFileData("D,1,1,1"));

        fileProcessorManager
            .GetAllFileProfileVersions(Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success(new List<FileProfileModel> { fileProfile }));

        fileProcessorManager
            .GetFile(TestData.FileId, TestData.EstateId, Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.NotFound("File aggregate not found"));

        fileProcessorManager
            .GetFileImportLogForFile(TestData.FileId, TestData.EstateId, Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success(new FileImportLogModel
            {
                EstateId = TestData.EstateId,
                FileImportLogId = TestData.FileImportLogId,
                FileImportLogDateTime = TestData.FileUploadedDateTime,
                Files = new List<ImportLogFile>
                {
                    new()
                    {
                        EstateId = TestData.EstateId,
                        FileId = TestData.FileId,
                        FileProfileId = TestData.FileProfileId,
                        FileProfileVersionId = TestData.FileProfileVersionId,
                        MerchantId = TestData.MerchantId,
                        UserId = TestData.UserId,
                        UploadedDateTime = TestData.FileUploadedDateTime
                    }
                }
            }));

        mediator
            .Send(Arg<IRequest<Result>>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success());

        FileProfileDirectoryRecoveryService service = new(
            fileProcessorManager.Instance(),
            mediator.Instance(),
            fileSystem);

        Result result = await service.RecoverInProgressFilesAsync(CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        mediator.Send(
            Arg<IRequest<Result>>.Is(request => request is FileCommands.ProcessUploadedFileCommand command &&
                command.FileId == TestData.FileId &&
                command.FileImportLogId == TestData.FileImportLogId &&
                command.FileProfileId == TestData.FileProfileId &&
                command.FileProfileVersionId == TestData.FileProfileVersionId &&
                command.EstateId == TestData.EstateId &&
                command.MerchantId == TestData.MerchantId &&
                command.UserId == TestData.UserId &&
                command.FileUploadedDateTime == TestData.FileUploadedDateTime &&
                Path.GetFileName(command.FilePath) == Path.GetFileName(inProgressFilePath)),
            Arg<CancellationToken>.Any()).Called(Count.Once());
    }

    [Fact]
    public async Task RecoverInProgressFilesAsync_ReplaysFilesFromHistoricalAndCurrentDirectories()
    {
        Shared.Logger.Logger.Initialise(Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance);

        IFileProcessorManagerImposter fileProcessorManager = new();
        IMediatorImposter mediator = new();
        MockFileSystem fileSystem = new();
        FileProfileModel historicalVersion = new(
            TestData.FileProfileId,
            TestData.SafaricomProfileName,
            "/home/txnproc/bulkfiles/safaricom-old",
            TestData.SafaricomRequestType,
            TestData.SafaricomOperatorIdentifier,
            "\n",
            TestData.SafaricomFileFormatHandler,
            TestData.FileProfileVersionId,
            1);
        FileProfileModel currentVersion = new(
            TestData.FileProfileId,
            TestData.SafaricomProfileName,
            "/home/txnproc/bulkfiles/safaricom-current",
            TestData.SafaricomRequestType,
            TestData.SafaricomOperatorIdentifier,
            "\n",
            TestData.SafaricomFileFormatHandler,
            Guid.NewGuid(),
            2);
        string historicalFilePath = $"{historicalVersion.ListeningDirectory}/inprogress/{TestData.EstateId:N}-{TestData.FileId:N}";
        string currentFilePath = $"{currentVersion.ListeningDirectory}/inprogress/{TestData.EstateId:N}-{TestData.FileId2:N}";
        fileSystem.AddFile(historicalFilePath, new MockFileData("D,1,1,1"));
        fileSystem.AddFile(currentFilePath, new MockFileData("D,1,1,1"));

        fileProcessorManager.GetAllFileProfileVersions(Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success(new List<FileProfileModel> { historicalVersion, currentVersion }));
        fileProcessorManager.GetFile(Arg<Guid>.Any(), Arg<Guid>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success(TestData.GetCreatedFileDetails()));
        mediator.Send(Arg<IRequest<Result>>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success());

        FileProfileDirectoryRecoveryService service = new(
            fileProcessorManager.Instance(),
            mediator.Instance(),
            fileSystem);

        Result result = await service.RecoverInProgressFilesAsync(CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        mediator.Send(Arg<IRequest<Result>>.Is(request => request is FileCommands.ProcessUploadedFileCommand command &&
            (Path.GetFileName(command.FilePath) == Path.GetFileName(historicalFilePath) ||
             Path.GetFileName(command.FilePath) == Path.GetFileName(currentFilePath))),
            Arg<CancellationToken>.Any()).Called(Count.Exactly(2));
    }

    [Fact]
    public async Task RecoverInProgressFilesAsync_WhenVersionsShareDirectory_ReplaysFileOnce()
    {
        Shared.Logger.Logger.Initialise(Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance);

        IFileProcessorManagerImposter fileProcessorManager = new();
        IMediatorImposter mediator = new();
        MockFileSystem fileSystem = new();
        FileProfileModel firstVersion = new(
            TestData.FileProfileId,
            TestData.SafaricomProfileName,
            TestData.SafaricomListeningDirectory,
            TestData.SafaricomRequestType,
            TestData.SafaricomOperatorIdentifier,
            "\n",
            TestData.SafaricomFileFormatHandler,
            Guid.NewGuid(),
            1);
        FileProfileModel secondVersion = new(
            TestData.FileProfileId,
            TestData.SafaricomProfileName,
            TestData.SafaricomListeningDirectory,
            TestData.SafaricomRequestType,
            TestData.SafaricomOperatorIdentifier,
            "\n",
            TestData.SafaricomFileFormatHandler,
            Guid.NewGuid(),
            2);
        string filePath = $"{firstVersion.ListeningDirectory}/inprogress/{TestData.EstateId:N}-{TestData.FileId:N}";
        fileSystem.AddFile(filePath, new MockFileData("D,1,1,1"));
        fileProcessorManager.GetAllFileProfileVersions(Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success(new List<FileProfileModel> { firstVersion, secondVersion }));
        fileProcessorManager.GetFile(Arg<Guid>.Any(), Arg<Guid>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success(TestData.GetCreatedFileDetails()));
        mediator.Send(Arg<IRequest<Result>>.Any(), Arg<CancellationToken>.Any())
            .ReturnsAsync(Result.Success());

        FileProfileDirectoryRecoveryService service = new(
            fileProcessorManager.Instance(),
            mediator.Instance(),
            fileSystem);

        Result result = await service.RecoverInProgressFilesAsync(CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        mediator.Send(Arg<IRequest<Result>>.Any(), Arg<CancellationToken>.Any()).Called(Count.Once());
    }

}
