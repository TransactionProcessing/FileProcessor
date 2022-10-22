﻿using System;
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
    using TransactionProcessor.Client;
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

        public ITransactionProcessorClient TransactionProcessorClient;

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
        
        /// <summary>
        /// The transaction processor port
        /// </summary>
        protected Int32 TransactionProcessorPort;
        
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
        
        #region Methods

        public String FileProcessorContainerName;
        /// <summary>
        /// Starts the containers for scenario run.
        /// </summary>
        /// <param name="scenarioName">Name of the scenario.</param>
        public override async Task StartContainersForScenarioRun(String scenarioName)
        {
            String IsSecureEventStoreEnvVar = Environment.GetEnvironmentVariable("IsSecureEventStore");

            if (IsSecureEventStoreEnvVar == null)
            {
                // No env var set so default to insecure
                this.IsSecureEventStore = false;
            }
            else
            {
                // We have the env var so we set the secure flag based on the value in the env var
                Boolean.TryParse(IsSecureEventStoreEnvVar, out Boolean isSecure);
                this.IsSecureEventStore = isSecure;
            }

            this.HostTraceFolder = FdOs.IsWindows() ? $"C:\\home\\txnproc\\trace\\{scenarioName}" : $"//home//txnproc//trace//{scenarioName}";
            
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
            this.ClientDetails = ("serviceClient", "Secret1");
            this.SqlServerDetails = (Setup.SqlServerContainerName, Setup.SqlUserName, Setup.SqlPassword);
            this.DockerCredentials = ("https://www.docker.com", "stuartferguson", "Sc0tland");

            INetworkService testNetwork = DockerHelper.SetupTestNetwork();
            this.TestNetworks.Add(testNetwork);

            IContainerService eventStoreContainer =
                this.SetupEventStoreContainer("eventstore/eventstore:21.10.0-buster-slim", testNetwork, isSecure: this.IsSecureEventStore);
            this.EventStoreHttpPort = eventStoreContainer.ToHostExposedEndpoint($"{DockerHelper.EventStoreHttpDockerPort}/tcp").Port;

            String insecureEventStoreEnvironmentVariable = "EventStoreSettings:Insecure=True";
            if (this.IsSecureEventStore)
            {
                insecureEventStoreEnvironmentVariable = "EventStoreSettings:Insecure=False";
            }

            String persistentSubscriptionPollingInSeconds = "AppSettings:PersistentSubscriptionPollingInSeconds=10";
            String internalSubscriptionServiceCacheDuration = "AppSettings:InternalSubscriptionServiceCacheDuration=0";
            String operationTimeoutEnvironmentVariable = "EventStoreSettings:OperationTimeoutInSeconds=60";

            IContainerService estateManagementContainer = this.SetupEstateManagementContainer("stuartferguson/estatemanagement",
                                                                                              new List<INetworkService>
                                                                                              {
                                                                                                  testNetwork,
                                                                                                  Setup.DatabaseServerNetwork
                                                                                              },
                                                                                              true,
                                                                                              additionalEnvironmentVariables: new List<String>
                                                                                                  {
                                                                                                      insecureEventStoreEnvironmentVariable,
                                                                                                      persistentSubscriptionPollingInSeconds,
                                                                                                      internalSubscriptionServiceCacheDuration
                                                                                                  });

            IContainerService securityServiceContainer = this.SetupSecurityServiceContainer("stuartferguson/securityservice",
                                                                                                    testNetwork,
                                                                                                    true);

            IContainerService voucherManagementContainer = this.SetupVoucherManagementContainer("stuartferguson/vouchermanagement",
                                                                                                new List<INetworkService>
                                                                                                {
                                                                                                    testNetwork
                                                                                                },
                                                                                                true,
                                                                                                additionalEnvironmentVariables: new List<String>
                                                                                                    {
                                                                                                        insecureEventStoreEnvironmentVariable,
                                                                                                        persistentSubscriptionPollingInSeconds,
                                                                                                        internalSubscriptionServiceCacheDuration
                                                                                                    });

            String pataPawaUrlEnvironmentVariable = "OperatorConfiguration:PataPawaPostPay:Url=http://" + this.TestHostContainerName + ":9000/PataPawaPostPayService/basichttp";
            String pataPawaApiLogonRequiredEnvironmentVariable = "OperatorConfiguration:PataPawaPostPay:ApiLogonRequired=false";
            String transactionProcessorReadModelConnectionString = $"ConnectionStrings:TransactionProcessorReadModel=\"server={this.SqlServerDetails.sqlServerContainerName};user id=sa;password={this.SqlServerDetails.sqlServerPassword};database=TransactionProcessorReadModel\"";

            IContainerService transactionProcessorContainer = this.SetupTransactionProcessorContainer("stuartferguson/transactionprocessor",
                                                                                                      new List<INetworkService> {
                                                                                                          testNetwork,
                                                                                                          Setup.DatabaseServerNetwork
                                                                                                      },
                                                                                                      true,
                                                                                                      additionalEnvironmentVariables:new List<String> {
                                                                                                          transactionProcessorReadModelConnectionString,
                                                                                                          pataPawaUrlEnvironmentVariable,
                                                                                                          pataPawaApiLogonRequiredEnvironmentVariable,
                                                                                                          insecureEventStoreEnvironmentVariable,
                                                                                                          persistentSubscriptionPollingInSeconds,
                                                                                                          internalSubscriptionServiceCacheDuration,
                                                                                                          $"AppSettings:VoucherManagementApi=http://{this.VoucherManagementContainerName}:{DockerHelper.VoucherManagementDockerPort}"
                                                                                                      });

            IContainerService estateReportingContainer = this.SetupEstateReportingContainer(        "stuartferguson/estatereporting",
                                                                                                    new List<INetworkService>
                                                                                                    {
                                                                                                        testNetwork,
                                                                                                        Setup.DatabaseServerNetwork
                                                                                                    },
                                                                                                    true,
                                                                                                    additionalEnvironmentVariables: new List<String>
                                                                                                        {
                                                                                                            insecureEventStoreEnvironmentVariable,
                                                                                                            persistentSubscriptionPollingInSeconds,
                                                                                                            internalSubscriptionServiceCacheDuration,
                                                                                                        });

            IContainerService testhostContainer = this.SetupTestHostContainer("stuartferguson/testhosts",
                                                                         new List<INetworkService>
                                                                         {
                                                                             testNetwork,
                                                                             Setup.DatabaseServerNetwork
                                                                         },
                                                                         true);

            IContainerService fileProcessorContainer = this.SetupFileProcessorContainer("fileprocessor",
                                                                                   new List<INetworkService>
                                                                                   {
                                                                                       testNetwork,
                                                                                       Setup.DatabaseServerNetwork
                                                                                   },
                                                                                   additionalEnvironmentVariables: new List<String>
                                                                                       {
                                                                                           insecureEventStoreEnvironmentVariable,
                                                                                           persistentSubscriptionPollingInSeconds,
                                                                                           internalSubscriptionServiceCacheDuration,
                                                                                           operationTimeoutEnvironmentVariable
                                                                                       });

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
            this.TransactionProcessorPort= transactionProcessorContainer.ToHostExposedEndpoint("5002/tcp").Port;

            // Setup the base address resolvers
            String EstateManagementBaseAddressResolver(String api) => $"http://127.0.0.1:{this.EstateManagementApiPort}";
            String SecurityServiceBaseAddressResolver(String api) => $"https://127.0.0.1:{this.SecurityServicePort}";
            String FileProcessorBaseAddressResolver(String api) => $"http://127.0.0.1:{this.FileProcessorPort}";
            String EstateReportingBaseAddressResolver(String api) => $"http://127.0.0.1:{this.EstateReportingApiPort}";
            String TransactionProcessorBaseAddressResolver(String api) => $"http://127.0.0.1:{this.TransactionProcessorPort}";

            var httpMessageHandler = new SocketsHttpHandler
                                     {
                                         SslOptions =
                                         {
                                             RemoteCertificateValidationCallback = (sender,
                                                                                    certificate,
                                                                                    chain,
                                                                                    errors) => true,
                                         }
                                     };
            HttpClient httpClient = new HttpClient(httpMessageHandler);
            this.EstateClient = new EstateClient(EstateManagementBaseAddressResolver, httpClient);
            this.SecurityServiceClient = new SecurityServiceClient(SecurityServiceBaseAddressResolver, httpClient);
            this.EstateReportingClient = new EstateReportingClient(EstateReportingBaseAddressResolver, httpClient);
            this.FileProcessorClient = new FileProcessorClient(FileProcessorBaseAddressResolver, httpClient);
            this.TransactionProcessorClient = new TransactionProcessorClient(TransactionProcessorBaseAddressResolver, httpClient);

            await this.LoadEventStoreProjections(this.EventStoreHttpPort, this.IsSecureEventStore).ConfigureAwait(false);
            await this.PopulateSubscriptionServiceConfigurationGeneric(this.IsSecureEventStore).ConfigureAwait(false);
        }

        public const Int32 FileProcessorDockerPort = 5009;

        public Boolean IsSecureEventStore { get; private set; }

        protected override String GenerateEventStoreConnectionString()
        {
            // TODO: this could move to shared
            String eventStoreAddress = $"esdb://admin:changeit@{this.EventStoreContainerName}:{DockerHelper.EventStoreHttpDockerPort}";
            if (this.IsSecureEventStore)
            {
                eventStoreAddress = $"{eventStoreAddress}?tls=true&tlsVerifyCert=false";
            }
            else
            {
                eventStoreAddress = $"{eventStoreAddress}?tls=false&tlsVerifyCert=false";
            }

            return eventStoreAddress;
        }

        private IContainerService SetupFileProcessorContainer(String imageName,
                                                              List<INetworkService> networkService,
                                                              Boolean forceLatestImage = false,
                                                              Int32 securityServicePort = DockerHelper.SecurityServiceDockerPort,
                                                              List<String> additionalEnvironmentVariables = null)
        {
            this.Logger.LogInformation("About to Start File Processor Container");

            List<String> environmentVariables = new List<String>();
            environmentVariables.Add($"EventStoreSettings:ConnectionString={this.GenerateEventStoreConnectionString()}");
            environmentVariables.Add($"AppSettings:SecurityService=https://{this.SecurityServiceContainerName}:{securityServicePort}");
            environmentVariables.Add($"SecurityConfiguration:Authority=https://{this.SecurityServiceContainerName}:{securityServicePort}");
            environmentVariables.Add($"urls=http://*:{DockerHelper.FileProcessorDockerPort}");
            environmentVariables.Add($"AppSettings:TransactionProcessorApi=http://{this.TransactionProcessorContainerName}:{DockerHelper.TransactionProcessorDockerPort}");
            environmentVariables.Add($"AppSettings:EstateManagementApi=http://{this.EstateManagementContainerName}:{DockerHelper.EstateManagementDockerPort}");
            environmentVariables.Add($"AppSettings:ClientId={this.ClientDetails.clientId}");
            environmentVariables.Add($"AppSettings:ClientSecret={this.ClientDetails.clientSecret}");
            environmentVariables
                .Add($"ConnectionStrings:EstateReportingReadModel=\"server={this.SqlServerDetails.sqlServerContainerName};user id={this.SqlServerDetails.sqlServerUserName};password={this.SqlServerDetails.sqlServerPassword};database=EstateReportingReadModel\"");
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

            ContainerBuilder fileProcessorContainer = new Builder().UseContainer().WithName(this.FileProcessorContainerName).WithEnvironment(environmentVariables.ToArray())
                                                                             .UseImage(imageName, forceLatestImage)
                                                                             .ExposePort(DockerHelper.FileProcessorDockerPort)
                                                                             .UseNetwork(networkService.ToArray());

            fileProcessorContainer = MountHostFolder(fileProcessorContainer);
            fileProcessorContainer = SetDockerCredentials(fileProcessorContainer);

            // Mount the folder to upload files
            String uploadFolder = FdOs.IsWindows() ? $"D:\\home\\txnproc\\specflow" : $"//home//txnproc//specflow";
            fileProcessorContainer.Mount(uploadFolder, "/home/txnproc/bulkfiles", MountType.ReadWrite);

            // Now build and return the container                
            IContainerService builtContainer =
                fileProcessorContainer.Build().Start().WaitForPort($"{DockerHelper.FileProcessorDockerPort}/tcp", 30000);

            this.Logger.LogInformation("File Processor Container Started");

            return builtContainer;
        }

        public async Task PopulateSubscriptionServiceConfigurationForEstate(String estateName, Boolean isSecureEventStore)
        {
            List<(String streamName, String groupName, Int32 maxRetries)> subscriptions = new List<(String streamName, String groupName, Int32 maxRetries)>();
            subscriptions.Add((estateName.Replace(" ", ""), "Reporting", 2));
            subscriptions.Add(($"EstateManagementSubscriptionStream_{estateName.Replace(" ", "")}", "Estate Management", 0));
            subscriptions.Add(($"TransactionProcessorSubscriptionStream_{estateName.Replace(" ", "")}", "Transaction Processor", 0));
            await this.PopulateSubscriptionServiceConfiguration(this.EventStoreHttpPort, subscriptions, isSecureEventStore);
        }
        public async Task PopulateSubscriptionServiceConfigurationGeneric(Boolean isSecureEventStore)
        {
            List<(String streamName, String groupName, Int32 maxRetries)> subscriptions = new List<(String streamName, String groupName, Int32 maxRetries)>();
            subscriptions.Add(($"$ce-MerchantBalanceArchive", "Transaction Processor - Ordered", 0));
            subscriptions.Add(($"$et-EstateCreatedEvent", "Transaction Processor - Ordered", 2));
            await this.PopulateSubscriptionServiceConfiguration(this.EventStoreHttpPort, subscriptions, isSecureEventStore);
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
                    EstateReportingSqlServerContext context = new EstateReportingSqlServerContext(connectionString);
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
