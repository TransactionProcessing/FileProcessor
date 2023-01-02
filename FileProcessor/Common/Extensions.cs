namespace FileProcessor.Common;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading;
using EventStore.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.EventStore.EventHandling;
using Shared.EventStore.SubscriptionWorker;
using Shared.General;
using Shared.Logger;

[ExcludeFromCodeCoverage]
public static class Extensions
{
    public static IServiceCollection AddInSecureEventStoreClient(
        this IServiceCollection services,
        Uri address,
        Func<HttpMessageHandler>? createHttpMessageHandler = null)
    {
        return services.AddEventStoreClient((Action<EventStoreClientSettings>)(options =>
                                                                               {
                                                                                   options.ConnectivitySettings.Address = address;
                                                                                   options.ConnectivitySettings.Insecure = true;
                                                                                   options.CreateHttpMessageHandler = createHttpMessageHandler;
                                                                               }));
    }

    static Action<TraceEventType, String, String> log = (tt, subType, message) => {
                                                            String logMessage = $"{subType} - {message}";
                                                            switch (tt)
                                                            {
                                                                case TraceEventType.Critical:
                                                                    Logger.LogCritical(new Exception(logMessage));
                                                                    break;
                                                                case TraceEventType.Error:
                                                                    Logger.LogError(new Exception(logMessage));
                                                                    break;
                                                                case TraceEventType.Warning:
                                                                    Logger.LogWarning(logMessage);
                                                                    break;
                                                                case TraceEventType.Information:
                                                                    Logger.LogInformation(logMessage);
                                                                    break;
                                                                case TraceEventType.Verbose:
                                                                    Logger.LogDebug(logMessage);
                                                                    break;
                                                            }
                                                        };

    static Action<TraceEventType, String> mainLog = (tt, message) => Extensions.log(tt, "MAIN", message);
    static Action<TraceEventType, String> orderedLog = (tt, message) => Extensions.log(tt, "ORDERED", message);

    public static void PreWarm(this IApplicationBuilder applicationBuilder)
    {
        Startup.LoadTypes();

        IConfigurationSection subscriptionConfigSection = Startup.Configuration.GetSection("AppSettings:SubscriptionConfiguration");
        SubscriptionWorkersRoot subscriptionWorkersRoot = new SubscriptionWorkersRoot();
        subscriptionConfigSection.Bind(subscriptionWorkersRoot);

        if (subscriptionWorkersRoot.InternalSubscriptionService)
        {
            String eventStoreConnectionString = ConfigurationReader.GetValue("EventStoreSettings", "ConnectionString");

            ISubscriptionRepository subscriptionRepository = SubscriptionRepository.Create(eventStoreConnectionString, subscriptionWorkersRoot.InternalSubscriptionServiceCacheDuration);
            ((SubscriptionRepository)subscriptionRepository).Trace += (sender,
                                                                       s) => Extensions.log(TraceEventType.Information, "REPOSITORY", s);

            // init our SubscriptionRepository
            subscriptionRepository.PreWarm(CancellationToken.None).Wait();

            List<SubscriptionWorker> workers = Extensions.ConfigureSubscriptions(subscriptionRepository, subscriptionWorkersRoot);
            foreach (SubscriptionWorker subscriptionWorker in workers)
            {
                subscriptionWorker.StartAsync(CancellationToken.None).Wait();
            }
        }
    }

    private static List<SubscriptionWorker> ConfigureSubscriptions(ISubscriptionRepository subscriptionRepository, SubscriptionWorkersRoot configuration)
    {
        List<SubscriptionWorker> workers = new List<SubscriptionWorker>();

        foreach (SubscriptionWorkerConfig configurationSubscriptionWorker in configuration.SubscriptionWorkers)
        {
            if (configurationSubscriptionWorker.Enabled == false)
                continue;

            if (configurationSubscriptionWorker.IsOrdered)
            {
                IDomainEventHandlerResolver eventHandlerResolver = Startup.Container.GetInstance<IDomainEventHandlerResolver>("Ordered");
                SubscriptionWorker worker = SubscriptionWorker.CreateOrderedSubscriptionWorker(Startup.EventStoreClientSettings,
                                                                                               eventHandlerResolver,
                                                                                               subscriptionRepository,
                                                                                               configuration.PersistentSubscriptionPollingInSeconds);
                worker.Trace += (_,
                                 args) => Extensions.orderedLog(TraceEventType.Information, args.Message);
                worker.Warning += (_,
                                   args) => Extensions.orderedLog(TraceEventType.Warning, args.Message);
                worker.Error += (_,
                                 args) => Extensions.orderedLog(TraceEventType.Error, args.Message);
                worker.SetIgnoreGroups(configurationSubscriptionWorker.IgnoreGroups);
                worker.SetIgnoreStreams(configurationSubscriptionWorker.IgnoreStreams);
                worker.SetIncludeGroups(configurationSubscriptionWorker.IncludeGroups);
                worker.SetIncludeStreams(configurationSubscriptionWorker.IncludeStreams);
                workers.Add(worker);
            }
            else
            {
                for (Int32 i = 0; i < configurationSubscriptionWorker.InstanceCount; i++)
                {
                    IDomainEventHandlerResolver eventHandlerResolver = Startup.Container.GetInstance<IDomainEventHandlerResolver>("Main");
                    SubscriptionWorker worker = SubscriptionWorker.CreateSubscriptionWorker(Startup.EventStoreClientSettings,
                                                                                            eventHandlerResolver,
                                                                                            subscriptionRepository,
                                                                                            configurationSubscriptionWorker.InflightMessages,
                                                                                            configuration.PersistentSubscriptionPollingInSeconds);

                    worker.Trace += (_,
                                     args) => Extensions.mainLog(TraceEventType.Information, args.Message);
                    worker.Warning += (_,
                                       args) => Extensions.mainLog(TraceEventType.Warning, args.Message);
                    worker.Error += (_,
                                     args) => Extensions.mainLog(TraceEventType.Error, args.Message);

                    worker.SetIgnoreGroups(configurationSubscriptionWorker.IgnoreGroups);
                    worker.SetIgnoreStreams(configurationSubscriptionWorker.IgnoreStreams);
                    worker.SetIncludeGroups(configurationSubscriptionWorker.IncludeGroups);
                    worker.SetIncludeStreams(configurationSubscriptionWorker.IncludeStreams);

                    workers.Add(worker);
                }
            }
        }

        return workers;
    }
}