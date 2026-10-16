using System;
using System.Collections.Generic;
using System.Linq;
using FileProcessor.DataTransferObjects.Requests;
using FileProcessor.FileProfileAggregate;
using FileProcessor.Testing;
using Shouldly;
using SimpleResults;
using Xunit;
using FileProfileAggregateRoot = FileProcessor.FileProfileAggregate.FileProfileAggregate;

namespace FileProcessor.FileProfileAggregate.Tests;

public class FileProfileAggregateTests
{
    [Fact]
    public void FileProfileAggregate_CreateUpdateAndUniquenessFlow_WorksAsExpected()
    {
        FileProfileAggregateRoot aggregate = FileProfileAggregateRoot.Create();

        Result firstCreate = aggregate.CreateProfile(this.CreateProfile(
            TestData.FileProfileId,
            TestData.SafaricomProfileName,
            TestData.SafaricomListeningDirectory,
            TestData.SafaricomRequestType,
            TestData.SafaricomOperatorIdentifier,
            LineTerminatorType.LineFeed,
            TestData.SafaricomFileFormatHandler));

        Result secondCreate = aggregate.CreateProfile(this.CreateProfile(
            TestData.VoucherFileProfileId,
            TestData.VoucherProfileName,
            TestData.VoucherListeningDirectory,
            TestData.VoucherRequestType,
            TestData.VoucherOperatorIdentifier,
            LineTerminatorType.LineFeed,
            TestData.VoucherFileFormatHandler));

        Result updateName = aggregate.UpdateProfile(TestData.FileProfileId, new UpdateFileProfileRequest
        {
            Name = "Safaricom Profile Updated"
        });

        Result updateListeningDirectory = aggregate.UpdateProfile(TestData.FileProfileId, new UpdateFileProfileRequest
        {
            ListeningDirectory = "/home/txnproc/bulkfiles/safaricom-updated"
        });

        Result updateRequestType = aggregate.UpdateProfile(TestData.FileProfileId, new UpdateFileProfileRequest
        {
            RequestType = TestData.SafaricomRequestType
        });

        Result updateOperatorName = aggregate.UpdateProfile(TestData.FileProfileId, new UpdateFileProfileRequest
        {
            OperatorName = "Safaricom Updated"
        });

        Result updateLineTerminator = aggregate.UpdateProfile(TestData.FileProfileId, new UpdateFileProfileRequest
        {
            LineTerminator = LineTerminatorType.CarriageReturnLineFeed
        });

        Result updateFileFormatHandler = aggregate.UpdateProfile(TestData.FileProfileId, new UpdateFileProfileRequest
        {
            FileFormatHandler = TestData.VoucherFileFormatHandler
        });

        Result duplicateNameCreate = aggregate.CreateProfile(this.CreateProfile(
            Guid.Parse("33333333-3333-3333-3333-333333333333"),
            "Voucher Profile",
            "/home/txnproc/bulkfiles/duplicate-name",
            "GammaRequest",
            "Gamma Operator",
            LineTerminatorType.LineFeed,
            "GammaHandler"));

        Result duplicateRequestTypeCreate = aggregate.CreateProfile(this.CreateProfile(
            Guid.Parse("44444444-4444-4444-4444-444444444444"),
            "Gamma Profile",
            "/home/txnproc/bulkfiles/duplicate-request",
            TestData.VoucherRequestType,
            "Gamma Operator",
            LineTerminatorType.LineFeed,
            "GammaHandler"));

        List<FileProcessor.Models.FileProfile> fileProfiles = aggregate.GetAllProfiles();
        FileProcessor.Models.FileProfile updatedProfile = aggregate.GetProfile(TestData.FileProfileId);
        FileProcessor.Models.FileProfile secondProfile = aggregate.GetProfile(TestData.SafaricomFileProfileId);

        firstCreate.IsSuccess.ShouldBeTrue();
        secondCreate.IsSuccess.ShouldBeTrue();
        updateName.IsSuccess.ShouldBeTrue();
        updateListeningDirectory.IsSuccess.ShouldBeTrue();
        updateRequestType.IsSuccess.ShouldBeTrue();
        updateOperatorName.IsSuccess.ShouldBeTrue();
        updateLineTerminator.IsSuccess.ShouldBeTrue();
        updateFileFormatHandler.IsSuccess.ShouldBeTrue();
        duplicateNameCreate.IsFailed.ShouldBeTrue();
        duplicateRequestTypeCreate.IsFailed.ShouldBeTrue();
        fileProfiles.Count.ShouldBe(2);
        fileProfiles.Select(profile => profile.Name).ShouldContain("Safaricom Profile Updated");
        fileProfiles.Select(profile => profile.Name).ShouldContain(TestData.VoucherProfileName);

        updatedProfile.ShouldNotBeNull();
        updatedProfile.Name.ShouldBe("Safaricom Profile Updated");
        updatedProfile.ListeningDirectory.ShouldBe("/home/txnproc/bulkfiles/safaricom-updated");
        updatedProfile.RequestType.ShouldBe(TestData.SafaricomRequestType);
        updatedProfile.OperatorName.ShouldBe("Safaricom Updated");
        updatedProfile.LineTerminator.ShouldBe("\r\n");
        updatedProfile.FileFormatHandler.ShouldBe(TestData.VoucherFileFormatHandler);
        updatedProfile.ProcessedDirectory.ShouldBe("/home/txnproc/bulkfiles/safaricom-updated/processed");
        updatedProfile.FailedDirectory.ShouldBe("/home/txnproc/bulkfiles/safaricom-updated/failed");

        secondProfile.ShouldNotBeNull();
        secondProfile.Name.ShouldBe(TestData.VoucherProfileName);
        secondProfile.RequestType.ShouldBe(TestData.VoucherRequestType);
        secondProfile.ListeningDirectory.ShouldBe(TestData.VoucherListeningDirectory);
    }

