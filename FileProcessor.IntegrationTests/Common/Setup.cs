namespace FileProcessor.IntegrationTests.Common
{
    using System;
    using Ductus.FluentDocker.Services;
    using Ductus.FluentDocker.Services.Extensions;
    using NLog;
    using Shared.Logger;
    using Shouldly;
    using TechTalk.SpecFlow;

    [Binding]
    public class Setup
    {
        public static IContainerService DatabaseServerContainer;
        private static String DbConnectionStringWithNoDatabase;
        public static INetworkService DatabaseServerNetwork;

        public static String SqlServerContainerName = "shareddatabasesqlserver";

        public const String SqlUserName = "sa";

        public const String SqlPassword = "thisisalongpassword123!";
        [BeforeTestRun]
        protected static void GlobalSetup()
        {
            ShouldlyConfiguration.DefaultTaskTimeout = TimeSpan.FromMinutes(1);

            (String, String, String) dockerCredentials = ("https://www.docker.com", "stuartferguson", "Sc0tland");

            // Setup a network for the DB Server
            Setup.DatabaseServerNetwork = global::Shared.IntegrationTesting.DockerHelper.SetupTestNetwork("sharednetwork", true);

            NlogLogger logger = new NlogLogger();
            logger.Initialise(LogManager.GetLogger("Specflow"), "Specflow");
            LogManager.AddHiddenAssembly(typeof(NlogLogger).Assembly);

            // Start the Database Server here
            Setup.DatabaseServerContainer = global::Shared.IntegrationTesting.DockerHelper.StartSqlContainerWithOpenConnection(Setup.SqlServerContainerName,
                logger,
                "stuartferguson/subscriptionservicedatabasesqlserver",
                Setup.DatabaseServerNetwork,
                "",
                dockerCredentials,
                Setup.SqlUserName,
                Setup.SqlPassword);
        }

        public static String GetConnectionString(String databaseName)
        {
            return $"server={Setup.DatabaseServerContainer.Name};database={databaseName};user id={Setup.SqlUserName};password={Setup.SqlPassword}";
        }

        public static String GetLocalConnectionString(String databaseName)
        {
            Int32 databaseHostPort = Setup.DatabaseServerContainer.ToHostExposedEndpoint("1433/tcp").Port;

            return $"server=localhost,{databaseHostPort};database={databaseName};user id={Setup.SqlUserName};password={Setup.SqlPassword}";
        }

    }
}