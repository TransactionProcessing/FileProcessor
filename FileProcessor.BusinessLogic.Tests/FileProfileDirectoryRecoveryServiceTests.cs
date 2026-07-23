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
using Moq;
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

        Mock<IFileProcessorManager> fileProcessorManager = new();
        Mock<IMediator> mediator = new();
        MockFileSystem fileSystem = new();

        FileProfileModel fileProfile = TestData.FileProfile;
        string inProgressDirectory = $"{fileProfile.ListeningDirectory}/inprogress";
        string inProgressFilePath = $"{inProgressDirectory}/{TestData.EstateId:N}-{TestData.FileId:N}";

        fileSystem.AddDirectory(inProgressDirectory);
        fileSystem.AddFile(inProgressFilePath, new MockFileData("D,1,1,1"));

        fileProcessorManager
            .Setup(manager => manager.GetAllFileProfiles(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new List<FileProfileModel> { fileProfile }));

        fileProcessorManager
            .Setup(manager => manager.GetFile(TestData.FileId, TestData.EstateId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(TestData.GetCreatedFileDetails()));

        mediator
            .Setup(send => send.Send(It.IsAny<FileCommands.ProcessUploadedFileCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        FileProfileDirectoryRecoveryService service = new(
            fileProcessorManager.Object,
            mediator.Object,
            fileSystem);

        Result result = await service.RecoverInProgressFilesAsync(CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        mediator.Verify(send => send.Send(
            It.Is<FileCommands.ProcessUploadedFileCommand>(command =>
                command.FileId == TestData.FileId &&
                Path.GetFileName(command.FilePath) == Path.GetFileName(inProgressFilePath) &&
                command.FileProfileId == TestData.FileProfileId &&
                command.EstateId == TestData.EstateId),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
