using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileProcessor.BusinessLogic.Managers;
using MediatR;
using SimpleResults;

namespace FileProcessor.BusinessLogic.Tests
{
    using System.Threading;
    using Imposter.Abstractions;
    using RequestHandlers;
    using Requests;
    using Services;
    using Shouldly;
    using Testing;
    using Xunit;

    public class FileRequestHandlerTests
    {
        private IFileProcessorDomainServiceImposter FileProcessorDomainService;
        private FileRequestHandler FileRequestHandler;
        private IFileProcessorManagerImposter Manager;

        public FileRequestHandlerTests() {
            this.FileProcessorDomainService = new IFileProcessorDomainServiceImposter();
            this.Manager = new IFileProcessorManagerImposter();
            this.FileRequestHandler = new FileRequestHandler(this.FileProcessorDomainService.Instance(), this.Manager.Instance());
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

        [Fact]
        public async Task FileRequestHandler_ReplayFileLineRequest_RequestIsDelegated()
        {
            FileCommands.ReplayFileLineCommand command = new(TestData.FileId, TestData.EstateId, TestData.LineNumber);
            this.FileProcessorDomainService.ReplayFileLine(Arg<FileCommands.ReplayFileLineCommand>.Any(), Arg<CancellationToken>.Any())
                .ReturnsAsync(Result.Success());

            Result result = await this.FileRequestHandler.Handle(command, CancellationToken.None);

            result.IsSuccess.ShouldBeTrue();
            this.FileProcessorDomainService.ReplayFileLine(command, CancellationToken.None).Called(Count.Once());
        }
    }
}
