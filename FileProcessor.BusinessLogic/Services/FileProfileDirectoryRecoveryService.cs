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
        Result<List<FileProfileModel>> fileProfilesResult = await this.FileProcessorManager.GetAllFileProfiles(cancellationToken);
        if (fileProfilesResult.IsFailed)
        {
            Shared.Logger.Logger.LogWarning($"Unable to load file profiles for in-progress recovery: {fileProfilesResult.Message}");
            return ResultHelpers.CreateFailure(fileProfilesResult);
        }

        foreach (FileProfileModel fileProfile in fileProfilesResult.Data)
        {
            string inProgressDirectory = Path.Combine(fileProfile.ListeningDirectory, InProgressDirectoryName);
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

    private async Task RecoverFileAsync(FileProfileModel fileProfile, string filePath, CancellationToken cancellationToken)
    {
        string fileName = Path.GetFileNameWithoutExtension(filePath);
        if (TryParseIdentifiers(fileName, out Guid estateId, out Guid fileId) == false)
        {
            Shared.Logger.Logger.LogWarning($"Skipping leftover in-progress file [{filePath}] because the name does not contain recoverable identifiers");
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
                fileDetails.FileReceivedDateTime);
        }
        else
        {
            Result<List<FileImportLogModel>> importLogsResult = await this.FileProcessorManager.GetFileImportLogs(
                estateId,
                DateTime.MinValue,
                DateTime.MaxValue,
                null,
                cancellationToken);

            ImportLogFile importLogFile = null;
            FileImportLogModel importLog = null;
            if (importLogsResult.IsSuccess && importLogsResult.Data != null)
            {
                foreach (FileImportLogModel candidate in importLogsResult.Data)
                {
                    importLogFile = candidate.Files?.Find(file => file.FileId == fileId);
                    if (importLogFile != null)
                    {
                        importLog = candidate;
                        break;
                    }
                }
            }

            if (importLog == null || importLogFile == null)
            {
                Shared.Logger.Logger.LogWarning($"Skipping leftover in-progress file [{filePath}] because file [{fileId}] could not be loaded from the file aggregate or import log read model");
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
                importLogFile.UploadedDateTime == default ? importLog.FileImportLogDateTime : importLogFile.UploadedDateTime);
        }

        Result result = await this.Mediator.Send(command, cancellationToken);
        if (result.IsFailed)
        {
            Shared.Logger.Logger.LogWarning($"Recovery processing failed for leftover in-progress file [{filePath}]: {result.Message}");
            return;
        }

        Shared.Logger.Logger.LogInformation($"Recovered leftover in-progress file [{filePath}] for file profile [{fileProfile.FileProfileId}]");
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
