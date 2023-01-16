namespace FileProcessor.BusinessLogic.Tests;

using System;
using System.Threading;
using System.Threading.Tasks;
using Requests;
using Services;

public class DummyFileProcessorDomainService : IFileProcessorDomainService
{
    public async Task<Guid> UploadFile(UploadFileRequest request,
                                       CancellationToken cancellationToken) {
        return Guid.NewGuid();
    }

    public async Task ProcessUploadedFile(ProcessUploadedFileRequest request,
                                          CancellationToken cancellationToken) {
    }

    public async Task ProcessSafaricomTopup(SafaricomTopupRequest request,
                                            CancellationToken cancellationToken) {
    }

    public async Task ProcessVoucher(VoucherRequest request,
                                     CancellationToken cancellationToken) {
    }

    public async Task ProcessTransactionForFileLine(ProcessTransactionForFileLineRequest request,
                                                    CancellationToken cancellationToken) {
    }
}