using System;
using System.Threading.Tasks;
using SimpleResults;

namespace FileProcessor.BusinessLogic.Services
{
    using System.Threading;
    using FileProcessor.BusinessLogic.RequestHandlers;
    using MediatR;
    using Requests;

    public interface IFileProcessorDomainService
    {
        Task<Result<Guid>> UploadFile(FileCommands.UploadFileCommand command, CancellationToken cancellationToken);

        Task<Result> ProcessUploadedFile(FileCommands.ProcessUploadedFileCommand command,
                                         CancellationToken cancellationToken);
        
        Task<Result> ProcessTransactionForFileLine(FileCommands.ProcessTransactionForFileLineCommand command,
                                                 CancellationToken cancellationToken);
    }
}
