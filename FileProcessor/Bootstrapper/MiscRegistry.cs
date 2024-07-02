using Microsoft.Extensions.Logging;

namespace FileProcessor.Bootstrapper;

using BusinessLogic.Common;
using BusinessLogic.Services;
using Lamar;
using Microsoft.Extensions.DependencyInjection;
using Shared.General;
using Shared.Middleware;
using System.Collections.Generic;
using System;
using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class MiscRegistry : ServiceRegistry
{
    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="MiscRegistry"/> class.
    /// </summary>
    public MiscRegistry()
    {
        this.AddSingleton<IModelFactory, ModelFactory>();
        this.AddSingleton<Common.IModelFactory, Common.ModelFactory>();
        this.AddSingleton<IFileProcessorDomainService, FileProcessorDomainService>();

        bool logRequests = ConfigurationReaderExtensions.GetValueOrDefault<Boolean>("MiddlewareLogging", "LogRequests", true);
        bool logResponses = ConfigurationReaderExtensions.GetValueOrDefault<Boolean>("MiddlewareLogging", "LogResponses", true);
        LogLevel middlewareLogLevel = ConfigurationReaderExtensions.GetValueOrDefault<LogLevel>("MiddlewareLogging", "MiddlewareLogLevel", LogLevel.Warning);

        RequestResponseMiddlewareLoggingConfig config =
            new RequestResponseMiddlewareLoggingConfig(middlewareLogLevel, logRequests, logResponses);

        this.AddSingleton(config);
    }

    #endregion
}

public static class ConfigurationReaderExtensions
{
    public static T GetValueOrDefault<T>(String sectionName, String keyName, T defaultValue)
    {
        try
        {
            var value = ConfigurationReader.GetValue(sectionName, keyName);

            if (String.IsNullOrEmpty(value))
            {
                return defaultValue;
            }

            return (T)Convert.ChangeType(value, typeof(T));
        }
        catch (KeyNotFoundException kex)
        {
            return defaultValue;
        }
    }
}