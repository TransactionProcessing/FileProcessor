using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileProcessor
{
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.IO.Abstractions;
    using System.Net.Http;
    using BusinessLogic.Managers;
    using EventStore.Client;
    using File.DomainEvents;
    using FileImportLog.DomainEvents;
    using Lamar.Microsoft.DependencyInjection;
    using MediatR;
    using Microsoft.Extensions.DependencyInjection;
    using Shared.EventStore.EventHandling;
    using Shared.EventStore.Subscriptions;
    using Shared.Logger;

    [ExcludeFromCodeCoverage]
    public class Program
    {
        public static void Main(string[] args)
        {
            Program.CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            Console.Title = "File Processor";

            //At this stage, we only need our hosting file for ip and ports
            IConfigurationRoot config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                                                                  .AddJsonFile("hosting.json", optional: true)
                                                                  .AddJsonFile("hosting.development.json", optional: true)
                                                                  .AddEnvironmentVariables().Build();

            IHostBuilder hostBuilder = Host.CreateDefaultBuilder(args);
            hostBuilder.UseLamar();
            hostBuilder.ConfigureLogging(logging =>
                                         {
                                             logging.AddConsole();

                                         });
            hostBuilder.ConfigureWebHostDefaults(webBuilder =>
                                                 {
                                                     webBuilder.UseStartup<Startup>();
                                                     webBuilder.UseConfiguration(config);
                                                     webBuilder.UseKestrel();
                                                 });
            hostBuilder.ConfigureServices(services =>
                                          {
                                              services.AddHostedService<FileProcessingWorker>(provider =>
                                                                                              {
                                                                                                  IFileProcessorManager fileProcessorManager =
                                                                                                      provider.GetRequiredService<IFileProcessorManager>();
                                                                                                  IMediator mediator = provider.GetRequiredService<IMediator>();
                                                                                                  IFileSystem fileSystem = provider.GetRequiredService<IFileSystem>();
                                                                                                  FileProcessingWorker worker =
                                                                                                      new FileProcessingWorker(fileProcessorManager,
                                                                                                          mediator,
                                                                                                          fileSystem);
                                                                                                  worker.TraceGenerated += Worker_TraceGenerated;
                                                                                                  return worker;
                                                                                              });
                                          });

            return hostBuilder;
        }

        private static void Worker_TraceGenerated(string trace, LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Trace:
                    Logger.LogTrace(trace);
                    break;
                case LogLevel.Debug:
                    Logger.LogDebug(trace);
                    break;
                case LogLevel.Information:
                    Logger.LogInformation(trace);
                    break;
                case LogLevel.Warning:
                    Logger.LogWarning(trace);
                    break;
                case LogLevel.Error:
                    Logger.LogError(new Exception(trace));
                    break;
                case LogLevel.Critical:
                    Logger.LogCritical(new Exception(trace));
                    break;
            }
        }
    }
}
