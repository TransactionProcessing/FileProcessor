using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;
using FileProcessor.BusinessLogic.Managers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Shared.Results;
using SimpleResults;
using FileProfileModel = global::FileProcessor.Models.FileProfile;

namespace FileProcessor.BusinessLogic.Services;

public sealed class FileProfileDirectorySynchronizer : IFileProfileDirectorySynchronizer
{
    private const string TemporaryFileLocationKey = "AppSettings:TemporaryFileLocation";
    private const string InProgressDirectoryName = "inprogress";

    private readonly IConfiguration Configuration;
    private readonly IFileProfileManager FileProfileManager;
    private readonly IFileSystem FileSystem;
    private readonly ILogger<FileProfileDirectorySynchronizer> Logger;

    public FileProfileDirectorySynchronizer(
        IConfiguration configuration,
        IFileProfileManager fileProfileManager,
        IFileSystem fileSystem,
        ILogger<FileProfileDirectorySynchronizer> logger)
    {
        this.Configuration = configuration;
        this.FileProfileManager = fileProfileManager;
        this.FileSystem = fileSystem;
        this.Logger = logger;
    }

    public async Task<Result> SyncAsync(CancellationToken cancellationToken)
    {
        string temporaryFileLocation = this.Configuration.GetValue<string>(TemporaryFileLocationKey);
        if (string.IsNullOrWhiteSpace(temporaryFileLocation))
        {
            return Result.Invalid("No temporary file location configured");
        }

        this.FileSystem.Directory.CreateDirectory(temporaryFileLocation);
        this.Logger.LogInformation("Ensured temporary file location exists at [{TemporaryFileLocation}]", temporaryFileLocation);

        Result<List<FileProfileModel>> fileProfilesResult = await this.FileProfileManager.GetAllFileProfiles(cancellationToken);
        if (fileProfilesResult.IsFailed)
        {
            this.Logger.LogWarning("Error getting file profiles {Message}", fileProfilesResult.Message);
            return ResultHelpers.CreateFailure(fileProfilesResult);
        }

        foreach (FileProfileModel fileProfile in fileProfilesResult.Data)
        {
            this.EnsureDirectory(Path.Combine(fileProfile.ListeningDirectory, InProgressDirectoryName), "in progress");
            this.EnsureDirectory(fileProfile.ProcessedDirectory, "processed");
            this.EnsureDirectory(fileProfile.FailedDirectory, "failed");
        }

        return Result.Success();
    }

    private void EnsureDirectory(string directoryPath, string directoryName)
    {
        this.FileSystem.Directory.CreateDirectory(directoryPath);
        this.Logger.LogInformation("Ensured {DirectoryName} directory exists at [{DirectoryPath}]", directoryName, directoryPath);
    }
}
