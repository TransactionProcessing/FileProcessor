using System;
using System.Threading.Tasks;

namespace FileProcessor.BusinessLogic.Services
{
    using System.Threading;
    using FileProcessor.BusinessLogic.RequestHandlers;
    using MediatR;
    using Requests;

    public interface IFileProcessorDomainService
    {
        Task<Guid> UploadFile(UploadFileRequest request, CancellationToken cancellationToken);

        Task ProcessUploadedFile(ProcessUploadedFileRequest request,
                                 CancellationToken cancellationToken);

        //Task ProcessSafaricomTopup(SafaricomTopupRequest request,
        //                                 CancellationToken cancellationToken);

        //Task ProcessVoucher(VoucherRequest request, CancellationToken cancellationToken);

        Task ProcessTransactionForFileLine(ProcessTransactionForFileLineRequest request,
                                                 CancellationToken cancellationToken);
    }
}
