using System.Collections.Generic;
using FileProcessor.BusinessLogic.Managers;
using FileProcessor.DataTransferObjects;
using FileProcessor.Models;
using SimpleResults;

namespace FileProcessor.BusinessLogic.Tests;

using System;
using System.Threading;
using System.Threading.Tasks;
using Requests;
using Services;

public class DummyFileProcessorDomainService : IFileProcessorDomainService {

    public async Task<Result<Guid>> UploadFile(FileCommands.UploadFileCommand command,
                                               CancellationToken cancellationToken) =>
        Result.Success(Guid.NewGuid());

    public async Task<Result> ProcessUploadedFile(FileCommands.ProcessUploadedFileCommand command,
                                                  CancellationToken cancellationToken) => Result.Success();

    public async Task<Result> ProcessTransactionForFileLine(FileCommands.ProcessTransactionForFileLineCommand command,
                                                            CancellationToken cancellationToken) => Result.Success();
}

public class DummyFileProcessorManager : IFileProcessorManager {
    public async Task<Result<List<FileProfile>>> GetAllFileProfiles(CancellationToken cancellationToken) {
        return Result.Success();
    }

    public async Task<Result<FileProfile>> GetFileProfile(Guid fileProfileId,
                                                          CancellationToken cancellationToken) {
        return Result.Success();
    }

    public async Task<Result<List<Models.FileImportLog>>> GetFileImportLogs(Guid estateId,
                                                                            DateTime startDateTime,
                                                                            DateTime endDateTime,
                                                                            Guid? merchantId,
                                                                            CancellationToken cancellationToken) {
        return Result.Success();
    }

    public async Task<Result<Models.FileImportLog>> GetFileImportLog(Guid fileImportLogId,
                                                                     Guid estateId,
                                                                     Guid? merchantId,
                                                                     CancellationToken cancellationToken) {
        return Result.Success();
    }

    public async Task<Result<FileDetails>> GetFile(Guid fileId,
                                                   Guid estateId,
                                                   CancellationToken cancellationToken) {
        return Result.Success();
    }
}