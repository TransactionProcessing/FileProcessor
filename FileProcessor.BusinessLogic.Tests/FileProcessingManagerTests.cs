using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileProcessor.BusinessLogic.Tests
{
    using System.Threading;
    using Managers;
    using Shouldly;
    using Testing;
    using Xunit;

    public class FileProcessingManagerTests
    {
        [Fact]
        public async Task FileProcessingManager_GetAllFileProfiles_AllFileProfilesReturned()
        {
            var fileProfiles = TestData.FileProfiles;
            FileProcessorManager manager = new FileProcessorManager(fileProfiles);

            var allFileProfiles = await manager.GetAllFileProfiles(CancellationToken.None);
            allFileProfiles.ShouldNotBeNull();
            allFileProfiles.ShouldNotBeEmpty();
        }

        [Fact]
        public async Task FileProcessingManager_GetFileProfile_FIleProfileReturned()
        {
            var fileProfiles = TestData.FileProfiles;
            FileProcessorManager manager = new FileProcessorManager(fileProfiles);

            var fileProfile = await manager.GetFileProfile(TestData.SafaricomFileProfileId, CancellationToken.None);
            fileProfile.ShouldNotBeNull();
            fileProfile.FileProfileId.ShouldBe(TestData.SafaricomFileProfileId);
        }
    }
}