    [Fact]
    public void FileProfileAggregate_Update_WithNoEffectiveChanges_DoesNotChangeAppliedEventCount()
    {
        FileProfileAggregateRoot aggregate = FileProfileAggregateRoot.Create();
        aggregate.CreateProfile(this.CreateProfile(
            TestData.FileProfileId,
            TestData.SafaricomProfileName,
            TestData.SafaricomListeningDirectory,
            TestData.SafaricomRequestType,
            TestData.SafaricomOperatorIdentifier,
            LineTerminatorType.LineFeed,
            TestData.SafaricomFileFormatHandler));
        
        Result updateResult = aggregate.UpdateProfile(TestData.FileProfileId, new UpdateFileProfileRequest
        {
            Name = TestData.SafaricomProfileName,
            RequestType = TestData.SafaricomRequestType,
            ListeningDirectory = TestData.SafaricomListeningDirectory,
            OperatorName = TestData.SafaricomOperatorIdentifier,
            LineTerminator = LineTerminatorType.LineFeed,
            FileFormatHandler = TestData.SafaricomFileFormatHandler
        });

        updateResult.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void FileProfileAggregate_Archive_RetainsHistoricalVersionsButHidesActiveProfile()
    {
        FileProfileAggregateRoot aggregate = FileProfileAggregateRoot.Create();
        aggregate.CreateProfile(this.CreateProfile(
            TestData.FileProfileId,
            TestData.SafaricomProfileName,
            TestData.SafaricomListeningDirectory,
            TestData.SafaricomRequestType,
            TestData.SafaricomOperatorIdentifier,
            LineTerminatorType.LineFeed,
            TestData.SafaricomFileFormatHandler));

        FileProcessor.Models.FileProfile originalVersion = aggregate.GetProfile(TestData.FileProfileId);
        aggregate.UpdateProfile(TestData.FileProfileId, new UpdateFileProfileRequest
        {
            FileFormatHandler = TestData.VoucherFileFormatHandler
        });

        Result archiveResult = aggregate.ArchiveProfile(TestData.FileProfileId);

        archiveResult.IsSuccess.ShouldBeTrue();
        aggregate.GetProfile(TestData.FileProfileId).ShouldBeNull();
        List<FileProcessor.Models.FileProfile> versions = aggregate.GetProfileVersions(TestData.FileProfileId);
        versions.Count.ShouldBe(2);
        versions[0].FileProfileVersion.ShouldBe(1);
        versions[1].FileProfileVersion.ShouldBe(2);
        aggregate.GetProfileVersion(TestData.FileProfileId, originalVersion.FileProfileVersionId)
                 .FileFormatHandler.ShouldBe(TestData.SafaricomFileFormatHandler);
    }

    [Fact]
    public void FileProfileAggregate_Archive_WhenAlreadyArchived_IsIdempotent()
    {
        FileProfileAggregateRoot aggregate = FileProfileAggregateRoot.Create();
        aggregate.CreateProfile(this.CreateProfile(
            TestData.FileProfileId,
            TestData.SafaricomProfileName,
            TestData.SafaricomListeningDirectory,
            TestData.SafaricomRequestType,
            TestData.SafaricomOperatorIdentifier,
            LineTerminatorType.LineFeed,
            TestData.SafaricomFileFormatHandler));

        Result firstArchive = aggregate.ArchiveProfile(TestData.FileProfileId);
        Result secondArchive = aggregate.ArchiveProfile(TestData.FileProfileId);

        firstArchive.IsSuccess.ShouldBeTrue();
        secondArchive.IsSuccess.ShouldBeTrue();
        aggregate.ActiveProfileCount.ShouldBe(0);
        aggregate.GetProfileVersions(TestData.FileProfileId).Count.ShouldBe(1);
    }

    [Fact]
    public void FileProfileAggregate_GetAllProfileVersions_IncludesArchivedProfilesAndRetainsDirectories()
    {
        FileProfileAggregateRoot aggregate = FileProfileAggregateRoot.Create();
        aggregate.CreateProfile(this.CreateProfile(
            TestData.FileProfileId,
            TestData.SafaricomProfileName,
            TestData.SafaricomListeningDirectory,
            TestData.SafaricomRequestType,
            TestData.SafaricomOperatorIdentifier,
            LineTerminatorType.LineFeed,
            TestData.SafaricomFileFormatHandler));

        aggregate.UpdateProfile(TestData.FileProfileId, new UpdateFileProfileRequest
        {
            ListeningDirectory = "/home/txnproc/bulkfiles/safaricom-v2"
        });
        aggregate.ArchiveProfile(TestData.FileProfileId);

        List<FileProcessor.Models.FileProfile> versions = aggregate.GetAllProfileVersions();

        versions.Count.ShouldBe(2);
        versions[0].ListeningDirectory.ShouldBe(TestData.SafaricomListeningDirectory);
        versions[0].ProcessedDirectory.ShouldBe($"{TestData.SafaricomListeningDirectory}/processed");
        versions[0].FailedDirectory.ShouldBe($"{TestData.SafaricomListeningDirectory}/failed");
        versions[1].ListeningDirectory.ShouldBe("/home/txnproc/bulkfiles/safaricom-v2");
    }

    [Fact]
    public void FileProfileAggregate_Update_ListeningDirectoryUpdatesDerivedDirectories()
    {
        FileProfileAggregateRoot aggregate = FileProfileAggregateRoot.Create();
        aggregate.CreateProfile(this.CreateProfile(
            TestData.FileProfileId,
            TestData.SafaricomProfileName,
            TestData.SafaricomListeningDirectory,
            TestData.SafaricomRequestType,
            TestData.SafaricomOperatorIdentifier,
            LineTerminatorType.LineFeed,
            TestData.SafaricomFileFormatHandler));

        Result updateResult = aggregate.UpdateProfile(TestData.FileProfileId, new UpdateFileProfileRequest
        {
            ListeningDirectory = "/home/txnproc/bulkfiles/new-safaricom"
        });

        FileProcessor.Models.FileProfile updatedProfile = aggregate.GetProfile(TestData.FileProfileId);

        updateResult.IsSuccess.ShouldBeTrue();
        updatedProfile.ShouldNotBeNull();
        updatedProfile.ListeningDirectory.ShouldBe("/home/txnproc/bulkfiles/new-safaricom");
        updatedProfile.ProcessedDirectory.ShouldBe("/home/txnproc/bulkfiles/new-safaricom/processed");
        updatedProfile.FailedDirectory.ShouldBe("/home/txnproc/bulkfiles/new-safaricom/failed");
    }

    [Fact]
    public void FileProfileAggregate_Create_CreatesInitialImmutableVersion()
    {
        FileProfileAggregateRoot aggregate = FileProfileAggregateRoot.Create();

        Result createResult = aggregate.CreateProfile(this.CreateProfile(
            TestData.FileProfileId,
            TestData.SafaricomProfileName,
            TestData.SafaricomListeningDirectory,
            TestData.SafaricomRequestType,
            TestData.SafaricomOperatorIdentifier,
            LineTerminatorType.LineFeed,
            TestData.SafaricomFileFormatHandler));

        FileProcessor.Models.FileProfile profile = aggregate.GetProfile(TestData.FileProfileId);
        List<FileProcessor.Models.FileProfile> versions = aggregate.GetProfileVersions(TestData.FileProfileId);

        createResult.IsSuccess.ShouldBeTrue();
        profile.FileProfileVersionId.ShouldNotBe(Guid.Empty);
        profile.FileProfileVersion.ShouldBe(1);
        versions.Count.ShouldBe(1);
        versions[0].FileProfileVersionId.ShouldBe(profile.FileProfileVersionId);
    }

    [Fact]
    public void FileProfileAggregate_Update_CreatesNewVersionAndPreservesPreviousVersion()
    {
        FileProfileAggregateRoot aggregate = FileProfileAggregateRoot.Create();
        aggregate.CreateProfile(this.CreateProfile(
            TestData.FileProfileId,
            TestData.SafaricomProfileName,
            TestData.SafaricomListeningDirectory,
            TestData.SafaricomRequestType,
            TestData.SafaricomOperatorIdentifier,
            LineTerminatorType.LineFeed,
            TestData.SafaricomFileFormatHandler));

        FileProcessor.Models.FileProfile originalProfile = aggregate.GetProfile(TestData.FileProfileId);

        Result updateResult = aggregate.UpdateProfile(TestData.FileProfileId, new UpdateFileProfileRequest
        {
            FileFormatHandler = TestData.VoucherFileFormatHandler
        });

        FileProcessor.Models.FileProfile updatedProfile = aggregate.GetProfile(TestData.FileProfileId);
        FileProcessor.Models.FileProfile historicalProfile = aggregate.GetProfileVersion(
            TestData.FileProfileId,
            originalProfile.FileProfileVersionId);
        List<FileProcessor.Models.FileProfile> versions = aggregate.GetProfileVersions(TestData.FileProfileId);

        updateResult.IsSuccess.ShouldBeTrue();
        updatedProfile.FileProfileVersionId.ShouldNotBe(originalProfile.FileProfileVersionId);
        updatedProfile.FileProfileVersion.ShouldBe(2);
        updatedProfile.FileFormatHandler.ShouldBe(TestData.VoucherFileFormatHandler);
        historicalProfile.FileProfileVersionId.ShouldBe(originalProfile.FileProfileVersionId);
        historicalProfile.FileProfileVersion.ShouldBe(1);
        historicalProfile.FileFormatHandler.ShouldBe(TestData.SafaricomFileFormatHandler);
        versions.Count.ShouldBe(2);
    }

    [Fact]
    public void FileProfileAggregate_ReplayingLegacyUpdateEvents_UpdatesInitialVersionSnapshot()
    {
        FileProfileAggregateRoot aggregate = FileProfileAggregateRoot.Create();
        Guid versionId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

        aggregate.PlayEvent(new FileProcessor.FileProfile.DomainEvents.FileProfileCreatedEvent(
            FileProfileAggregateRoot.FileProfileCollectionId,
            TestData.FileProfileId,
            TestData.SafaricomProfileName,
            TestData.SafaricomListeningDirectory,
            TestData.SafaricomRequestType,
            TestData.SafaricomOperatorIdentifier,
            "\n",
            TestData.SafaricomFileFormatHandler,
            versionId,
            1));
        aggregate.PlayEvent(new FileProcessor.FileProfile.DomainEvents.FileProfileFileFormatHandlerUpdatedEvent(
            FileProfileAggregateRoot.FileProfileCollectionId,
            TestData.FileProfileId,
            "LegacyUpdatedHandler"));

        FileProcessor.Models.FileProfile profile = aggregate.GetProfile(TestData.FileProfileId);
        FileProcessor.Models.FileProfile version = aggregate.GetProfileVersion(TestData.FileProfileId, versionId);

        profile.FileFormatHandler.ShouldBe("LegacyUpdatedHandler");
        version.FileFormatHandler.ShouldBe("LegacyUpdatedHandler");
    }

    [Fact]
    public void FileProfileAggregate_Create_WithDuplicateName_IsRejected()
    {
        FileProfileAggregateRoot aggregate = FileProfileAggregateRoot.Create();

        Result firstCreate = aggregate.CreateProfile(this.CreateProfile(
            TestData.FileProfileId,
            TestData.SafaricomProfileName,
            TestData.SafaricomListeningDirectory,
            TestData.SafaricomRequestType,
            TestData.SafaricomOperatorIdentifier,
            LineTerminatorType.LineFeed,
            TestData.SafaricomFileFormatHandler));

        Result duplicateCreate = aggregate.CreateProfile(this.CreateProfile(
            TestData.SafaricomFileProfileId,
            TestData.SafaricomProfileName,
            "/tmp/other",
            "DifferentRequest",
            "Other",
            LineTerminatorType.LineFeed,
            "OtherHandler"));

        firstCreate.IsSuccess.ShouldBeTrue();
        duplicateCreate.IsFailed.ShouldBeTrue();
        duplicateCreate.Status.ShouldBe(ResultStatus.Invalid);
    }

    [Theory]
    [InlineData(null)]
    public void FileProfileAggregate_Create_WithInvalidTerminator_IsRejected(LineTerminatorType? lineTerminator)
    {
        FileProfileAggregateRoot aggregate = FileProfileAggregateRoot.Create();

        Result firstCreate = aggregate.CreateProfile(this.CreateProfile(
            TestData.FileProfileId,
            TestData.SafaricomProfileName,
            TestData.SafaricomListeningDirectory,
            TestData.SafaricomRequestType,
            TestData.SafaricomOperatorIdentifier,
            lineTerminator,
            TestData.SafaricomFileFormatHandler));

        firstCreate.IsFailed.ShouldBeTrue();
        firstCreate.Status.ShouldBe(ResultStatus.Invalid);
    }

    [Fact]
    public void FileProfileAggregate_Create_WithUnknownTerminator_IsRejected()
    {
        FileProfileAggregateRoot aggregate = FileProfileAggregateRoot.Create();

        Result firstCreate = aggregate.CreateProfile(this.CreateProfile(
            TestData.FileProfileId,
            TestData.SafaricomProfileName,
            TestData.SafaricomListeningDirectory,
            TestData.SafaricomRequestType,
            TestData.SafaricomOperatorIdentifier,
            (LineTerminatorType?)99,
            TestData.SafaricomFileFormatHandler));

        firstCreate.IsFailed.ShouldBeTrue();
        firstCreate.Status.ShouldBe(ResultStatus.Invalid);
    }

    [Fact]
    public void FileProfileAggregate_Create_WithDuplicateRequestType_IsRejected()
    {
        FileProfileAggregateRoot aggregate = FileProfileAggregateRoot.Create();

        Result firstCreate = aggregate.CreateProfile(this.CreateProfile(
            TestData.FileProfileId,
            TestData.SafaricomProfileName,
            TestData.SafaricomListeningDirectory,
            TestData.SafaricomRequestType,
            TestData.SafaricomOperatorIdentifier,
            LineTerminatorType.LineFeed,
            TestData.SafaricomFileFormatHandler));

        Result duplicateCreate = aggregate.CreateProfile(this.CreateProfile(
            TestData.SafaricomFileProfileId,
            "Different Name",
            "/tmp/other",
            TestData.SafaricomRequestType,
            "Other",
            LineTerminatorType.LineFeed,
            "OtherHandler"));

        firstCreate.IsSuccess.ShouldBeTrue();
        duplicateCreate.IsFailed.ShouldBeTrue();
        duplicateCreate.Status.ShouldBe(ResultStatus.Invalid);
    }

    [Fact]
    public void FileProfileAggregate_Create_WithUnsupportedProcessingConfiguration_IsRejected()
    {
        FileProfileAggregateRoot aggregate = FileProfileAggregateRoot.Create();

        Result result = aggregate.CreateProfile(this.CreateProfile(
            TestData.FileProfileId,
            TestData.SafaricomProfileName,
            TestData.SafaricomListeningDirectory,
            "UnsupportedRequestType",
            TestData.SafaricomOperatorIdentifier,
            LineTerminatorType.LineFeed,
            "UnsupportedFileFormatHandler"));

        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.Invalid);
        result.Errors.ShouldContain(error => error.Contains("Unsupported request type"));
        result.Errors.ShouldContain(error => error.Contains("Unsupported file format handler"));
    }

