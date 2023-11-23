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
using Shared.EventStore.Extensions;
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

    public static void PreWarm(this IApplicationBuilder applicationBuilder)
    {
        Startup.LoadTypes();

        IConfigurationSection subscriptionConfigSection = Startup.Configuration.GetSection("AppSettings:SubscriptionConfiguration");
        SubscriptionWorkersRoot subscriptionWorkersRoot = new SubscriptionWorkersRoot();
        subscriptionConfigSection.Bind(subscriptionWorkersRoot);

        String eventStoreConnectionString = ConfigurationReader.GetValue("EventStoreSettings", "ConnectionString");

        IDomainEventHandlerResolver mainEventHandlerResolver = Startup.Container.GetInstance<IDomainEventHandlerResolver>("Main");

        Dictionary<String, IDomainEventHandlerResolver> eventHandlerResolvers = new Dictionary<String, IDomainEventHandlerResolver> {
                                                                                        {"Main", mainEventHandlerResolver}
                                                                                    };

        Func<String, Int32, ISubscriptionRepository> subscriptionRepositoryResolver = Startup.Container.GetInstance<Func<String, Int32, ISubscriptionRepository>>();

        applicationBuilder.ConfigureSubscriptionService(subscriptionWorkersRoot,
                                                        eventStoreConnectionString,
                                                        Startup.EventStoreClientSettings,
                                                        eventHandlerResolvers,
                                                        Extensions.log,
                                                        subscriptionRepositoryResolver,
                                                        CancellationToken.None).Wait(CancellationToken.None);
    }
}