using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FileProcessor.BusinessLogic.Managers;
using FileProcessor.BusinessLogic.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SimpleResults;
using Shouldly;
using System.IO.Abstractions.TestingHelpers;
using Xunit;
using FileProfileModel = global::FileProcessor.Models.FileProfile;

namespace FileProcessor.BusinessLogic.Tests;

public class FileProfileDirectorySynchronizerTests
{
    [Fact]
    public async Task SyncAsync_CreatesTemporaryAndProfileDirectories()
    {
        MockFileSystem fileSystem = new MockFileSystem();
        Mock<IFileProfileManager> fileProfileManager = new Mock<IFileProfileManager>();
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AppSettings:TemporaryFileLocation"] = @"C:\temp\temporary"
            })
            .Build();

        FileProfileModel profile = new FileProfileModel(
            Guid.NewGuid(),
            "Profile A",
            @"C:\temp\profile-a",
            "ProfileARequest",
            "Operator A",
            "1",
            "ProfileAHandler");

        fileProfileManager
            .Setup(manager => manager.GetAllFileProfiles(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(Result.Success(new List<FileProfileModel> { profile })));

        FileProfileDirectorySynchronizer synchronizer = new FileProfileDirectorySynchronizer(
            configuration,
            fileProfileManager.Object,
            fileSystem,
            NullLogger<FileProfileDirectorySynchronizer>.Instance);

        Result result = await synchronizer.SyncAsync(CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        fileSystem.Directory.Exists(@"C:\temp\temporary").ShouldBeTrue();
        fileSystem.Directory.Exists(Path.Combine(profile.ListeningDirectory, "inprogress")).ShouldBeTrue();
        fileSystem.Directory.Exists(profile.ProcessedDirectory).ShouldBeTrue();
        fileSystem.Directory.Exists(profile.FailedDirectory).ShouldBeTrue();
    }
}