    [Fact]
    public void FileProfileAggregate_Update_WithUnsupportedProcessingConfiguration_IsRejected()
    {
        FileProfileAggregateRoot aggregate = FileProfileAggregateRoot.Create();
        aggregate.CreateProfile(this.CreateProfile(
            TestData.FileProfileId,
            TestData.SafaricomProfileName,
            TestData.SafaricomListeningDirectory,
            TestData.SafaricomRequestType,
            TestData.SafaricomOperatorIdentifier,
            LineTerminatorType.LineFeed,
            TestData.SafaricomFileFormatHandler));

        Result result = aggregate.UpdateProfile(TestData.FileProfileId, new UpdateFileProfileRequest
        {
            RequestType = "UnsupportedRequestType",
            FileFormatHandler = "UnsupportedFileFormatHandler"
        });

        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.Invalid);
        result.Errors.ShouldContain(error => error.Contains("Unsupported request type"));
        result.Errors.ShouldContain(error => error.Contains("Unsupported file format handler"));
    }

    private CreateFileProfileRequest CreateProfile(Guid fileProfileId,
                                                   string name,
                                                   string listeningDirectory,
                                                   string requestType,
                                                   string operatorName,
                                                   LineTerminatorType? lineTerminator,
                                                   string fileFormatHandler)
    {
        return new CreateFileProfileRequest
        {
            FileProfileId = fileProfileId,
            Name = name,
            ListeningDirectory = listeningDirectory,
            RequestType = requestType,
            OperatorName = operatorName,
            LineTerminator = lineTerminator,
            FileFormatHandler = fileFormatHandler
        };
    }
}
