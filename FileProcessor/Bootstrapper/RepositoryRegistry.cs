using TransactionProcessor.Database.Contexts;

namespace FileProcessor.Bootstrapper;

using BusinessLogic.Common;
using BusinessLogic.Managers;
using FileAggregate;
using FileImportLogAggregate;
using Lamar;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EntityFramework;
using Shared.EventStore.Aggregate;
using Shared.EventStore.EventStore;
using Shared.EventStore.SubscriptionWorker;
using Shared.General;
using System;
using System.Diagnostics.CodeAnalysis;
using FileProfileAggregateRoot = FileProcessor.FileProfileAggregate.FileProfileAggregate;
using Microsoft.AspNetCore.Hosting;

[ExcludeFromCodeCoverage]
public class RepositoryRegistry : ServiceRegistry
{
    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="RepositoryRegistry"/> class.
    /// </summary>
    public RepositoryRegistry(IConfiguration configuration, IWebHostEnvironment environment)
    {
        this.AddSingleton(typeof(IDbContextResolver<>), typeof(DbContextResolver<>));
        if (environment.IsEnvironment("IntegrationTest") || configuration.GetValue<Boolean>("ServiceOptions:UseInMemoryDatabase") == true)
        {
            this.AddDbContext<EstateManagementContext>(builder => builder.UseInMemoryDatabase("TransactionProcessorReadModel"));
        }
        else
        {
            this.AddDbContext<EstateManagementContext>(options =>
                options.UseSqlServer(ConfigurationReader.GetConnectionString("TransactionProcessorReadModel")));
        }

        String connectionString = configuration.GetValue<String>("EventStoreSettings:ConnectionString");

        this.AddKurrentDBProjectionManagementClient(connectionString);
        this.AddKurrentDBPersistentSubscriptionsClient(connectionString);

        this.AddKurrentDBClient(connectionString);

        this.AddSingleton<IEventStoreContext, EventStoreContext>();

        this.AddSingleton<IAggregateRepository<FileAggregate, DomainEvent>, AggregateRepository<FileAggregate, DomainEvent>>();
        this.AddSingleton<IAggregateRepository<FileImportLogAggregate, DomainEvent>,
            AggregateRepository<FileImportLogAggregate, DomainEvent>>();
        this.AddSingleton<IAggregateRepository<FileProfileAggregateRoot, DomainEvent>,
            AggregateRepository<FileProfileAggregateRoot, DomainEvent>>();

        this.AddSingleton<IFileProfileManager, FileProfileManager>();
        this.AddSingleton<IFileProcessorManager, FileProcessorManager>();

        this.AddSingleton<Func<String, Int32, ISubscriptionRepository>>(cont => SubscriptionRepository.Create);
    }

    #endregion
}
