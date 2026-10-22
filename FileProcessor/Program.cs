using FileProcessor.Bootstrapper;
using FileProcessor.Common;
using FileProcessor.Endpoints;
using FileProcessor.Services;
using FileProcessor.BusinessLogic.Services;
using Lamar;
using Lamar.Microsoft.DependencyInjection;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using LogManager = NLog.LogManager;
using Sentry.Extensibility;
using Shared.EventStore.EventHandling;
using Shared.Extensions;
using Shared.General;
using Shared.Logger;
using Shared.Middleware;
using Shared.Serialisation;
using Shared.DomainDrivenDesign.EventSourcing;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using System;
using System.IO;
using System.Reflection;
using NLog;
using Logger = Shared.Logger.Logger;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using Shared.EventStore.Aggregate;
using Shared.EventStore.SubscriptionWorker;
using System.IO.Abstractions;
using TransactionProcessing.ApiDefaults;

namespace FileProcessor;

[ExcludeFromCodeCoverage]
public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        FileInfo assemblyFile = new FileInfo(Assembly.GetExecutingAssembly().Location);
        string assemblyDirectory = assemblyFile.DirectoryName!;
        string contentRoot = Directory.GetCurrentDirectory();
        string nlogConfigPath = Path.Combine(contentRoot, "nlog.config");

        LogManager.Setup(b =>
        {
            b.SetupLogFactory(setup =>
            {
                setup.AddCallSiteHiddenAssembly(typeof(NlogLogger).Assembly);
                setup.AddCallSiteHiddenAssembly(typeof(Shared.Logger.Logger).Assembly);
                setup.AddCallSiteHiddenAssembly(typeof(TenantMiddleware).Assembly);
            });
            b.LoadConfigurationFromFile(nlogConfigPath);
        });

        builder.Host.UseWindowsService();
        builder.Host.UseLamar();
        builder.Logging.AddConsole();
        builder.Logging.AddNLog();

        ConfigureApplicationConfiguration(builder, assemblyDirectory);
        ConfigurationReader.Initialise((IConfigurationRoot)builder.Configuration);
        builder.Services.AddHostedService<FileProfileDirectoryRefreshService>();

        builder.Host.ConfigureContainer<ServiceRegistry>((context, services) =>
        {
            TypeProvider.LoadDomainEventsTypeDynamically();
            services.IncludeRegistry<MediatorRegistry>();
            services.IncludeRegistry(new DomainEventHandlerRegistry(builder.Configuration));
            services.IncludeRegistry(new RepositoryRegistry(builder.Configuration, builder.Environment));
            services.IncludeRegistry(new MiddlewareRegistry(builder.Configuration));
            services.IncludeRegistry<FileRegistry>();
            services.IncludeRegistry<MiscRegistry>();
            services.IncludeRegistry<ClientRegistry>();
            services.IncludeRegistry<SerialiserRegistry>();
        });

        builder.WebHost.UseConfiguration(BuildHostingConfiguration(assemblyDirectory));
        builder.WebHost.UseKestrel();

        if (builder.Configuration.GetSection("SentryConfiguration").Exists() && builder.Environment.IsDevelopment() == false)
        {
            IConfiguration sentryConfiguration = builder.Configuration;
            builder.WebHost.UseSentry(options =>
            {
                options.Dsn = sentryConfiguration["SentryConfiguration:Dsn"];
                options.SendDefaultPii = true;
                options.MaxRequestBodySize = RequestSize.Always;
                options.CaptureBlockingCalls = ConfigurationReader.GetValueOrDefault("SentryConfiguration", "CaptureBlockingCalls", false);
                options.IncludeActivityData = ConfigurationReader.GetValueOrDefault("SentryConfiguration", "IncludeActivityData", false);
                options.Release = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";
            });
        }

        builder.WebHost.ConfigureKestrel(serverOptions =>
        {
            serverOptions.Limits.MaxRequestBodySize = UploadFileSettings.GetMaximumMultipartRequestSizeBytes();
        });

        WebApplication app = builder.Build();
        StringSerialiser.Initialise(app.Services.GetRequiredService<IStringSerialiser>());

        ConfigurePipeline(app);
        app.Run();
    }

    private static void ConfigureApplicationConfiguration(WebApplicationBuilder builder, string assemblyDirectory)
    {
        IWebHostEnvironment environment = builder.Environment;
        builder.Configuration.SetBasePath(assemblyDirectory)
            .AddJsonFile("hosting.json", optional: true)
            .AddJsonFile($"hosting.{environment.EnvironmentName}.json", optional: true)
            .AddJsonFile("/home/txnproc/config/appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"/home/txnproc/config/appsettings.{environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
            .AddJsonFile("/home/txnproc/config/appsettings.local.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["HealthMonitoring:Service:Version"] = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "0.0.0.0"
            });
    }

    private static IConfiguration BuildHostingConfiguration(string assemblyDirectory)
    {
        return new ConfigurationBuilder()
            .SetBasePath(assemblyDirectory)
            .AddJsonFile("hosting.json", optional: true)
            .AddJsonFile("hosting.development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
    }

    private static void ConfigurePipeline(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        ILogger logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("FileProcessor");
        Logger.Initialise(logger);
        IConfigurationRoot configuration = (IConfigurationRoot)app.Configuration;
        configuration.LogConfiguration(Logger.LogWarning);
        ConfigurationReader.Initialise(configuration);

        app.UseMiddleware<TenantMiddleware>();
        app.AddRequestResponseLogging();
        app.AddExceptionHandler();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapDomainEventEndpoint();
        app.MapFileEndpoints();
        app.MapFileImportLogEndpoints();
        app.MapFileProfileEndpoints();
        app.MapMigrationEndpoints();
        // Liveness details for every registered health check.
        app.MapHealthChecks("health", new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = Shared.HealthChecks.HealthCheckMiddleware.WriteResponse
        });
        // Health-check payload formatted for the Health Checks UI client.
        app.MapHealthChecks("healthui", new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = HealthChecks.UI.Client.UIResponseWriter.WriteHealthCheckUIResponse
        });

        IContainer container = (IContainer)app.Services;
        app.PreWarm(container.GetInstance<IFileSystem>(),
                    container.GetInstance<IFileProfileDirectorySynchronizer>(),
                    container.GetInstance<IFileProfileDirectoryRecoveryService>(),
                    app.Configuration,
                    container.GetInstance<IDomainEventHandlerResolver>("Main"),
                    container.GetInstance<Func<string, int, ISubscriptionRepository>>());
        app.UseApiDefaults();
    }
}
