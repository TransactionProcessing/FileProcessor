namespace FileProcessor.IntegrationTests.Common
{
    using Ductus.FluentDocker.Builders;
    using Ductus.FluentDocker.Services;
    using Ductus.FluentDocker.Services.Extensions;
    using Microsoft.Extensions.Logging;
    using NLog;
    using Reqnroll;
    using Shared.IntegrationTesting;
    using Shared.Logger;
    using Shouldly;
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.IO;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using ILogger = Microsoft.Extensions.Logging.ILogger;

    [Binding]
    public class Setup
    {
        public static (String usename, String password) SqlCredentials = ("sa", "thisisalongpassword123!");
        public static (String url, String username, String password) DockerCredentials = ("https://www.docker.com", "stuartferguson", "Sc0tland");

        public static async Task GlobalSetup(DockerHelper dockerHelper)
        {
            ShouldlyConfiguration.DefaultTaskTimeout = TimeSpan.FromMinutes(1);
        }
    }
}