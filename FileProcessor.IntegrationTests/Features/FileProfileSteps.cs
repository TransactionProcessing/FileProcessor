using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FileProcessor.DataTransferObjects.Requests;
using FileProcessor.IntegrationTests.Common;
using FileProcessor.Models;
using FileProcessorStepsHelper = FileProcessor.IntegrationTesting.Helpers.FileProcessor.IntegrationTests.Steps.FileProcessorSteps;
using Reqnroll;
using Shouldly;
using SimpleResults;

namespace FileProcessor.IntegrationTests.Features;

[Binding]
[Scope(Tag = "fileprofiles")]
public class FileProfileSteps
{
    private readonly TestingContext TestingContext;
    private readonly FileProcessorStepsHelper FileProcessorSteps;
    private readonly Dictionary<string, Guid> FileProfileIds = new(StringComparer.OrdinalIgnoreCase);

    public FileProfileSteps(TestingContext testingContext)
    {
        this.TestingContext = testingContext;
        this.FileProcessorSteps = new FileProcessorStepsHelper(testingContext.DockerHelper.FileProcessorClient);
    }

    [When(@"I create the following file profiles")]
    public async Task WhenICreateTheFollowingFileProfiles(DataTable table)
    {
        foreach (DataTableRow row in table.Rows)
        {
            string alias = row["Alias"];
            await this.CreateProfile(alias, this.BuildCreateRequest(row));
        }
    }

    [When(@"I update the following file profiles")]
    public async Task WhenIUpdateTheFollowingFileProfiles(DataTable table)
    {
        foreach (DataTableRow row in table.Rows)
        {
            string alias = row["Alias"];
            UpdateFileProfileRequest request = this.BuildUpdateRequest(row);
            await this.UpdateProfile(alias, request);
        }
    }

    [When(@"I try to create the following duplicate file profiles")]
    public async Task WhenITryToCreateTheFollowingDuplicateFileProfiles(DataTable table)
    {
        foreach (DataTableRow row in table.Rows)
        {
            string duplicateType = row["DuplicateType"];
            string basedOn = row["BasedOn"];
            FileProfile sourceProfile = await this.GetProfileByAlias(basedOn);

            CreateFileProfileRequest request = duplicateType.Equals("Name", StringComparison.OrdinalIgnoreCase)
                ? new CreateFileProfileRequest
                {
                    FileProfileId = Guid.NewGuid(),
                    Name = sourceProfile.Name,
                    ListeningDirectory = $"/tmp/{Guid.NewGuid():N}",
                    RequestType = $"{sourceProfile.RequestType}-duplicate",
                    OperatorName = sourceProfile.OperatorName,
                    LineTerminator = sourceProfile.LineTerminator,
                    FileFormatHandler = sourceProfile.FileFormatHandler
                }
                : new CreateFileProfileRequest
                {
                    FileProfileId = Guid.NewGuid(),
                    Name = $"{sourceProfile.Name}-duplicate",
                    ListeningDirectory = $"/tmp/{Guid.NewGuid():N}",
                    RequestType = sourceProfile.RequestType,
                    OperatorName = sourceProfile.OperatorName,
                    LineTerminator = sourceProfile.LineTerminator,
                    FileFormatHandler = sourceProfile.FileFormatHandler
                };

            Result<FileProfile> duplicateResult = await this.FileProcessorSteps.CreateFileProfile(this.TestingContext.AccessToken, request, CancellationToken.None);
            duplicateResult.IsFailed.ShouldBeTrue(duplicateResult.Message);
        }
    }

    [Then(@"the file profiles list should contain (.*) items")]
    public async Task ThenTheFileProfilesListShouldContainItems(int expectedCount)
    {
        Result<List<FileProfile>> fileProfilesResult = await this.FileProcessorSteps.GetFileProfiles(this.TestingContext.AccessToken, CancellationToken.None);

        fileProfilesResult.IsSuccess.ShouldBeTrue(fileProfilesResult.Message);
        fileProfilesResult.Data.ShouldNotBeNull();
        fileProfilesResult.Data.Count.ShouldBe(expectedCount);
    }

    [Then(@"the file profiles should have the following values")]
    public async Task ThenTheFileProfilesShouldHaveTheFollowingValues(DataTable table)
    {
        Result<List<FileProfile>> fileProfilesResult = await this.FileProcessorSteps.GetFileProfiles(this.TestingContext.AccessToken, CancellationToken.None);
        fileProfilesResult.IsSuccess.ShouldBeTrue(fileProfilesResult.Message);
        fileProfilesResult.Data.ShouldNotBeNull();

        foreach (DataTableRow row in table.Rows)
        {
            Guid fileProfileId = this.FileProfileIds[row["Alias"]];
            FileProfile fileProfile = fileProfilesResult.Data.SingleOrDefault(profile => profile.FileProfileId == fileProfileId);

            fileProfile.ShouldNotBeNull();
            fileProfile.Name.ShouldBe(row["Name"]);
            fileProfile.ListeningDirectory.ShouldBe(row["ListeningDirectory"]);
            fileProfile.RequestType.ShouldBe(row["RequestType"]);
            fileProfile.OperatorName.ShouldBe(row["OperatorName"]);
            fileProfile.LineTerminator.ShouldBe(row["LineTerminator"]);
            fileProfile.FileFormatHandler.ShouldBe(row["FileFormatHandler"]);
        }
    }

    private CreateFileProfileRequest BuildCreateRequest(DataTableRow row)
    {
        return new CreateFileProfileRequest
        {
            FileProfileId = Guid.Parse(row["FileProfileId"]),
            Name = row["Name"],
            ListeningDirectory = row["ListeningDirectory"],
            RequestType = row["RequestType"],
            OperatorName = row["OperatorName"],
            LineTerminator = row["LineTerminator"],
            FileFormatHandler = row["FileFormatHandler"]
        };
    }

    private async Task CreateProfile(string alias, CreateFileProfileRequest request)
    {
        Result<FileProfile> createResult = await this.FileProcessorSteps.CreateFileProfile(this.TestingContext.AccessToken, request, CancellationToken.None);

        createResult.IsSuccess.ShouldBeTrue(createResult.Message);
        createResult.Data.ShouldNotBeNull();

        this.FileProfileIds[alias] = createResult.Data.FileProfileId;
    }

    private UpdateFileProfileRequest BuildUpdateRequest(DataTableRow row)
    {
        return new UpdateFileProfileRequest
        {
            Name = row["Name"],
            ListeningDirectory = row["ListeningDirectory"],
            RequestType = row["RequestType"],
            OperatorName = row["OperatorName"],
            LineTerminator = row["LineTerminator"],
            FileFormatHandler = row["FileFormatHandler"]
        };
    }

    private async Task UpdateProfile(string alias, UpdateFileProfileRequest request)
    {
        Guid fileProfileId = this.FileProfileIds[alias];
        Result<FileProfile> updateResult = await this.FileProcessorSteps.UpdateFileProfile(this.TestingContext.AccessToken, fileProfileId, request, CancellationToken.None);

        updateResult.IsSuccess.ShouldBeTrue(updateResult.Message);
        updateResult.Data.ShouldNotBeNull();
    }

    private async Task<FileProfile> GetProfileByAlias(string alias)
    {
        Guid fileProfileId = this.FileProfileIds[alias];
        Result<FileProfile> result = await this.FileProcessorSteps.GetFileProfile(this.TestingContext.AccessToken, fileProfileId, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue(result.Message);
        result.Data.ShouldNotBeNull();
        return result.Data;
    }
}
