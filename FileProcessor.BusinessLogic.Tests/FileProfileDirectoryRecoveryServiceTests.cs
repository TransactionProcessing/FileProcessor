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
            .GetAllFileProfiles(Arg<CancellationToken>.Any())
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
                command.EstateId == TestData.EstateId),
            Arg<CancellationToken>.Any()).Called(Count.Once());
    }
}
