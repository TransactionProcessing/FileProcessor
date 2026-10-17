using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FileProcessor.BusinessLogic.Managers;
using FileProcessor.BusinessLogic.Services;
using FileProcessor.FileAggregate;
using FileProcessor.Models;
using Imposter.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EntityFramework;
using Shared.EventStore.Aggregate;
using Shouldly;
using SimpleResults;
using TransactionProcessor.Database.Contexts;
using Xunit;
using FileProfileModel = global::FileProcessor.Models.FileProfile;
using FileAggregateRoot = FileProcessor.FileAggregate.FileAggregate;

namespace FileProcessor.BusinessLogic.Tests;

public class FileProfileVersionMigrationServiceTests
{
    [Fact]
    public async Task RunAsync_AssignsInitialVersionToLegacyFile()
    {
        (EstateManagementContext context, IDbContextResolverImposter<EstateManagementContext> resolver) = this.CreateContext();
        TransactionProcessor.Database.Entities.File file = FileProcessor.Testing.TestData.Files1[0];
        context.Files.Add(file);
        await context.SaveChangesAsync();

        IFileProfileManagerImposter profileManager = new();
        profileManager.GetAllFileProfileVersions(Arg<CancellationToken>.Any())
                      .ReturnsAsync(Result.Success(new List<FileProfileModel>
                      {
                          new FileProfileModel(file.FileProfileId,
                                               "Profile",
                                               "C:/profile",
                                               "Request",
                                               "Operator",
                                               "\n",
                                               "Handler",
                                               FileProcessor.Testing.TestData.FileProfileVersionId,
                                               1)
                      }));

        FileAggregateRoot aggregate = FileAggregateRoot.Create(file.FileId);
        aggregate.CreateFile(new FileAggregateExtensions.CreateFileRequest(file.FileImportLogId,
                                                                            FileProcessor.Testing.TestData.EstateId,
                                                                            file.MerchantId,
                                                                            file.UserId,
                                                                            file.FileProfileId,
                                                                            file.FileLocation,
                                                                            FileProcessor.Testing.TestData.FileUploadedDateTime,
                                                                            FileProcessor.Testing.TestData.SafaricomOperatorId));
        IAggregateRepositoryImposter<FileAggregateRoot, DomainEvent> repository = new();
        repository.GetLatestVersion(file.FileId, Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(aggregate));
        repository.SaveChanges(Arg<FileAggregateRoot>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success());

        FileProfileVersionMigrationService service = new(profileManager.Instance(),
                                                          resolver.Instance(),
                                                          repository.Instance());

        Result<FileProfileVersionMigrationReport> result = await service.RunAsync(CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Data.FilesAssigned.ShouldBe(1);
        result.Data.Failures.ShouldBeEmpty();
        aggregate.GetFile().FileProfileVersionId.ShouldBe(FileProcessor.Testing.TestData.FileProfileVersionId);
    }

    [Fact]
    public async Task RunAsync_WhenProfileCannotBeResolved_ReportsFailureAndLeavesFileUnchanged()
    {
        (EstateManagementContext context, IDbContextResolverImposter<EstateManagementContext> resolver) = this.CreateContext();
        TransactionProcessor.Database.Entities.File file = FileProcessor.Testing.TestData.Files1[0];
        file.FileProfileId = Guid.NewGuid();
        context.Files.Add(file);
        await context.SaveChangesAsync();

        IFileProfileManagerImposter profileManager = new();
        profileManager.GetAllFileProfileVersions(Arg<CancellationToken>.Any())
                      .ReturnsAsync(Result.Success(new List<FileProfileModel>()));

        FileAggregateRoot aggregate = FileAggregateRoot.Create(file.FileId);
        aggregate.CreateFile(new FileAggregateExtensions.CreateFileRequest(file.FileImportLogId,
                                                                            FileProcessor.Testing.TestData.EstateId,
                                                                            file.MerchantId,
                                                                            file.UserId,
                                                                            file.FileProfileId,
                                                                            file.FileLocation,
                                                                            FileProcessor.Testing.TestData.FileUploadedDateTime,
                                                                            FileProcessor.Testing.TestData.SafaricomOperatorId));
        IAggregateRepositoryImposter<FileAggregateRoot, DomainEvent> repository = new();
        repository.GetLatestVersion(file.FileId, Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(aggregate));

        FileProfileVersionMigrationService service = new(profileManager.Instance(),
                                                          resolver.Instance(),
                                                          repository.Instance());

        Result<FileProfileVersionMigrationReport> result = await service.RunAsync(CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        result.Data.FilesAssigned.ShouldBe(0);
        result.Data.Failures.Count.ShouldBe(1);
        result.Data.Failures.Single().ShouldContain(file.FileId.ToString());
        aggregate.GetFile().FileProfileVersionId.ShouldBe(Guid.Empty);
    }

    [Fact]
    public async Task RunAsync_WhenRepeated_SkipsAlreadyAssignedFile()
    {
        (EstateManagementContext context, IDbContextResolverImposter<EstateManagementContext> resolver) = this.CreateContext();
        TransactionProcessor.Database.Entities.File file = FileProcessor.Testing.TestData.Files1[0];
        context.Files.Add(file);
        await context.SaveChangesAsync();

        IFileProfileManagerImposter profileManager = new();
        profileManager.GetAllFileProfileVersions(Arg<CancellationToken>.Any())
                      .ReturnsAsync(Result.Success(new List<FileProfileModel>
                      {
                          new FileProfileModel(file.FileProfileId,
                                               "Profile",
                                               "C:/profile",
                                               "Request",
                                               "Operator",
                                               "\n",
                                               "Handler",
                                               FileProcessor.Testing.TestData.FileProfileVersionId,
                                               1)
                      }));

        FileAggregateRoot aggregate = FileAggregateRoot.Create(file.FileId);
        aggregate.CreateFile(new FileAggregateExtensions.CreateFileRequest(file.FileImportLogId,
                                                                            FileProcessor.Testing.TestData.EstateId,
                                                                            file.MerchantId,
                                                                            file.UserId,
                                                                            file.FileProfileId,
                                                                            file.FileLocation,
                                                                            FileProcessor.Testing.TestData.FileUploadedDateTime,
                                                                            FileProcessor.Testing.TestData.SafaricomOperatorId));
        IAggregateRepositoryImposter<FileAggregateRoot, DomainEvent> repository = new();
        repository.GetLatestVersion(file.FileId, Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success(aggregate));
        repository.SaveChanges(Arg<FileAggregateRoot>.Any(), Arg<CancellationToken>.Any()).ReturnsAsync(Result.Success());

        FileProfileVersionMigrationService service = new(profileManager.Instance(),
                                                          resolver.Instance(),
                                                          repository.Instance());

        Result<FileProfileVersionMigrationReport> firstResult = await service.RunAsync(CancellationToken.None);
        Result<FileProfileVersionMigrationReport> secondResult = await service.RunAsync(CancellationToken.None);

        firstResult.Data.FilesAssigned.ShouldBe(1);
        secondResult.Data.FilesAssigned.ShouldBe(0);
        secondResult.Data.FilesSkipped.ShouldBe(1);
        secondResult.Data.Failures.ShouldBeEmpty();
    }

    private (EstateManagementContext Context, IDbContextResolverImposter<EstateManagementContext> Resolver) CreateContext()
    {
        DbContextOptions<EstateManagementContext> options = new DbContextOptionsBuilder<EstateManagementContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        EstateManagementContext context = new(options);
        ServiceCollection services = new();
        services.AddTransient(_ => context);
        ServiceProvider provider = services.BuildServiceProvider();
        IServiceScope scope = provider.CreateScope();
        IDbContextResolverImposter<EstateManagementContext> resolver = new();
        resolver.Resolve(Arg<string>.Any(), Arg<string>.Any()).Returns(new ResolvedDbContext<EstateManagementContext>(scope));
        return (context, resolver);
    }
}
