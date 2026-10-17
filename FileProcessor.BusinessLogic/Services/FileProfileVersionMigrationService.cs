using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FileProcessor.BusinessLogic.Managers;
using FileProcessor.FileAggregate;
using FileProcessor.Models;
using Microsoft.EntityFrameworkCore;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EntityFramework;
using Shared.EventStore.Aggregate;
using Shared.Results;
using SimpleResults;
using TransactionProcessor.Database.Contexts;
using FileAggregateRoot = FileProcessor.FileAggregate.FileAggregate;
using FileProfileModel = global::FileProcessor.Models.FileProfile;

namespace FileProcessor.BusinessLogic.Services;

public interface IFileProfileVersionMigrationService
{
    Task<Result<FileProfileVersionMigrationReport>> RunAsync(CancellationToken cancellationToken);
}

public sealed class FileProfileVersionMigrationService : IFileProfileVersionMigrationService
{
    private const String EstateManagementDatabaseName = "TransactionProcessorReadModel";

    private readonly IFileProfileManager FileProfileManager;
    private readonly IDbContextResolver<EstateManagementContext> Resolver;
    private readonly IAggregateRepository<FileAggregateRoot, DomainEvent> FileAggregateRepository;

    public FileProfileVersionMigrationService(IFileProfileManager fileProfileManager,
                                               IDbContextResolver<EstateManagementContext> resolver,
                                               IAggregateRepository<FileAggregateRoot, DomainEvent> fileAggregateRepository)
    {
        this.FileProfileManager = fileProfileManager;
        this.Resolver = resolver;
        this.FileAggregateRepository = fileAggregateRepository;
    }

    public async Task<Result<FileProfileVersionMigrationReport>> RunAsync(CancellationToken cancellationToken)
    {
        Result<List<FileProfileModel>> profileVersionsResult = await this.FileProfileManager.GetAllFileProfileVersions(cancellationToken);
        if (profileVersionsResult.IsFailed)
            return ResultHelpers.CreateFailure(profileVersionsResult);

        FileProfileVersionMigrationReport report = new();
        Dictionary<Guid, Guid> initialVersions = this.GetInitialVersions(profileVersionsResult.Data, report);

        ResolvedDbContext<EstateManagementContext> resolvedContext = this.Resolver.Resolve(EstateManagementDatabaseName, null);
        List<ExistingFile> files = await resolvedContext.Context.Files
                                                        .AsNoTracking()
                                                        .Select(file => new ExistingFile(file.FileId, file.FileProfileId))
                                                        .ToListAsync(cancellationToken);

        foreach (ExistingFile file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();
            report.FilesChecked++;

            Result<FileAggregateRoot> aggregateResult = await this.FileAggregateRepository.GetLatestVersion(file.FileId, cancellationToken);
            if (aggregateResult.IsFailed || aggregateResult.Data == null)
            {
                report.Failures.Add($"File [{file.FileId}] could not be loaded: {aggregateResult.Message}");
                continue;
            }

            FileDetails fileDetails = aggregateResult.Data.GetFile();
            if (fileDetails.FileProfileVersionId != Guid.Empty)
            {
                report.FilesSkipped++;
                continue;
            }

            if (initialVersions.TryGetValue(file.FileProfileId, out Guid initialVersionId) == false)
            {
                report.Failures.Add($"File [{file.FileId}] references profile [{file.FileProfileId}], which has no usable initial version");
                continue;
            }

            Result assignResult = aggregateResult.Data.AssignFileProfileVersion(initialVersionId);
            if (assignResult.IsFailed)
            {
                report.Failures.Add($"File [{file.FileId}] could not be assigned profile version [{initialVersionId}]: {assignResult.Message}");
                continue;
            }

            Result saveResult = await this.FileAggregateRepository.SaveChanges(aggregateResult.Data, cancellationToken);
            if (saveResult.IsFailed)
            {
                report.Failures.Add($"File [{file.FileId}] could not be saved: {saveResult.Message}");
                continue;
            }

            report.FilesAssigned++;
        }

        return Result.Success(report);
    }

    private Dictionary<Guid, Guid> GetInitialVersions(List<FileProfileModel> profileVersions,
                                                       FileProfileVersionMigrationReport report)
    {
        Dictionary<Guid, Guid> initialVersions = new();

        foreach (IGrouping<Guid, FileProfileModel> profileGroup in profileVersions.GroupBy(profile => profile.FileProfileId))
        {
            report.ProfilesChecked++;
            List<FileProfileModel> versions = profileGroup.Where(profile => profile.FileProfileVersion == 1 && profile.FileProfileVersionId != Guid.Empty).ToList();
            if (versions.Count != 1)
            {
                report.Failures.Add($"Profile [{profileGroup.Key}] has {versions.Count} usable initial versions");
                continue;
            }

            initialVersions[profileGroup.Key] = versions[0].FileProfileVersionId;
            report.ProfilesWithInitialVersion++;
        }

        return initialVersions;
    }

    private sealed record ExistingFile(Guid FileId, Guid FileProfileId);
}

public sealed class FileProfileVersionMigrationReport
{
    public Int32 ProfilesChecked { get; set; }

    public Int32 ProfilesWithInitialVersion { get; set; }

    public Int32 FilesChecked { get; set; }

    public Int32 FilesAssigned { get; set; }

    public Int32 FilesSkipped { get; set; }

    public List<String> Failures { get; } = new();
}
