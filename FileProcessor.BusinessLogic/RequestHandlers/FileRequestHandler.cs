using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FileProcessor.Models;
using SimpleResults;

namespace FileProcessor.BusinessLogic.RequestHandlers
{
    using MediatR;
    using System.Threading;
    using Requests;
    using Services;
    using FileProcessor.BusinessLogic.Managers;
    
    public class FileRequestHandler : IRequestHandler<FileCommands.ProcessTransactionForFileLineCommand,Result>,
                                      IRequestHandler<FileCommands.ProcessUploadedFileCommand, Result>,
                                      IRequestHandler<FileCommands.UploadFileCommand, Result<Guid>>,
                                      IRequestHandler<FileQueries.GetFileQuery, Result<FileDetails>>,
                                      IRequestHandler<FileQueries.GetImportLogsQuery, Result<List<Models.FileImportLog>>>,
                                      IRequestHandler<FileQueries.GetImportLogQuery, Result<Models.FileImportLog>>
    {
        private readonly IFileProcessorDomainService DomainService;
        private readonly IFileProcessorManager Manager;

        public FileRequestHandler(IFileProcessorDomainService domainService, IFileProcessorManager manager) {
            this.DomainService = domainService;
            this.Manager = manager;
        }
        
        public async Task<Result<Guid>> Handle(FileCommands.UploadFileCommand command,
                                               CancellationToken cancellationToken) {
            return await this.DomainService.UploadFile(command, cancellationToken);
        }

        public async Task<Result> Handle(FileCommands.ProcessUploadedFileCommand command, CancellationToken cancellationToken)
        {
            return await this.DomainService.ProcessUploadedFile(command, cancellationToken);
        }
        
        public async Task<Result> Handle(FileCommands.ProcessTransactionForFileLineCommand command,
                                         CancellationToken cancellationToken) {
            return await this.DomainService.ProcessTransactionForFileLine(command, cancellationToken);
        }

        public async Task<Result<FileDetails>> Handle(FileQueries.GetFileQuery query,
                                                      CancellationToken cancellationToken) {
            return await this.Manager.GetFile(query.FileId, query.EstateId, cancellationToken);
        }

        public async Task<Result<List<Models.FileImportLog>>> Handle(FileQueries.GetImportLogsQuery query,
                                                                     CancellationToken cancellationToken) {
            return await this.Manager.GetFileImportLogs(query.EstateId, query.StartDateTime, query.EndDateTime, query.MerchantId, cancellationToken);
        }

        public async Task<Result<Models.FileImportLog>> Handle(FileQueries.GetImportLogQuery query,
                                                               CancellationToken cancellationToken) {
            return await this.Manager.GetFileImportLog(query.FileImportLogId, query.EstateId, query.MerchantId, cancellationToken);
        }
    }
}
