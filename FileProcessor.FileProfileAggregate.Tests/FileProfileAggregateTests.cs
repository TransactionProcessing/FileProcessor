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
            TestData.SafaricomLineTerminator,
            TestData.SafaricomFileFormatHandler));

        Result secondCreate = aggregate.CreateProfile(this.CreateProfile(
            TestData.VoucherFileProfileId,
            TestData.VoucherProfileName,
            TestData.VoucherListeningDirectory,
            TestData.VoucherRequestType,
            TestData.VoucherOperatorIdentifier,
            TestData.VoucherLineTerminator,
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
            RequestType = "SafaricomTopupRequestUpdated"
        });

        Result updateOperatorName = aggregate.UpdateProfile(TestData.FileProfileId, new UpdateFileProfileRequest
        {
            OperatorName = "Safaricom Updated"
        });

        Result updateLineTerminator = aggregate.UpdateProfile(TestData.FileProfileId, new UpdateFileProfileRequest
        {
            LineTerminator = "\r\n"
        });

        Result updateFileFormatHandler = aggregate.UpdateProfile(TestData.FileProfileId, new UpdateFileProfileRequest
        {
            FileFormatHandler = "SafaricomUpdatedFileFormatHandler"
        });

        Result duplicateNameCreate = aggregate.CreateProfile(this.CreateProfile(
            Guid.Parse("33333333-3333-3333-3333-333333333333"),
            "Voucher Profile",
            "/home/txnproc/bulkfiles/duplicate-name",
            "GammaRequest",
            "Gamma Operator",
            "\n",
            "GammaHandler"));

        Result duplicateRequestTypeCreate = aggregate.CreateProfile(this.CreateProfile(
            Guid.Parse("44444444-4444-4444-4444-444444444444"),
            "Gamma Profile",
            "/home/txnproc/bulkfiles/duplicate-request",
            TestData.VoucherRequestType,
            "Gamma Operator",
            "\n",
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
        updatedProfile.RequestType.ShouldBe("SafaricomTopupRequestUpdated");
        updatedProfile.OperatorName.ShouldBe("Safaricom Updated");
        updatedProfile.LineTerminator.ShouldBe("\r\n");
        updatedProfile.FileFormatHandler.ShouldBe("SafaricomUpdatedFileFormatHandler");
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
            TestData.SafaricomLineTerminator,
            TestData.SafaricomFileFormatHandler));
        
        Result updateResult = aggregate.UpdateProfile(TestData.FileProfileId, new UpdateFileProfileRequest
        {
            Name = TestData.SafaricomProfileName,
            RequestType = TestData.SafaricomRequestType,
            ListeningDirectory = TestData.SafaricomListeningDirectory,
            OperatorName = TestData.SafaricomOperatorIdentifier,
            LineTerminator = TestData.SafaricomLineTerminator,
            FileFormatHandler = TestData.SafaricomFileFormatHandler
        });

        updateResult.IsSuccess.ShouldBeTrue();
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
            TestData.SafaricomLineTerminator,
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
    public void FileProfileAggregate_Create_WithDuplicateName_IsRejected()
    {
        FileProfileAggregateRoot aggregate = FileProfileAggregateRoot.Create();

        Result firstCreate = aggregate.CreateProfile(this.CreateProfile(
            TestData.FileProfileId,
            TestData.SafaricomProfileName,
            TestData.SafaricomListeningDirectory,
            TestData.SafaricomRequestType,
            TestData.SafaricomOperatorIdentifier,
            TestData.SafaricomLineTerminator,
            TestData.SafaricomFileFormatHandler));

        Result duplicateCreate = aggregate.CreateProfile(this.CreateProfile(
            TestData.SafaricomFileProfileId,
            TestData.SafaricomProfileName,
            "/tmp/other",
            "DifferentRequest",
            "Other",
            TestData.SafaricomLineTerminator,
            "OtherHandler"));

        firstCreate.IsSuccess.ShouldBeTrue();
        duplicateCreate.IsFailed.ShouldBeTrue();
        duplicateCreate.Status.ShouldBe(ResultStatus.Invalid);
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
            TestData.SafaricomLineTerminator,
            TestData.SafaricomFileFormatHandler));

        Result duplicateCreate = aggregate.CreateProfile(this.CreateProfile(
            TestData.SafaricomFileProfileId,
            "Different Name",
            "/tmp/other",
            TestData.SafaricomRequestType,
            "Other",
            TestData.SafaricomLineTerminator,
            "OtherHandler"));

        firstCreate.IsSuccess.ShouldBeTrue();
        duplicateCreate.IsFailed.ShouldBeTrue();
        duplicateCreate.Status.ShouldBe(ResultStatus.Invalid);
    }

    private CreateFileProfileRequest CreateProfile(Guid fileProfileId,
                                                   string name,
                                                   string listeningDirectory,
                                                   string requestType,
                                                   string operatorName,
                                                   string lineTerminator,
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
