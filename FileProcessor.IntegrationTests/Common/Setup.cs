namespace FileProcessor.IntegrationTests.Common
{
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.Net;
    using System.Threading;
    using Ductus.FluentDocker.Builders;
    using Ductus.FluentDocker.Services;
    using Ductus.FluentDocker.Services.Extensions;
    using Microsoft.Extensions.Logging;
    using NLog;
    using Shared.Logger;
    using Shouldly;
    using TechTalk.SpecFlow;
    using ILogger = Microsoft.Extensions.Logging.ILogger;

    [Binding]
    public class Setup
    {
        public static IContainerService DatabaseServerContainer;
        private static String DbConnectionStringWithNoDatabase;
        public static INetworkService DatabaseServerNetwork;

        public static String SqlServerContainerName = "shareddatabasesqlserver";

        public const String SqlUserName = "sa";

        public const String SqlPassword = "thisisalongpassword123!";

        //public static IContainerService StartSqlContainerWithOpenConnection(String containerName,
        //                                                                    ILogger logger,
        //                                                                    String imageName,
        //                                                                    INetworkService networkService,
        //                                                                    String hostFolder,
        //                                                                    (String URL, String UserName, String Password)? dockerCredentials,
        //                                                                    String sqlUserName = "sa",
        //                                                                    String sqlPassword = "thisisalongpassword123!")
        //{
        //    //logger.LogInformation("About to start SQL Server Container");
        //    IContainerService databaseServerContainer = new Builder().UseContainer().WithName(containerName).UseImage(imageName)
        //                                                             .WithEnvironment("ACCEPT_EULA=Y", $"SA_PASSWORD={sqlPassword}").ExposePort(1433)
        //                                                             .UseNetwork(networkService).KeepContainer().KeepRunning().ReuseIfExists().Build().Start()
        //                                                             .WaitForPort("1433/tcp", 30000);

        //    //logger.LogInformation("SQL Server Container Started");

        //    //logger.LogInformation("About to SQL Server Container is running");
        //    IPEndPoint sqlServerEndpoint = databaseServerContainer.ToHostExposedEndpoint("1433/tcp");

        //    // Try opening a connection
        //    Int32 maxRetries = 10;
        //    Int32 counter = 1;

        //    String server = "127.0.0.1";
        //    String database = "master";
        //    String user = sqlUserName;
        //    String password = sqlPassword;
        //    String port = sqlServerEndpoint.Port.ToString();

        //    String connectionString = $"server={server},{port};user id={user}; password={password}; database={database};";
        //    //logger.LogInformation($"Connection String {connectionString}");
        //    SqlConnection connection = new SqlConnection(connectionString);

        //    while (counter <= maxRetries)
        //    {
        //        try
        //        {
        //            //logger.LogInformation($"Database Connection Attempt {counter}");

        //            connection.Open();

        //            SqlCommand command = connection.CreateCommand();
        //            command.CommandText = "SELECT * FROM sys.databases";
        //            command.ExecuteNonQuery();

        //            //logger.LogInformation("Connection Opened");

        //            connection.Close();
        //            //logger.LogInformation("SQL Server Container Running");
        //            break;
        //        }
        //        catch (SqlException ex)
        //        {
        //            if (connection.State == ConnectionState.Open)
        //            {
        //                connection.Close();
        //            }

        //            //logger.LogError(ex);
        //            Thread.Sleep(20000);
        //        }
        //        finally
        //        {
        //            counter++;
        //        }
        //    }

        //    return databaseServerContainer;
        //}

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
            Setup.DatabaseServerContainer = Shared.IntegrationTesting.DockerHelper.StartSqlContainerWithOpenConnection(Setup.SqlServerContainerName,
                logger,
                "mcr.microsoft.com/mssql/server:2019-latest",
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