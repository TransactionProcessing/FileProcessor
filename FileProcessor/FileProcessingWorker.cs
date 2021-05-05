namespace FileProcessor
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessLogic.Managers;
    using BusinessLogic.Requests;
    using FIleProcessor.Models;
    using MediatR;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Shared.EventStore.Aggregate;
    using Shared.EventStore.EventStore;
    using Shared.EventStore.Subscriptions;
    using Shared.General;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Microsoft.Extensions.Hosting.BackgroundService" />
    [ExcludeFromCodeCoverage]
    public class FileProcessingWorker : BackgroundService
    {
        #region Fields

        /// <summary>
        /// The file handler resolver
        /// </summary>
        private readonly Func<String, Object> FileHandlerResolver;

        /// <summary>
        /// The file processor manager
        /// </summary>
        private readonly IFileProcessorManager FileProcessorManager;

        /// <summary>
        /// The mediator
        /// </summary>
        private readonly IMediator Mediator;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionWorker" /> class.
        /// </summary>
        /// <param name="fileProcessorManager">The file processor manager.</param>
        /// <param name="mediator">The mediator.</param>
        public FileProcessingWorker(IFileProcessorManager fileProcessorManager,
                                    IMediator mediator)
        {
            this.FileProcessorManager = fileProcessorManager;
            this.Mediator = mediator;
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when trace is generated.
        /// </summary>
        public event TraceHandler TraceGenerated;

        #endregion

        #region Methods

        /// <summary>
        /// Triggered when the application host is ready to start the service.
        /// </summary>
        /// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            // TODO: Do we poll here for files incase they have been left from a previous run
            await base.StartAsync(cancellationToken);
        }

        /// <summary>
        /// Triggered when the application host is performing a graceful shutdown.
        /// </summary>
        /// <param name="cancellationToken">Indicates that the shutdown process should no longer be graceful.</param>
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
        }

        /// <summary>
        /// This method is called when the <see cref="T:Microsoft.Extensions.Hosting.IHostedService" /> starts. The implementation should return a task that represents
        /// the lifetime of the long running operation(s) being performed.
        /// </summary>
        /// <param name="stoppingToken">Triggered when <see cref="M:Microsoft.Extensions.Hosting.IHostedService.StopAsync(System.Threading.CancellationToken)" /> is called.</param>
        /// <returns>
        /// A <see cref="T:System.Threading.Tasks.Task" /> that represents the long running operations.
        /// </returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            TypeProvider.LoadDomainEventsTypeDynamically();

            foreach (KeyValuePair<Type, String> type in TypeMap.Map)
            {
                this.LogInformation($"Type name {type.Value} mapped to {type.Key.Name}");
            }

            while (stoppingToken.IsCancellationRequested == false)
            {
                try
                {
                    List<Task> fileProcessingTasks = new List<Task>();
                    var fileProfiles = await this.FileProcessorManager.GetAllFileProfiles(stoppingToken);
                    foreach (FileProfile fileProfile in fileProfiles)
                    {
                        var files = Directory.GetFiles(fileProfile.ListeningDirectory).Take(1).ToList(); // Only process 1 file per file profile concurrently

                        foreach (String file in files)
                        {
                            this.LogDebug($"File {file} detected");
                            var request = this.CreateProcessFileRequest(fileProfile, file);
                            fileProcessingTasks.Add(this.Mediator.Send(request));
                        }
                    }

                    await Task.WhenAll(fileProcessingTasks.ToArray());
                }
                catch(Exception e)
                {
                    this.LogCritical(e);
                }

                String fileProfilePollingWindowInSeconds = ""; //ConfigurationReader.GetValue("AppSetting", "FileProfilePollingWindowInSeconds");
                if (string.IsNullOrEmpty(fileProfilePollingWindowInSeconds))
                {
                    fileProfilePollingWindowInSeconds = "60";
                }

                // Delay for configured seconds before polling for files again
                await Task.Delay(TimeSpan.FromSeconds(int.Parse(fileProfilePollingWindowInSeconds)), stoppingToken);
            }
        }

        /// <summary>
        /// Creates the process file request.
        /// </summary>
        /// <param name="fileProfile">The file profile.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        private IRequest CreateProcessFileRequest(FileProfile fileProfile,
                                                  String fileName)
        {
            IRequest request = null;

            // Extract the file Id
            var fields = fileName.Split('-');

            switch(fileProfile.RequestType)
            {
                case "SafaricomTopupRequest":
                    request = new SafaricomTopupRequest(Guid.Parse(fields[1]), fileName, fileProfile.FileProfileId);
                    break;
            }

            return request;
        }

        /// <summary>
        /// Logs the critical.
        /// </summary>
        /// <param name="exception">The exception.</param>
        private void LogCritical(Exception exception)
        {
            if (this.TraceGenerated != null)
            {
                this.TraceGenerated(exception.Message, LogLevel.Critical);
                if (exception.InnerException != null)
                {
                    this.LogCritical(exception.InnerException);
                }
            }
        }

        /// <summary>
        /// Logs the debug.
        /// </summary>
        /// <param name="trace">The trace.</param>
        private void LogDebug(String trace)
        {
            if (this.TraceGenerated != null)
            {
                this.TraceGenerated(trace, LogLevel.Debug);
            }
        }

        /// <summary>
        /// Traces the specified exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        private void LogError(Exception exception)
        {
            if (this.TraceGenerated != null)
            {
                this.TraceGenerated(exception.Message, LogLevel.Error);
                if (exception.InnerException != null)
                {
                    this.LogError(exception.InnerException);
                }
            }
        }

        /// <summary>
        /// Traces the specified trace.
        /// </summary>
        /// <param name="trace">The trace.</param>
        private void LogInformation(String trace)
        {
            if (this.TraceGenerated != null)
            {
                this.TraceGenerated(trace, LogLevel.Information);
            }
        }

        /// <summary>
        /// Logs the warning.
        /// </summary>
        /// <param name="trace">The trace.</param>
        private void LogWarning(String trace)
        {
            if (this.TraceGenerated != null)
            {
                this.TraceGenerated(trace, LogLevel.Warning);
            }
        }

        #endregion
    }
}