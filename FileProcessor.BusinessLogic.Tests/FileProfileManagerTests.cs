using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FileProcessor.BusinessLogic.Managers;
using FileProcessor.DataTransferObjects.Requests;
using FileProcessor.FileProfileAggregate;
using FileProcessor.Testing;
using Imposter.Abstractions;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EventStore.Aggregate;
using Shouldly;
using SimpleResults;
using Xunit;
using FileProfileAggregateRoot = FileProcessor.FileProfileAggregate.FileProfileAggregate;
using FileProfileModel = FileProcessor.Models.FileProfile;

namespace FileProcessor.BusinessLogic.Tests;

public class FileProfileManagerTests
{
    [Fact]
    public async Task GetFileProfileVersions_WhenProfileIsArchived_ReturnsHistoricalVersions()
    {
        FileProfileAggregateRoot aggregate = this.CreateArchivedAggregate();
        IAggregateRepositoryImposter<FileProfileAggregateRoot, DomainEvent> repository = new();
        repository.GetLatestVersion(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                  .ReturnsAsync(Result.Success(aggregate));
        FileProfileManager manager = new(repository.Instance());

        Result<List<FileProfileModel>> result = await manager.GetFileProfileVersions(TestData.FileProfileId, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Data.Count.ShouldBe(2);
        result.Data[0].FileProfileVersion.ShouldBe(1);
        result.Data[1].FileProfileVersion.ShouldBe(2);
    }

    [Fact]
    public async Task GetFileProfileVersion_WhenVersionDoesNotExist_ReturnsActionableNotFound()
    {
        FileProfileAggregateRoot aggregate = this.CreateArchivedAggregate();
        IAggregateRepositoryImposter<FileProfileAggregateRoot, DomainEvent> repository = new();
        repository.GetLatestVersion(Arg<Guid>.Any(), Arg<CancellationToken>.Any())
                  .ReturnsAsync(Result.Success(aggregate));
        FileProfileManager manager = new(repository.Instance());
        Guid missingVersionId = Guid.NewGuid();

        Result<FileProfileModel> result = await manager.GetFileProfileVersion(TestData.FileProfileId,
                                                                                missingVersionId,
                                                                                CancellationToken.None);

        result.IsFailed.ShouldBeTrue();
        result.Status.ShouldBe(ResultStatus.NotFound);
        result.Message.ShouldContain(TestData.FileProfileId.ToString());
        result.Message.ShouldContain(missingVersionId.ToString());
    }

    private FileProfileAggregateRoot CreateArchivedAggregate()
    {
        FileProfileAggregateRoot aggregate = FileProfileAggregateRoot.Create();
        aggregate.CreateProfile(new CreateFileProfileRequest
        {
            FileProfileId = TestData.FileProfileId,
            Name = TestData.SafaricomProfileName,
            ListeningDirectory = TestData.SafaricomListeningDirectory,
            RequestType = TestData.SafaricomRequestType,
            OperatorName = TestData.SafaricomOperatorIdentifier,
            LineTerminator = LineTerminatorType.LineFeed,
            FileFormatHandler = TestData.SafaricomFileFormatHandler
        });
        aggregate.UpdateProfile(TestData.FileProfileId, new UpdateFileProfileRequest
        {
            FileFormatHandler = "UpdatedHandler"
        });
        aggregate.ArchiveProfile(TestData.FileProfileId);
        return aggregate;
    }
}
