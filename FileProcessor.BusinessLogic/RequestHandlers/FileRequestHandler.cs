using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileProcessor.BusinessLogic.RequestHandlers
{
    using System.IO;
    using System.IO.Abstractions;
    using System.Security.Cryptography;
    using MediatR;
    using System.Threading;
    using Common;
    using EstateManagement.Client;
    using EstateManagement.DataTransferObjects.Responses;
    using EventHandling;
    using FileAggregate;
    using FileFormatHandlers;
    using FileImportLogAggregate;
    using FIleProcessor.Models;
    using Managers;
    using Newtonsoft.Json;
    using Requests;
    using SecurityService.Client;
    using SecurityService.DataTransferObjects.Responses;
    using Services;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EventStore.Aggregate;
    using Shared.Exceptions;
    using Shared.General;
    using Shared.Logger;
    using TransactionProcessor.Client;
    using TransactionProcessor.DataTransferObjects;
    using Exception = System.Exception;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="UploadFileRequest" />
    /// <seealso cref="ProcessTransactionForFileLineRequest" />
    public class FileRequestHandler : IRequestHandler<UploadFileRequest,Guid>,
                                      IRequestHandler<ProcessUploadedFileRequest>,
                                      IRequestHandler<ProcessTransactionForFileLineRequest>
    {
        private readonly IFileProcessorDomainService DomainService;

        public FileRequestHandler(IFileProcessorDomainService domainService)
        {
            this.DomainService = domainService;
        }
        
        public async Task<Guid> Handle(UploadFileRequest request,
                                       CancellationToken cancellationToken) {
            return await this.DomainService.UploadFile(request, cancellationToken);
        }

        public async Task Handle(ProcessUploadedFileRequest request, CancellationToken cancellationToken)
        {
            await this.DomainService.ProcessUploadedFile(request, cancellationToken);
        }
        
        public async Task Handle(ProcessTransactionForFileLineRequest request,
                                       CancellationToken cancellationToken) {
            await this.DomainService.ProcessTransactionForFileLine(request, cancellationToken);
        }
    }
}
