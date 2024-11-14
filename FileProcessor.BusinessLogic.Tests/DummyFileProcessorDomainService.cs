using FileProcessor.DataTransferObjects;
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