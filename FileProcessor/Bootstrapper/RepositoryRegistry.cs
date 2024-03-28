﻿namespace FileProcessor.Bootstrapper;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Net.Security;
using BusinessLogic.Common;
using BusinessLogic.Managers;
using Common;
using EstateManagement.Database.Contexts;
using FileAggregate;
using FileImportLogAggregate;
using Lamar;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.DomainDrivenDesign.EventSourcing;
using Shared.EntityFramework;
using Shared.EntityFramework.ConnectionStringConfiguration;
using Shared.EventStore.Aggregate;
using Shared.EventStore.EventStore;
using Shared.EventStore.Extensions;
using Shared.EventStore.SubscriptionWorker;
using Shared.General;
using Shared.Repositories;

[ExcludeFromCodeCoverage]
public class RepositoryRegistry : ServiceRegistry
{
    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="RepositoryRegistry"/> class.
    /// </summary>
    public RepositoryRegistry()
    {
        Boolean useConnectionStringConfig = bool.Parse(ConfigurationReader.GetValue("AppSettings", "UseConnectionStringConfig"));

        if (useConnectionStringConfig)
        {
            String connectionStringConfigurationConnString = ConfigurationReader.GetConnectionString("ConnectionStringConfiguration");
            this.AddSingleton<IConnectionStringConfigurationRepository, ConnectionStringConfigurationRepository>();
            this.AddTransient(c => { return new ConnectionStringConfigurationContext(connectionStringConfigurationConnString); });

            // TODO: Read this from a the database and set
        }
        else
        {
            String connectionString = Startup.Configuration.GetValue<String>("EventStoreSettings:ConnectionString");

            this.AddEventStoreProjectionManagementClient(connectionString);
            this.AddEventStorePersistentSubscriptionsClient(connectionString);

            this.AddEventStoreClient(connectionString);
            this.AddSingleton<IConnectionStringConfigurationRepository, ConfigurationReaderConnectionStringRepository>();
        }

        this.AddSingleton<IEventStoreContext, EventStoreContext>();

        this.AddSingleton<IAggregateRepository<FileAggregate, DomainEvent>, AggregateRepository<FileAggregate, DomainEvent>>();
        this.AddSingleton<IAggregateRepository<FileImportLogAggregate, DomainEvent>,
            AggregateRepository<FileImportLogAggregate, DomainEvent>>();

        this.AddSingleton<IDbContextFactory<EstateManagementGenericContext>, DbContextFactory<EstateManagementGenericContext>>();
        this.AddSingleton<Func<String, EstateManagementGenericContext>>(cont => connectionString =>
                                                                                {
                                                                                    String databaseEngine =
                                                                                        ConfigurationReader.GetValue("AppSettings", "DatabaseEngine");

                                                                                    return databaseEngine switch
                                                                                    {
                                                                                        "MySql" => new EstateManagementMySqlContext(connectionString),
                                                                                        "SqlServer" => new EstateManagementSqlServerContext(connectionString),
                                                                                        _ => throw new
                                                                                            NotSupportedException($"Unsupported Database Engine {databaseEngine}")
                                                                                    };
                                                                                });
        this.AddSingleton<IFileProcessorManager, FileProcessorManager>();

        this.AddSingleton<Func<String, Int32, ISubscriptionRepository>>(cont => (esConnString, cacheDuration) => {
                                                                                    return SubscriptionRepository.Create(esConnString, cacheDuration);
                                                                                });
    }

    #endregion
}