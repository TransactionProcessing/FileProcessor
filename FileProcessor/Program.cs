using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Middleware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileProcessor
{
    using BusinessLogic.Managers;
    using EventStore.Client;
    using File.DomainEvents;
    using FileImportLog.DomainEvents;
    using Lamar.Microsoft.DependencyInjection;
    using MediatR;
    using Microsoft.Extensions.DependencyInjection;
    using NLog;
    using NLog.Extensions.Logging;
    using Sentry.Extensibility;
    using Shared.EventStore.EventHandling;
    using Shared.General;
    using Shared.Logger;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.IO.Abstractions;
    using System.Net.Http;
    using System.Reflection;

    [ExcludeFromCodeCoverage]
    public class Program
    {
        public static void Main(string[] args)
        {
            Program.CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            //At this stage, we only need our hosting file for ip and ports
            FileInfo fi = new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location);

            IConfigurationRoot config = new ConfigurationBuilder().SetBasePath(fi.Directory.FullName)
                                                                  .AddJsonFile("hosting.json", optional: true)
                                                                  .AddJsonFile("hosting.development.json", optional: true)
                                                                  .AddEnvironmentVariables().Build();

            String contentRoot = Directory.GetCurrentDirectory();
            String nlogConfigPath = Path.Combine(contentRoot, "nlog.config");

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

            IHostBuilder hostBuilder = Host.CreateDefaultBuilder(args);
            hostBuilder.UseWindowsService();
            hostBuilder.UseLamar();
            hostBuilder.ConfigureLogging(logging =>
                                         {
                                             logging.AddConsole();
                                             logging.AddNLog();
                                         });
            hostBuilder.ConfigureWebHostDefaults(webBuilder =>
                                                 {
                                                     webBuilder.ConfigureAppConfiguration((context, configBuilder) =>
                                                     {
                                                         var env = context.HostingEnvironment;

                                                         configBuilder.SetBasePath(fi.Directory.FullName)
                                                             .AddJsonFile("hosting.json", optional: true)
                                                             .AddJsonFile($"hosting.{env.EnvironmentName}.json", optional: true)
                                                             .AddJsonFile("/home/txnproc/config/appsettings.json", optional: true, reloadOnChange: true)
                                                             .AddJsonFile($"/home/txnproc/config/appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                                                             .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                                                             .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                                                             .AddEnvironmentVariables();

                                                         // Build a snapshot of configuration so we can use it immediately (e.g. for Sentry)
                                                         var builtConfig = configBuilder.Build();

                                                         // Keep existing static usage (if you must), and initialise the ConfigurationReader now.
                                                         Startup.Configuration = builtConfig;
                                                         ConfigurationReader.Initialise(Startup.Configuration);

                                                         // Configure Sentry on the webBuilder using the config snapshot.
                                                         var sentrySection = builtConfig.GetSection("SentryConfiguration");
                                                         if (sentrySection.Exists())
                                                         {
                                                             // Replace the condition below if you intended to only enable Sentry in certain environments.
                                                             if (env.IsDevelopment() == false)
                                                             {
                                                                 webBuilder.UseSentry(o =>
                                                                 {
                                                                     o.Dsn = builtConfig["SentryConfiguration:Dsn"];
                                                                     o.SendDefaultPii = true;
                                                                     o.MaxRequestBodySize = RequestSize.Always;
                                                                     o.CaptureBlockingCalls = ConfigurationReader.GetValueOrDefault("SentryConfiguration", "CaptureBlockingCalls", false);
                                                                     o.IncludeActivityData = ConfigurationReader.GetValueOrDefault("SentryConfiguration", "IncludeActivityData", false);
                                                                     o.Release = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";
                                                                 });
                                                             }
                                                         }
                                                     });

                                                     webBuilder.UseStartup<Startup>();
                                                     webBuilder.UseConfiguration(config);
                                                     webBuilder.UseKestrel();
                                                     webBuilder.ConfigureKestrel(serverOptions =>
                                                     {
                                                         // Remove Kestrel max request body size limit (set to null -> no limit)
                                                         serverOptions.Limits.MaxRequestBodySize = null;
                                                     });
                                                 });
            

            return hostBuilder;
        }
    }
}
