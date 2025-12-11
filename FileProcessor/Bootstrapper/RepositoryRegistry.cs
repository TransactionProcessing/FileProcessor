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

[ExcludeFromCodeCoverage]
public class RepositoryRegistry : ServiceRegistry
{
    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="RepositoryRegistry"/> class.
    /// </summary>
    public RepositoryRegistry()
    {
        this.AddSingleton(typeof(IDbContextResolver<>), typeof(DbContextResolver<>));
        if (Startup.WebHostEnvironment.IsEnvironment("IntegrationTest") || Startup.Configuration.GetValue<Boolean>("ServiceOptions:UseInMemoryDatabase") == true)
        {
            this.AddDbContext<EstateManagementContext>(builder => builder.UseInMemoryDatabase("TransactionProcessorReadModel"));
        }
        else
        {
            this.AddDbContext<EstateManagementContext>(options =>
                options.UseSqlServer(ConfigurationReader.GetConnectionString("TransactionProcessorReadModel")));
        }

        String connectionString = Startup.Configuration.GetValue<String>("EventStoreSettings:ConnectionString");

        this.AddKurrentDBProjectionManagementClient(connectionString);
        this.AddKurrentDBPersistentSubscriptionsClient(connectionString);

        this.AddKurrentDBClient(connectionString);

        this.AddSingleton<IEventStoreContext, EventStoreContext>();

        this.AddSingleton<IAggregateRepository<FileAggregate, DomainEvent>, AggregateRepository<FileAggregate, DomainEvent>>();
        this.AddSingleton<IAggregateRepository<FileImportLogAggregate, DomainEvent>,
            AggregateRepository<FileImportLogAggregate, DomainEvent>>();

        this.AddSingleton<IFileProcessorManager, FileProcessorManager>();

        this.AddSingleton<Func<String, Int32, ISubscriptionRepository>>(cont => SubscriptionRepository.Create);
    }

    #endregion
}