using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FileProcessor.BusinessLogic.Managers;
using FIleProcessor.Models;
using FileProcessor.Testing;
using MediatR;
using Microsoft.Extensions.Configuration;
using Moq;
using Shared.General;
using Shouldly;
using Xunit;

namespace FileProcessor.Tests
{
    public class FileProcessingWorkerTests
    {
        private readonly Mock<IFileProcessorManager> FileProcessingManager;
        private readonly Mock<IMediator> Mediator;
        private readonly MockFileSystem FileSystem;
        private readonly FileProcessingWorker FileProcessingWorker;

        public FileProcessingWorkerTests()
        {
            this.FileProcessingManager = new Mock<IFileProcessorManager>();
            this.Mediator = new Mock<IMediator>();
            this.FileSystem = new MockFileSystem();
            this.FileProcessingWorker =
                new FileProcessingWorker(this.FileProcessingManager.Object, this.Mediator.Object, this.FileSystem);
        }

        [Fact]
        public async Task FileProcessingWorker_StartAsync_IsStarted()
        {
            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);

            this.FileProcessingManager.Setup(f => f.GetAllFileProfiles(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<FileProfile>() {TestData.FileProfileSafaricom});

            await this.FileProcessingWorker.StartAsync(CancellationToken.None);

            this.FileProcessingWorker.IsRunning.ShouldBeTrue();
        }

        [Fact]
        public async Task FileProcessingWorker_StartAsync_FilesInProgress_FilesMovedAndIsStarted()
        {
            IConfigurationRoot configurationRoot = new ConfigurationBuilder().AddInMemoryCollection(TestData.DefaultAppSettings).Build();
            ConfigurationReader.Initialise(configurationRoot);

            this.FileProcessingManager.Setup(f => f.GetAllFileProfiles(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<FileProfile>() { TestData.FileProfileSafaricom });

            this.FileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom/inprogress");
            this.FileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom/processed");
            this.FileSystem.AddDirectory("home/txnproc/bulkfiles/safaricom/failed");

            this.FileSystem.AddFile(TestData.InProgressSafaricomFilePathWithName, new MockFileData("D,1,1,1"));
            CancellationToken c = new CancellationToken(true); // :| Do this to force Execute async to not process the files
            await this.FileProcessingWorker.StartAsync(c);

            this.FileProcessingWorker.IsRunning.ShouldBeTrue();
            IDirectoryInfo directoryInfo = this.FileSystem.DirectoryInfo.FromDirectoryName("home/txnproc/bulkfiles/safaricom");
            directoryInfo.GetFiles("*.*").Length.ShouldBe(1);
            
        }

        [Fact]
        public void FileProcessingWorker_StartAsync_NoFileProfiles_IsStarted()
        {

        }

    }
}
