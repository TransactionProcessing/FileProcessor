using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Threading;
using System.Threading.Tasks;
using FileProcessor.BusinessLogic.Managers;
using FileProcessor.BusinessLogic.Requests;
using FileProcessor.Models;
using MediatR;
using Shared.Logger;
using Shared.Results;
using SimpleResults;
using FileProfileModel = global::FileProcessor.Models.FileProfile;
using FileImportLogModel = global::FileProcessor.Models.FileImportLog;

namespace FileProcessor.BusinessLogic.Services;

public sealed class FileProfileDirectoryRecoveryService : IFileProfileDirectoryRecoveryService
{
    private const string InProgressDirectoryName = "inprogress";

    private readonly IFileProcessorManager FileProcessorManager;
    private readonly IMediator Mediator;
    private readonly IFileSystem FileSystem;

    public FileProfileDirectoryRecoveryService(
        IFileProcessorManager fileProcessorManager,
        IMediator mediator,
        IFileSystem fileSystem)
    {
        this.FileProcessorManager = fileProcessorManager;
        this.Mediator = mediator;
        this.FileSystem = fileSystem;
    }

    public async Task<Result> RecoverInProgressFilesAsync(CancellationToken cancellationToken)
    {
        Result<List<FileProfileModel>> fileProfilesResult = await this.FileProcessorManager.GetAllFileProfileVersions(cancellationToken);
        if (fileProfilesResult.IsFailed)
        {
            Shared.Logger.Logger.LogWarning($"Unable to load file profiles for in-progress recovery: {fileProfilesResult.Message}");
            return ResultHelpers.CreateFailure(fileProfilesResult);
        }

        HashSet<string> recoveredDirectories = new(GetPathComparer());
        foreach (FileProfileModel fileProfile in fileProfilesResult.Data)
        {
            string inProgressDirectory = Path.Combine(fileProfile.ListeningDirectory, InProgressDirectoryName);
            if (recoveredDirectories.Add(inProgressDirectory) == false)
            {
                continue;
            }

            if (this.FileSystem.Directory.Exists(inProgressDirectory) == false)
            {
                continue;
            }

            foreach (string filePath in this.FileSystem.Directory.GetFiles(inProgressDirectory))
            {
                await this.RecoverFileAsync(fileProfile, filePath, cancellationToken);
            }
        }

        return Result.Success();
    }

    private static StringComparer GetPathComparer()
    {
        return OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
    }

    private async Task RecoverFileAsync(FileProfileModel fileProfile, string filePath, CancellationToken cancellationToken)
    {
        string fileName = Path.GetFileNameWithoutExtension(filePath);
        if (TryParseIdentifiers(fileName, out Guid estateId, out Guid fileId) == false) {
            Shared.Logger.Logger.LogWarning("Skipping leftover in-progress file [{FileName}] because the name does not contain recoverable identifiers", filePath);
            return;
        }

        Result<FileDetails> fileResult = await this.FileProcessorManager.GetFile(fileId, estateId, cancellationToken);
        FileCommands.ProcessUploadedFileCommand command;

        if (fileResult.IsSuccess && fileResult.Data != null)
        {
            FileDetails fileDetails = fileResult.Data;
            command = new(
                fileDetails.EstateId,
                fileDetails.MerchantId,
                fileDetails.FileImportLogId,
                fileId,
                fileDetails.UserId,
                filePath,
                fileDetails.FileProfileId,
                 fileDetails.FileReceivedDateTime,
                 fileDetails.FileProfileVersionId);
        }
        else
        {
            Result<FileImportLogModel> importLogResult = await this.FileProcessorManager.GetFileImportLogForFile(
                fileId,
                estateId,
                cancellationToken);

            FileImportLogModel importLog = importLogResult.Data;
            ImportLogFile importLogFile = importLog?.Files?.Find(file => file.FileId == fileId);

            if (importLogResult.IsFailed || importLog == null || importLogFile == null) {
                Logger.LogWarning("Skipping leftover in-progress file [{FileName}] because file [{FileId}] could not be loaded from the file aggregate or import log read model", filePath, fileId);
                return;
            }

            command = new(
                estateId,
                importLogFile.MerchantId,
                importLog.FileImportLogId,
                fileId,
                importLogFile.UserId,
                filePath,
                importLogFile.FileProfileId,
                 importLogFile.UploadedDateTime == default ? importLog.FileImportLogDateTime : importLogFile.UploadedDateTime,
                 importLogFile.FileProfileVersionId);
        }

        Result result = await this.Mediator.Send(command, cancellationToken);
        if (result.IsFailed)
        {
            Logger.LogWarning("Recovery processing failed for leftover in-progress file [{FileName}]: {Message}", filePath, result.Message);
            return;
        }

        Logger.LogInformation("Recovered leftover in-progress file [{FileName}] for file profile [{FileProfileId}]", filePath, fileProfile.FileProfileId);
    }

    private static bool TryParseIdentifiers(string fileName, out Guid estateId, out Guid fileId)
    {
        estateId = Guid.Empty;
        fileId = Guid.Empty;

        string[] parts = fileName.Split('-', 2, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2)
        {
            return false;
        }

        return Guid.TryParse(parts[0], out estateId) && Guid.TryParse(parts[1], out fileId);
    }
}
