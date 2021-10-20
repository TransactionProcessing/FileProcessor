using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileProcessor.IntegrationTests.Common
{
    using System.Data;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using Client;
    using Ductus.FluentDocker.Builders;
    using Ductus.FluentDocker.Common;
    using Ductus.FluentDocker.Model.Builders;
    using Ductus.FluentDocker.Services;
    using Ductus.FluentDocker.Services.Extensions;
    using EstateManagement.Client;
    using EstateReporting.Client;
    using EstateReporting.Database;
    using EventStore.Client;
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
    using SecurityService.Client;
    using Shared.IntegrationTesting;
    using Shared.Logger;
    using ILogger = Shared.Logger.ILogger;

    public class DockerHelper : global::Shared.IntegrationTesting.DockerHelper
    {
        #region Fields

        /// <summary>
        /// The estate client
        /// </summary>
        public IEstateClient EstateClient;

        public IEstateReportingClient EstateReportingClient;

        /// <summary>
        /// The security service client
        /// </summary>
        public ISecurityServiceClient SecurityServiceClient;

        /// <summary>
        /// The file processor client
        /// </summary>
        public IFileProcessorClient FileProcessorClient;

        /// <summary>
        /// The test identifier
        /// </summary>
        public Guid TestId;

        /// <summary>
        /// The transaction processor client
        /// </summary>
        //public ITransactionProcessorClient TransactionProcessorClient;

        /// <summary>
        /// The containers
        /// </summary>
        protected List<IContainerService> Containers;

        /// <summary>
        /// The estate management API port
        /// </summary>
        protected Int32 EstateManagementApiPort;

        /// <summary>
        /// The estate reporting API port
        /// </summary>
        protected Int32 EstateReportingApiPort;

        /// <summary>
        /// The event store HTTP port
        /// </summary>
        protected Int32 EventStoreHttpPort;

        /// <summary>
        /// The security service port
        /// </summary>
        protected Int32 SecurityServicePort;

        public Int32 FileProcessorPort;

        /// <summary>
        /// The test networks
        /// </summary>
        protected List<INetworkService> TestNetworks;

        protected String FileProcessorContainerName;

        protected String SecurityServiceContainerName;

        protected String EstateManagementContainerName;

        protected String EventStoreContainerName;

        protected String EstateReportingContainerName;

        protected String TransactionProcessorContainerName;

        protected String TestHostContainerName;

        /// <summary>
        /// The transaction processor port
        /// </summary>
        protected Int32 TransactionProcessorPort;

        /// <summary>
        /// The logger
        /// </summary>
        private readonly NlogLogger Logger;

        private readonly TestingContext TestingContext;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DockerHelper" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="testingContext">The testing context.</param>
        public DockerHelper(NlogLogger logger, TestingContext testingContext)
        {
            this.Logger = logger;
            this.TestingContext = testingContext;
            this.Containers = new List<IContainerService>();
            this.TestNetworks = new List<INetworkService>();
        }

        #endregion

        protected String VoucherManagementContainerName;

        private async Task LoadEventStoreProjections()
        {
            //Start our Continous Projections - we might decide to do this at a different stage, but now lets try here
            String projectionsFolder = "../../../projections/continuous";
            IPAddress[] ipAddresses = Dns.GetHostAddresses("127.0.0.1");

            if (!String.IsNullOrWhiteSpace(projectionsFolder))
            {
                DirectoryInfo di = new DirectoryInfo(projectionsFolder);

                if (di.Exists)
                {
                    FileInfo[] files = di.GetFiles();

                    EventStoreProjectionManagementClient projectionClient = new EventStoreProjectionManagementClient(ConfigureEventStoreSettings(this.EventStoreHttpPort));

                    foreach (FileInfo file in files)
                    {
                        String projection = File.ReadAllText(file.FullName);
                        String projectionName = file.Name.Replace(".js", String.Empty);

                        try
                        {
                            Logger.LogInformation($"Creating projection [{projectionName}]");
                            await projectionClient.CreateContinuousAsync(projectionName, projection, trackEmittedStreams: true).ConfigureAwait(false);
                        }
                        catch (Exception e)
                        {
                            Logger.LogError(new Exception($"Projection [{projectionName}] error", e));
                        }
                    }
                }
            }

            Logger.LogInformation("Loaded projections");
        }

        #region Methods

        /// <summary>
        /// Starts the containers for scenario run.
        /// </summary>
        /// <param name="scenarioName">Name of the scenario.</param>
        public override async Task StartContainersForScenarioRun(String scenarioName)
        {
            String traceFolder = FdOs.IsWindows() ? $"D:\\home\\txnproc\\trace\\{scenarioName}" : $"//home//txnproc//trace//{scenarioName}";
            
            Logging.Enabled();

            Guid testGuid = Guid.NewGuid();
            this.TestId = testGuid;

            this.Logger.LogInformation($"Test Id is {testGuid}");

            // Setup the container names
            this.SecurityServiceContainerName = $"securityservice{testGuid:N}";
            this.EstateManagementContainerName = $"estate{testGuid:N}";
            this.EventStoreContainerName = $"eventstore{testGuid:N}";
            this.EstateReportingContainerName = $"estatereporting{testGuid:N}";
            this.TransactionProcessorContainerName = $"txnprocessor{testGuid:N}";
            this.TestHostContainerName = $"testhosts{testGuid:N}";
            this.VoucherManagementContainerName = $"vouchermanagement{testGuid:N}";
            this.FileProcessorContainerName = $"fileprocessor{testGuid:N}";

            String eventStoreAddress = $"http://{this.EventStoreContainerName}";

            (String, String, String) dockerCredentials = ("https://www.docker.com", "stuartferguson", "Sc0tland");

            INetworkService testNetwork = DockerHelper.SetupTestNetwork();
            this.TestNetworks.Add(testNetwork);
            IContainerService eventStoreContainer = DockerHelper.SetupEventStoreContainer(this.EventStoreContainerName, this.Logger, "eventstore/eventstore:20.10.0-buster-slim", testNetwork, traceFolder);
            this.EventStoreHttpPort = eventStoreContainer.ToHostExposedEndpoint($"{DockerHelper.EventStoreHttpDockerPort}/tcp").Port;

            await Retry.For(async () =>
            {
                await this.PopulateSubscriptionServiceConfiguration().ConfigureAwait(false);
            }, retryFor: TimeSpan.FromMinutes(2), retryInterval: TimeSpan.FromSeconds(30));

            IContainerService estateManagementContainer = DockerHelper.SetupEstateManagementContainer(this.EstateManagementContainerName, this.Logger,
                                                                                                      "stuartferguson/estatemanagement", new List<INetworkService>
                                                                                                                          {
                                                                                                                              testNetwork,
                                                                                                                              Setup.DatabaseServerNetwork
                                                                                                                          }, traceFolder, dockerCredentials,
                                                                                                      this.SecurityServiceContainerName,
                                                                                                      eventStoreAddress,
                                                                                                      (Setup.SqlServerContainerName,
                                                                                                      "sa",
                                                                                                      "thisisalongpassword123!"),
                                                                                                      ("serviceClient", "Secret1"),
                                                                                                      true);

            IContainerService securityServiceContainer = DockerHelper.SetupSecurityServiceContainer(this.SecurityServiceContainerName,
                                                                                                    this.Logger,
                                                                                                    "stuartferguson/securityservice",
                                                                                                    testNetwork,
                                                                                                    traceFolder,
                                                                                                    dockerCredentials,
                                                                                                    true);

            IContainerService voucherManagementContainer = SetupVoucherManagementContainer(this.VoucherManagementContainerName,
                                                                                           this.Logger,
                                                                                           "stuartferguson/vouchermanagement",
                                                                                           new List<INetworkService>
                                                                                           {
                                                                                               testNetwork
                                                                                           },
                                                                                           traceFolder,
                                                                                           dockerCredentials,
                                                                                           this.SecurityServiceContainerName,
                                                                                           this.EstateManagementContainerName,
                                                                                           eventStoreAddress,
                                                                                           (Setup.SqlServerContainerName,
                                                                                               "sa",
                                                                                               "thisisalongpassword123!"),
                                                                                           ("serviceClient", "Secret1"),
                                                                                           true);

            List<String> additionalVariables = new List<String>()
                                               {
                                                   $"AppSettings:VoucherManagementApi=http://{this.VoucherManagementContainerName}:{DockerHelper.VoucherManagementDockerPort}"
                                               };
            IContainerService transactionProcessorContainer = DockerHelper.SetupTransactionProcessorContainer(this.TransactionProcessorContainerName,
                                                                                                              this.Logger,
                                                                                                              "stuartferguson/transactionprocessor",
                                                                                                              new List<INetworkService>
                                                                                                              {
                                                                                                                  testNetwork
                                                                                                              },
                                                                                                              traceFolder,
                                                                                                              dockerCredentials,
                                                                                                              this.SecurityServiceContainerName,
                                                                                                              this.EstateManagementContainerName,
                                                                                                              eventStoreAddress,
                                                                                                              ("serviceClient", "Secret1"),
                                                                                                              this.TestHostContainerName,
                                                                                                              this.VoucherManagementContainerName,
                                                                                                              forceLatestImage:true);

            IContainerService estateReportingContainer = DockerHelper.SetupEstateReportingContainer(this.EstateReportingContainerName,
                                                                                                    this.Logger,
                                                                                                    "stuartferguson/estatereporting",
                                                                                                    new List<INetworkService>
                                                                                                    {
                                                                                                        testNetwork,
                                                                                                        Setup.DatabaseServerNetwork
                                                                                                    },
                                                                                                    traceFolder,
                                                                                                    dockerCredentials,
                                                                                                    this.SecurityServiceContainerName,
                                                                                                    eventStoreAddress,
                                                                                                    (Setup.SqlServerContainerName,
                                                                                                    "sa",
                                                                                                    "thisisalongpassword123!"),
                                                                                                    ("serviceClient", "Secret1"),
                                                                                                    true);

            IContainerService testhostContainer = SetupTestHostContainer(this.TestHostContainerName,
                                                                         this.Logger,
                                                                         "stuartferguson/testhosts",
                                                                         new List<INetworkService>
                                                                         {
                                                                             testNetwork,
                                                                             Setup.DatabaseServerNetwork
                                                                         },
                                                                         traceFolder,
                                                                         dockerCredentials,
                                                                         (Setup.SqlServerContainerName,
                                                                             "sa",
                                                                             "thisisalongpassword123!"),
                                                                         true);

            IContainerService fileProcessorContainer = SetupFileProcessorContainer(this.FileProcessorContainerName,
                                                                                   this.Logger,
                                                                                   "fileprocessor",
                                                                                   new List<INetworkService>
                                                                                   {
                                                                                       testNetwork,
                                                                                       Setup.DatabaseServerNetwork
                                                                                   },
                                                                                   traceFolder,
                                                                                   dockerCredentials,
                                                                                   this.SecurityServiceContainerName,
                                                                                   this.EstateManagementContainerName,
                                                                                   this.TransactionProcessorContainerName,
                                                                                   eventStoreAddress,
                                                                                   (Setup.SqlServerContainerName,
                                                                                       "sa",
                                                                                       "thisisalongpassword123!"),
                                                                                   ("serviceClient", "Secret1"));

            this.Containers.AddRange(new List<IContainerService>
                                     {
                                         eventStoreContainer,
                                         estateManagementContainer,
                                         securityServiceContainer,
                                         transactionProcessorContainer,
                                         estateReportingContainer,
                                         testhostContainer,
                                         voucherManagementContainer,
                                         fileProcessorContainer
                                     });

            // Cache the ports
            this.EstateManagementApiPort = estateManagementContainer.ToHostExposedEndpoint("5000/tcp").Port;
            this.EstateReportingApiPort = estateReportingContainer.ToHostExposedEndpoint("5005/tcp").Port;
            this.SecurityServicePort = securityServiceContainer.ToHostExposedEndpoint("5001/tcp").Port;
            this.FileProcessorPort = fileProcessorContainer.ToHostExposedEndpoint("5009/tcp").Port;

            // Setup the base address resolvers
            String EstateManagementBaseAddressResolver(String api) => $"http://127.0.0.1:{this.EstateManagementApiPort}";
            String SecurityServiceBaseAddressResolver(String api) => $"https://127.0.0.1:{this.SecurityServicePort}";
            String FileProcessorBaseAddressResolver(String api) => $"http://127.0.0.1:{this.FileProcessorPort}";
            String EstateReportingBaseAddressResolver(String api) => $"http://127.0.0.1:{this.EstateReportingApiPort}";

            HttpClientHandler clientHandler = new HttpClientHandler
                                              {
                                                  ServerCertificateCustomValidationCallback = (message,
                                                                                               certificate2,
                                                                                               arg3,
                                                                                               arg4) =>
                                                                                              {
                                                                                                  return true;
                                                                                              }

                                              };
            HttpClient httpClient = new HttpClient(clientHandler);
            this.EstateClient = new EstateClient(EstateManagementBaseAddressResolver, httpClient);
            this.SecurityServiceClient = new SecurityServiceClient(SecurityServiceBaseAddressResolver, httpClient);
            this.EstateReportingClient = new EstateReportingClient(EstateReportingBaseAddressResolver, httpClient);
            this.FileProcessorClient = new FileProcessorClient(FileProcessorBaseAddressResolver, httpClient);

            await this.LoadEventStoreProjections().ConfigureAwait(false);
        }

        public const Int32 FileProcessorDockerPort = 5009;

        private IContainerService SetupFileProcessorContainer(String containerName, ILogger logger, String imageName,
                                                              List<INetworkService> networkService,
                                                              String hostFolder,
                                                              (String URL, String UserName, String Password)? dockerCredentials,
                                                              String securityServiceContainerName,
                                                              String estateManamgementContainerName,
                                                              String transactionProcessorContainerName,
                                                              String eventStoreAddress,
                                                              (String sqlServerContainerName, String sqlServerUserName, String sqlServerPassword)
                                                                  sqlServerDetails,
                                                              (String clientId, String clientSecret) clientDetails,
                                                              Boolean forceLatestImage = false,
                                                              Int32 securityServicePort = DockerHelper.SecurityServiceDockerPort,
                                                              List<String> additionalEnvironmentVariables = null)
        {
            logger.LogInformation("About to Start File Processor Container");
            List<String> environmentVariables = new List<String>();
            environmentVariables.Add($"EventStoreSettings:ConnectionString={eventStoreAddress}:{DockerHelper.EventStoreHttpDockerPort}");
            environmentVariables.Add($"AppSettings:SecurityService=https://{securityServiceContainerName}:{securityServicePort}");
            environmentVariables.Add($"SecurityConfiguration:Authority=https://{securityServiceContainerName}:{securityServicePort}");
            environmentVariables.Add($"urls=http://*:{DockerHelper.FileProcessorDockerPort}");
            environmentVariables.Add($"AppSettings:TransactionProcessorApi=http://{transactionProcessorContainerName}:{DockerHelper.TransactionProcessorDockerPort}");
            environmentVariables.Add($"AppSettings:EstateManagementApi=http://{estateManamgementContainerName}:{DockerHelper.EstateManagementDockerPort}");
            environmentVariables.Add($"AppSettings:ClientId={clientDetails.clientId}");
            environmentVariables.Add($"AppSettings:ClientSecret={clientDetails.clientSecret}");
            environmentVariables
                .Add($"ConnectionStrings:EstateReportingReadModel=\"server={sqlServerDetails.sqlServerContainerName};user id={sqlServerDetails.sqlServerUserName};password={sqlServerDetails.sqlServerPassword};database=EstateReportingReadModel\"");
            var ciEnvVar = Environment.GetEnvironmentVariable("CI");
            if ((String.IsNullOrEmpty(ciEnvVar) == false) && String.Compare(ciEnvVar, Boolean.TrueString, StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                // we are running in CI 
                environmentVariables.Add($"AppSettings:TemporaryFileLocation={"/home/runner/bulkfiles/temporary"}");
                
                environmentVariables.Add($"AppSettings:FileProfiles:0:ListeningDirectory={"/home/runner/bulkfiles/safaricom"}");
                environmentVariables.Add($"AppSettings:FileProfiles:1:ListeningDirectory={"/home/runner/bulkfiles/voucher"}");
            }

            if (additionalEnvironmentVariables != null)
            {
                environmentVariables.AddRange(additionalEnvironmentVariables);
            }

            ContainerBuilder fileProcessorContainer = new Builder().UseContainer().WithName(containerName).WithEnvironment(environmentVariables.ToArray())
                                                                             .UseImage(imageName, forceLatestImage)
                                                                             .ExposePort(DockerHelper.FileProcessorDockerPort)
                                                                             .UseNetwork(networkService.ToArray());

            if (String.IsNullOrEmpty(hostFolder) == false)
            {
                fileProcessorContainer = fileProcessorContainer.Mount(hostFolder, "/home/txnproc/trace", MountType.ReadWrite);
            }

            if (dockerCredentials.HasValue)
            {
                fileProcessorContainer.WithCredential(dockerCredentials.Value.URL, dockerCredentials.Value.UserName, dockerCredentials.Value.Password);
            }

            // Mount the folder to upload files
            String uploadFolder = FdOs.IsWindows() ? $"D:\\home\\txnproc\\specflow" : $"//home//txnproc//specflow";
            fileProcessorContainer.Mount(uploadFolder, "/home/txnproc/bulkfiles", MountType.ReadWrite);

            // Now build and return the container                
            IContainerService builtContainer =
                fileProcessorContainer.Build().Start().WaitForPort($"{DockerHelper.FileProcessorDockerPort}/tcp", 30000);

            logger.LogInformation("File Processor Container Started");

            return builtContainer;
        }

        protected async Task PopulateSubscriptionServiceConfiguration()
        {
            EventStorePersistentSubscriptionsClient client = new EventStorePersistentSubscriptionsClient(ConfigureEventStoreSettings(this.EventStoreHttpPort));

            PersistentSubscriptionSettings settings = new PersistentSubscriptionSettings(resolveLinkTos: true);
            await client.CreateAsync("$ce-EstateAggregate", "Reporting", settings);
            await client.CreateAsync("$ce-MerchantAggregate", "Reporting", settings);
            await client.CreateAsync("$ce-ContractAggregate", "Reporting", settings);
            await client.CreateAsync("$ce-TransactionAggregate", "Reporting", settings);
            await client.CreateAsync("$ce-FileImportLogAggregate", "Reporting", settings);
            await client.CreateAsync("$ce-FileAggregate", "Reporting", settings);

            await client.CreateAsync("$et-TransactionHasBeenCompletedEvent", "TransactionProcessor", settings);
            await client.CreateAsync("$ce-FileImportLogAggregate", "FileProcessor", settings);
            await client.CreateAsync("$ce-FileAggregate", "FileProcessor", settings);
        }

        private static EventStoreClientSettings ConfigureEventStoreSettings(Int32 eventStoreHttpPort)
        {
            String connectionString = $"http://127.0.0.1:{eventStoreHttpPort}";

            EventStoreClientSettings settings = new EventStoreClientSettings();
            settings.CreateHttpMessageHandler = () => new SocketsHttpHandler
            {
                SslOptions =
                                                          {
                                                              RemoteCertificateValidationCallback = (sender,
                                                                                                     certificate,
                                                                                                     chain,
                                                                                                     errors) => true,
                                                          }
            };
            settings.ConnectionName = "Specflow";
            settings.ConnectivitySettings = new EventStoreClientConnectivitySettings
            {
                Address = new Uri(connectionString),
            };

            settings.DefaultCredentials = new UserCredentials("admin", "changeit");
            return settings;
        }
        
        private async Task RemoveEstateReadModel()
        {
            List<Guid> estateIdList = this.TestingContext.GetAllEstateIds();

            foreach (Guid estateId in estateIdList)
            {
                String databaseName = $"EstateReportingReadModel{estateId}";

                await Retry.For(async () =>
                {
                    // Build the connection string (to master)
                    String connectionString = Setup.GetLocalConnectionString(databaseName);
                    EstateReportingContext context = new EstateReportingContext(connectionString);
                    await context.Database.EnsureDeletedAsync(CancellationToken.None);
                });
            }
        }

        /// <summary>
        /// Stops the containers for scenario run.
        /// </summary>
        public override async Task StopContainersForScenarioRun()
        {
            await RemoveEstateReadModel().ConfigureAwait(false);

            if (this.Containers.Any())
            {
                foreach (IContainerService containerService in this.Containers)
                {
                    containerService.StopOnDispose = true;
                    containerService.RemoveOnDispose = true;
                    containerService.Dispose();
                }
            }

            if (this.TestNetworks.Any())
            {
                foreach (INetworkService networkService in this.TestNetworks)
                {
                    networkService.Stop();
                    networkService.Remove(true);
                }
            }
        }

        #endregion
    }
}
