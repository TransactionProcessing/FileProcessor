using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileProcessor.BusinessLogic.Managers;
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
        private Mock<IFileProcessorManager> Manager;

        public FileRequestHandlerTests() {
            this.FileProcessorDomainService = new Mock<IFileProcessorDomainService>();
            this.Manager = new Mock<IFileProcessorManager>();
            this.FileRequestHandler = new FileRequestHandler(this.FileProcessorDomainService.Object, this.Manager.Object);
        }

        public async Task FileRequestHandler_HandleUploadFileRequest_RequestHandled() {
            Should.NotThrow(async () => {
                                await this.FileRequestHandler.Handle(TestData.UploadFileCommand, CancellationToken.None);
                            });
        }

        public async Task FileRequestHandler_ProcessUploadedFileRequest_RequestHandled()
        {
            Should.NotThrow(async () => {
                                await this.FileRequestHandler.Handle(TestData.ProcessUploadedFileCommand, CancellationToken.None);
                            });
        }

        public async Task FileRequestHandler_ProcessTransactionForFileLineRequest_RequestHandled()
        {
            Should.NotThrow(async () => {
                                await this.FileRequestHandler.Handle(TestData.ProcessTransactionForFileLineCommand, CancellationToken.None);
                            });
        }
    }
}
