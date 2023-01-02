using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace FileProcessor.BusinessLogic.Tests
{
    using System.Threading;
    using Moq;
    using RequestHandlers;
    using Services;
    using Shouldly;
    using Testing;

    public class FileRequestHandlerTests
    {
        private Mock<IFileProcessorDomainService> FileProcessorDomainService;
        private FileRequestHandler FileRequestHandler;

        public FileRequestHandlerTests() {
            this.FileProcessorDomainService = new Mock<IFileProcessorDomainService>();
            this.FileRequestHandler = new FileRequestHandler(this.FileProcessorDomainService.Object);
        }

        public async Task FileRequestHandler_HandleUploadFileRequest_RequestHandled() {
            Should.NotThrow(async () => {
                                await this.FileRequestHandler.Handle(TestData.UploadFileRequest, CancellationToken.None);
                            });
        }

        public async Task FileRequestHandler_ProcessUploadedFileRequest_RequestHandled()
        {
            Should.NotThrow(async () => {
                                await this.FileRequestHandler.Handle(TestData.ProcessUploadedFileRequest, CancellationToken.None);
                            });
        }

        public async Task FileRequestHandler_SafaricomTopupRequest_RequestHandled()
        {
            Should.NotThrow(async () => {
                                await this.FileRequestHandler.Handle(TestData.SafaricomTopupRequest, CancellationToken.None);
                            });
        }

        public async Task FileRequestHandler_ProcessTransactionForFileLineRequest_RequestHandled()
        {
            Should.NotThrow(async () => {
                                await this.FileRequestHandler.Handle(TestData.ProcessTransactionForFileLineRequest, CancellationToken.None);
                            });
        }

        public async Task FileRequestHandler_VoucherRequest_RequestHandled()
        {
            Should.NotThrow(async () => {
                                await this.FileRequestHandler.Handle(TestData.VoucherRequest, CancellationToken.None);
                            });
        }
    }
}
