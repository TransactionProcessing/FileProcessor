using FileProcessor.BusinessLogic.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Results;
using SimpleResults;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FileProcessor.Services;

public sealed class FileProfileDirectoryRefreshService : BackgroundService
{
    private const int DefaultPollingWindowInSeconds = 120;
    private const string PollingWindowKey = "AppSettings:FileProfilePollingWindowInSeconds";

    private readonly IFileProfileDirectorySynchronizer DirectorySynchronizer;
    private readonly IConfiguration Configuration;
    private readonly ILogger<FileProfileDirectoryRefreshService> Logger;

    public FileProfileDirectoryRefreshService(
        IFileProfileDirectorySynchronizer directorySynchronizer,
        IConfiguration configuration,
        ILogger<FileProfileDirectoryRefreshService> logger)
    {
        this.DirectorySynchronizer = directorySynchronizer;
        this.Configuration = configuration;
        this.Logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        int pollingWindowInSeconds = this.Configuration.GetValue<int?>(PollingWindowKey) ?? DefaultPollingWindowInSeconds;
        pollingWindowInSeconds = Math.Max(1, pollingWindowInSeconds);
        TimeSpan pollingWindow = TimeSpan.FromSeconds(pollingWindowInSeconds);

        this.Logger.LogInformation("File profile directory refresh started with a {PollingWindow} second interval", pollingWindowInSeconds);

        using PeriodicTimer timer = new PeriodicTimer(pollingWindow);

        while (stoppingToken.IsCancellationRequested == false)
        {
            try
            {
                if (await timer.WaitForNextTickAsync(stoppingToken) == false)
                {
                    break;
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            try
            {
                Result result = await this.DirectorySynchronizer.SyncAsync(stoppingToken);
                if (result.IsFailed)
                {
                    this.Logger.LogWarning("File profile directory refresh failed: {Message}", result.Message);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                this.Logger.LogError(exception, "Unexpected error while refreshing file profile directories");
            }
        }
    }
}
