using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FileProcessor.DataTransferObjects.Requests;
using FileProcessor.Models;
using FileProcessor.FileProfileAggregate;
using TransactionProcessing.Shared.DomainDrivenDesign.EventSourcing;
using TransactionProcessing.Shared.EventStore.Aggregate;
using TransactionProcessing.UnionResults;
using FileProfileAggregateRoot = FileProcessor.FileProfileAggregate.FileProfileAggregate;
using FileProfileModel = global::FileProcessor.Models.FileProfile;

namespace FileProcessor.BusinessLogic.Managers;

public class FileProfileManager : IFileProfileManager
{
    private readonly IAggregateRepository<FileProfileAggregateRoot, DomainEvent> FileProfileAggregateRepository;

    public FileProfileManager(IAggregateRepository<FileProfileAggregateRoot, DomainEvent> fileProfileAggregateRepository)
    {
        this.FileProfileAggregateRepository = fileProfileAggregateRepository;
    }

    public async Task<Result<List<FileProfileModel>>> GetAllFileProfiles(CancellationToken cancellationToken)
    {
        Result<FileProfileAggregateRoot> aggregateResult = await this.LoadAggregate(cancellationToken);
        if (aggregateResult.IsSuccess() == false)
        {
            return ResultHelpers.CreateFailure(aggregateResult);
        }

        return Results.Ok(aggregateResult.GetValueOrThrow().GetAllProfiles());
    }

    public async Task<Result<FileProfileModel>> GetFileProfile(Guid fileProfileId, CancellationToken cancellationToken)
    {
        Result<FileProfileAggregateRoot> aggregateResult = await this.LoadAggregate(cancellationToken);
        if (aggregateResult.IsSuccess() == false)
        {
            return ResultHelpers.CreateFailure(aggregateResult);
        }

        FileProfileModel fileProfile = aggregateResult.GetValueOrThrow().GetProfile(fileProfileId);
        if (fileProfile == null)
        {
            return new Error(ErrorType.NotFound, $"No file profile found for File Profile Id {fileProfileId}");
        }

        return Results.Ok(fileProfile);
    }

    public async Task<Result<List<FileProfileModel>>> GetFileProfileVersions(Guid fileProfileId, CancellationToken cancellationToken)
    {
        Result<FileProfileAggregateRoot> aggregateResult = await this.LoadAggregate(cancellationToken);
        if (aggregateResult.IsSuccess() == false)
            return ResultHelpers.CreateFailure(aggregateResult);

        List<FileProfileModel> versions = aggregateResult.GetValueOrThrow().GetProfileVersions(fileProfileId);
        if (versions.Count == 0)
            return new Error(ErrorType.NotFound, $"No file profile found for File Profile Id {fileProfileId}");

        return Results.Ok(versions);
    }

    public async Task<Result<List<FileProfileModel>>> GetAllFileProfileVersions(CancellationToken cancellationToken)
    {
        Result<FileProfileAggregateRoot> aggregateResult = await this.LoadAggregate(cancellationToken);
        if (aggregateResult.IsSuccess() == false)
            return ResultHelpers.CreateFailure(aggregateResult);

        return Results.Ok(aggregateResult.GetValueOrThrow().GetAllProfileVersions());
    }

    public async Task<Result<FileProfileModel>> GetFileProfileVersion(Guid fileProfileId,
                                                                       Guid fileProfileVersionId,
                                                                       CancellationToken cancellationToken)
    {
        Result<FileProfileAggregateRoot> aggregateResult = await this.LoadAggregate(cancellationToken);
        if (aggregateResult.IsSuccess() == false)
            return ResultHelpers.CreateFailure(aggregateResult);

        FileProfileModel version = aggregateResult.GetValueOrThrow().GetProfileVersion(fileProfileId, fileProfileVersionId);
        if (version == null)
            return new Error(ErrorType.NotFound, $"No file profile version found for File Profile Id {fileProfileId} and Version Id {fileProfileVersionId}");

        return Results.Ok(version);
    }

    public async Task<Result<FileProfileModel>> CreateFileProfile(CreateFileProfileRequest request, CancellationToken cancellationToken)
    {
        Result<FileProfileAggregateRoot> aggregateResult = await this.LoadAggregate(cancellationToken);
        if (aggregateResult.IsSuccess() == false)
        {
            return ResultHelpers.CreateFailure(aggregateResult);
        }

        Result createResult = aggregateResult.GetValueOrThrow().CreateProfile(request);
        if (createResult.IsSuccess() == false)
        {
            return ResultHelpers.CreateFailure(createResult);
        }

        Result saveResult = await this.FileProfileAggregateRepository.SaveChanges(aggregateResult.GetValueOrThrow(), cancellationToken);
        if (saveResult.IsSuccess() == false)
        {
            return ResultHelpers.CreateFailure(saveResult);
        }

        Result<FileProfileModel> fileProfileResult = await this.GetFileProfile(request.FileProfileId, cancellationToken);
        return fileProfileResult;
    }

    public async Task<Result<FileProfileModel>> UpdateFileProfile(Guid fileProfileId, UpdateFileProfileRequest request, CancellationToken cancellationToken)
    {
        Result<FileProfileAggregateRoot> aggregateResult = await this.LoadAggregate(cancellationToken);
        if (aggregateResult.IsSuccess() == false)
        {
            return ResultHelpers.CreateFailure(aggregateResult);
        }

        Result updateResult = aggregateResult.GetValueOrThrow().UpdateProfile(fileProfileId, request);
        if (updateResult.IsSuccess() == false)
        {
            return ResultHelpers.CreateFailure(updateResult);
        }

        Result saveResult = await this.FileProfileAggregateRepository.SaveChanges(aggregateResult.GetValueOrThrow(), cancellationToken);
        if (saveResult.IsSuccess() == false)
        {
            return ResultHelpers.CreateFailure(saveResult);
        }

        Result<FileProfileModel> fileProfileResult = await this.GetFileProfile(fileProfileId, cancellationToken);
        return fileProfileResult;
    }

    public async Task<Result> ArchiveFileProfile(Guid fileProfileId, CancellationToken cancellationToken)
    {
        Result<FileProfileAggregateRoot> aggregateResult = await this.LoadAggregate(cancellationToken);
        if (aggregateResult.IsSuccess() == false)
        {
            return ResultHelpers.CreateFailure(aggregateResult);
        }

        Result archiveResult = aggregateResult.GetValueOrThrow().ArchiveProfile(fileProfileId);
        if (archiveResult.IsSuccess() == false)
        {
            return ResultHelpers.CreateFailure(archiveResult);
        }

        return await this.FileProfileAggregateRepository.SaveChanges(aggregateResult.GetValueOrThrow(), cancellationToken);
    }

    private async Task<Result<FileProfileAggregateRoot>> LoadAggregate(CancellationToken cancellationToken)
    {
        Result<FileProfileAggregateRoot> aggregateResult = await this.FileProfileAggregateRepository.GetLatestVersion(FileProfileAggregateRoot.FileProfileCollectionId, cancellationToken);
        if (aggregateResult.IsSuccess())
        {
            return aggregateResult;
        }

        if (aggregateResult.GetErrors()[0].ErrorType != ErrorType.NotFound)
        {
            return ResultHelpers.CreateFailure(aggregateResult);
        }

        return Results.Ok(FileProfileAggregateRoot.Create());
    }
}
