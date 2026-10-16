using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FileProcessor.BusinessLogic.Managers;
using FileProcessor.BusinessLogic.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Imposter.Abstractions;
using SimpleResults;
using Shouldly;
using System.IO.Abstractions.TestingHelpers;
using Xunit;
using FileProfileModel = global::FileProcessor.Models.FileProfile;

namespace FileProcessor.BusinessLogic.Tests;

public class FileProfileDirectorySynchronizerTests
{
    [Fact]
    public async Task SyncAsync_CreatesTemporaryAndHistoricalProfileVersionDirectories()
    {
        MockFileSystem fileSystem = new MockFileSystem();
        IFileProfileManagerImposter fileProfileManager = new IFileProfileManagerImposter();
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

        string historicalDirectory = @"C:\temp\profile-a-old";
        FileProfileModel historicalProfileVersion = new(
            profile.FileProfileId,
            "Profile A",
            historicalDirectory,
            "ProfileARequest",
            "Operator A",
            "1",
            "ProfileAHandler",
            Guid.NewGuid(),
            1);
        fileSystem.AddFile(Path.Combine(historicalDirectory, "inprogress", "existing-file"), new MockFileData("contents"));

        fileProfileManager
            .GetAllFileProfileVersions(Arg<CancellationToken>.Any())
            .Returns(Task.FromResult(Result.Success(new List<FileProfileModel> { historicalProfileVersion, profile })));

        FileProfileDirectorySynchronizer synchronizer = new FileProfileDirectorySynchronizer(
            configuration,
            fileProfileManager.Instance(),
            fileSystem,
            NullLogger<FileProfileDirectorySynchronizer>.Instance);

        Result result = await synchronizer.SyncAsync(CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        fileSystem.Directory.Exists(@"C:\temp\temporary").ShouldBeTrue();
        fileSystem.Directory.Exists(Path.Combine(profile.ListeningDirectory, "inprogress")).ShouldBeTrue();
        fileSystem.Directory.Exists(profile.ProcessedDirectory).ShouldBeTrue();
        fileSystem.Directory.Exists(profile.FailedDirectory).ShouldBeTrue();
        fileSystem.Directory.Exists(Path.Combine(historicalDirectory, "inprogress")).ShouldBeTrue();
        fileSystem.Directory.Exists(Path.Combine(historicalDirectory, "processed")).ShouldBeTrue();
        fileSystem.Directory.Exists(Path.Combine(historicalDirectory, "failed")).ShouldBeTrue();
        fileSystem.File.Exists(Path.Combine(historicalDirectory, "inprogress", "existing-file")).ShouldBeTrue();
    }

    [Fact]
    public async Task SyncAsync_WhenVersionsShareDirectory_EnsuresSharedDirectoryWithoutDeletingFiles()
    {
        MockFileSystem fileSystem = new();
        IFileProfileManagerImposter fileProfileManager = new();
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AppSettings:TemporaryFileLocation"] = @"C:\temp\temporary"
            })
            .Build();
        string sharedDirectory = @"C:\temp\shared-profile";
        FileProfileModel firstVersion = new(
            Guid.NewGuid(), "Profile A", sharedDirectory, "ProfileARequest", "Operator A", "1", "ProfileAHandler", Guid.NewGuid(), 1);
        FileProfileModel secondVersion = new(
            firstVersion.FileProfileId, "Profile A", sharedDirectory, "ProfileARequest", "Operator A", "1", "ProfileAHandler", Guid.NewGuid(), 2);
        string existingFile = Path.Combine(sharedDirectory, "failed", "old-file");
        fileSystem.AddFile(existingFile, new MockFileData("contents"));

        fileProfileManager.GetAllFileProfileVersions(Arg<CancellationToken>.Any())
            .Returns(Task.FromResult(Result.Success(new List<FileProfileModel> { firstVersion, secondVersion })));

        FileProfileDirectorySynchronizer synchronizer = new(
            configuration,
            fileProfileManager.Instance(),
            fileSystem,
            NullLogger<FileProfileDirectorySynchronizer>.Instance);

        Result result = await synchronizer.SyncAsync(CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        fileSystem.File.Exists(existingFile).ShouldBeTrue();
        fileSystem.Directory.Exists(Path.Combine(sharedDirectory, "inprogress")).ShouldBeTrue();
        fileSystem.Directory.Exists(Path.Combine(sharedDirectory, "processed")).ShouldBeTrue();
        fileSystem.Directory.Exists(Path.Combine(sharedDirectory, "failed")).ShouldBeTrue();
    }
}
