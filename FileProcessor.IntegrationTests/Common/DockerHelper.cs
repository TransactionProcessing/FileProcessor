using DotNet.Testcontainers.Builders;
using KurrentDB.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FileProcessor.IntegrationTests.Common
{
    using Client;
    using EventStore.Client;
    using SecurityService.Client;
    using TransactionProcessing.UnionResults;
    using TransactionProcessing.Shared.IntegrationTesting;
    using TransactionProcessing.Shared.IntegrationTesting.TestContainers;
    using TransactionProcessing.Shared.Serialisation;
    using System.Net.Http;
    using System.Threading;
    using TestHosts.Clients;
    using TransactionProcessor.Client;

    public class DockerHelper : global::TransactionProcessing.Shared.IntegrationTesting.TestContainers.DockerHelper{
        #region Fields
        
        public HttpClient TestHostHttpClient;

        /// <summary>
        /// The security service client
        /// </summary>
        public ISecurityServiceClient SecurityServiceClient;

        /// <summary>
        /// The file processor client
        /// </summary>
        public IFileProcessorClient FileProcessorClient;

        public ITransactionProcessorClient TransactionProcessorClient;

        private readonly TestingContext TestingContext;

        public KurrentDBProjectionManagementClient ProjectionManagementClient;

        public IAgencyBankingClient AgencyBankingClient;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DockerHelper" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="testingContext">The testing context.</param>
        public DockerHelper(){
            this.TestingContext = new TestingContext();
            StringSerialiser.Initialise((IStringSerialiser)new SystemTextJsonSerializer(SystemTextJsonSerializer.GetDefaultJsonSerializerOptions()));
        }

        #endregion

        #region Methods

        public override async Task CreateSubscriptions(CancellationToken cancellationToken){
            List<(String streamName, String groupName, Int32 maxRetries)> subscriptions = new();
            subscriptions.AddRange(MessagingService.IntegrationTesting.Helpers.SubscriptionsHelper.GetSubscriptions());
            subscriptions.AddRange(TransactionProcessor.IntegrationTesting.Helpers.SubscriptionsHelper.GetSubscriptions());
            subscriptions.AddRange(FileProcessor.IntegrationTesting.Helpers.SubscriptionsHelper.GetSubscriptions());
            
            foreach ((String streamName, String groupName, Int32 maxRetries) subscription in subscriptions)
            {
                var x = subscription;
                x.maxRetries = 2;
                await this.CreatePersistentSubscription(x);
            }
        }

        public override ContainerBuilder SetupFileProcessorContainer() {

            var variables = new Dictionary<String, String>()
            {
                { "AppSettings:FileProfilePollingWindowInSeconds", "5" }
            };

            this.AdditionalVariables[ContainerType.FileProcessor] = variables;
            return base.SetupFileProcessorContainer()
                .WithOutputConsumer(Consume.RedirectStdoutAndStderrToConsole());
        }

        public override ContainerBuilder SetupTransactionProcessorContainer()
        {
            if (this.IntegrationTestCertificate is null)
            {
                throw new InvalidOperationException("Integration test certificate was not initialised before TransactionProcessor container setup.");
            }

            this.AdditionalVariables[ContainerType.TransactionProcessor] = new Dictionary<String, String>
            {
                ["OperatorConfiguration:AgencyBanking:Url"] = $"http://{this.TestHostContainerName}:{DockerPorts.TestHostPort}/api/agencybanking",
                ["ServiceOptions:KestrelOptions:Path"] = this.IntegrationTestCertificate.GetContainerCertificatePath(),
                ["ServiceOptions:KestrelOptions:Password"] = this.IntegrationTestCertificate.Password,
                ["HealthMonitoring:Service:HealthUrl"] = $"https://localhost:{DockerPorts.TransactionProcessorDockerPort}/healthui"
            };

            this.SkipHealthChecks = true;
            Dictionary<String, String> environmentVariables = this.GetCommonEnvironmentVariables();
            environmentVariables.Add("urls", $"https://*:{DockerPorts.TransactionProcessorDockerPort}");
            environmentVariables.Add("AppSettings:SubscriptionFilter", "TransactionProcessor");
            environmentVariables.Add("OperatorConfiguration:Safaricom:Url", $"http://{this.TestHostContainerName}:{DockerPorts.TestHostPort}/api/safaricom");
            environmentVariables.Add("OperatorConfiguration:PataPawaPostPay:Url", $"http://{this.TestHostContainerName}:{DockerPorts.TestHostPort}/PataPawaPostPayService/basichttp");
            environmentVariables.Add("OperatorConfiguration:PataPawaPrePay:Url", $"http://{this.TestHostContainerName}:{DockerPorts.TestHostPort}/api/patapawaprepay");
            environmentVariables.Add("ConnectionStrings:TransactionProcessorReadModel", this.SetConnectionString("TransactionProcessorReadModel", this.UseSecureSqlServerDatabase));

            foreach (KeyValuePair<String, String> additionalVariable in this.GetAdditionalVariables(ContainerType.TransactionProcessor))
            {
                environmentVariables.Add(additionalVariable.Key, additionalVariable.Value);
            }

            (String imageName, Boolean useLatest) imageDetails = this.GetImageDetails(ContainerType.TransactionProcessor).GetValueOrThrow();
            return new ContainerBuilder(imageDetails.imageName)
                .WithName(this.TransactionProcessorContainerName)
                .WithEnvironment(environmentVariables)
                .MountHostFolder(this.HostTraceFolder)
                .WithPortBinding(DockerPorts.TransactionProcessorDockerPort, true)
                .WithResourceMapping(this.IntegrationTestCertificate.CertificateDirectory,
                                     this.IntegrationTestCertificate.ContainerCertificateDirectory);
        }

        public override ContainerBuilder SetupMessagingServiceContainer()
        {
            if (this.IntegrationTestCertificate is null)
            {
                throw new InvalidOperationException("Integration test certificate was not initialised before MessagingService container setup.");
            }

            this.SkipHealthChecks = true;
            this.AdditionalVariables[ContainerType.MessagingService] = new Dictionary<String, String>
            {
                ["MessagingService:Https:CertificatePath"] = this.IntegrationTestCertificate.GetContainerCertificatePath(),
                ["MessagingService:Https:CertificatePassword"] = this.IntegrationTestCertificate.Password
            };

            return base.SetupMessagingServiceContainer()
                .WithResourceMapping(this.IntegrationTestCertificate.CertificateDirectory,
                                     this.IntegrationTestCertificate.ContainerCertificateDirectory);
        }

        public override Dictionary<String, String> GetCommonEnvironmentVariables()
        {
            Dictionary<String, String> environmentVariables = base.GetCommonEnvironmentVariables();
            environmentVariables["AppSettings:TransactionProcessorApi"] = $"https://{this.TransactionProcessorContainerName}:{DockerPorts.TransactionProcessorDockerPort}";
            environmentVariables["AppSettings:MessagingServiceApi"] = $"https://{this.MessagingServiceContainerName}:{DockerPorts.MessagingServiceDockerPort}";
            return environmentVariables;
        }

        public String AccessToken;

        protected override String GenerateKurrentDbConnectionString(String userName = "admin", String password = "changeit")
        {
            String eventStoreAddress = $"esdb://{userName}:{password}@{this.KurrentDbContainerName}:{DockerPorts.KurrentDbHttpDockerPort}?tls=false&defaultDeadline=240000";

            return eventStoreAddress;
        }

        public override async Task StartContainersForScenarioRun(String scenarioName, DockerServices dockerServices, CancellationToken cancellationToken)
        {
            
            await base.StartContainersForScenarioRun(scenarioName, dockerServices, cancellationToken);

            // Setup the base address resolvers
            String SecurityServiceBaseAddressResolver(String api) => $"https://127.0.0.1:{this.SecurityServicePort}";
            String FileProcessorBaseAddressResolver(String api) => $"http://127.0.0.1:{this.FileProcessorPort}";
            String TransactionProcessorBaseAddressResolver(String api) => $"http://127.0.0.1:{this.TransactionProcessorPort}";
            String TestHostServiceBaseAddressResolver(String api) => $"http://127.0.0.1:{this.TestHostServicePort}";
            
            HttpClientHandler clientHandler = new HttpClientHandler{
                                                                       ServerCertificateCustomValidationCallback = (message,
                                                                                                                    certificate2,
                                                                                                                    arg3,
                                                                                                                    arg4) => {
                                                                                                                       return true;
                                                                                                                   }
                                                                   };

            var httpMessageHandler = new SocketsHttpHandler{
                                                               SslOptions ={
                                                                               RemoteCertificateValidationCallback = (sender,
                                                                                                                      certificate,
                                                                                                                      chain,
                                                                                                                      errors) => true,
                                                                           }
                                                           };
            HttpClient httpClient = new HttpClient(httpMessageHandler);
            this.SecurityServiceClient = new SecurityServiceClient(SecurityServiceBaseAddressResolver, httpClient, Serialise, Deserialise);
            this.FileProcessorClient = new FileProcessorClient(FileProcessorBaseAddressResolver, httpClient, Serialise, Deserialise);
            this.TransactionProcessorClient = new TransactionProcessorClient(TransactionProcessorBaseAddressResolver, httpClient, Serialise, Deserialise);
            this.TestHostHttpClient = new HttpClient(clientHandler);
            this.TestHostHttpClient.BaseAddress = new Uri($"http://127.0.0.1:{this.TestHostServicePort}");
            this.AgencyBankingClient = new AgencyBankingClient(TestHostServiceBaseAddressResolver, httpClient, Serialise_CamelCase, this.Deserialise_CamelCase);
            
            this.ProjectionManagementClient = new KurrentDBProjectionManagementClient(this.ConfigureKurrentDbSettings());

            TransactionProcessing.UnionResults.Result<SecurityService.DataTransferObjects.TokenResponse> bootstrapToken = await this.SecurityServiceClient.GetToken("management-bootstrap", "management-bootstrap-secret", CancellationToken.None);
            if (bootstrapToken.IsSuccess() == false || String.IsNullOrWhiteSpace(bootstrapToken.GetValueOrThrow()?.AccessToken))
            {
                throw new InvalidOperationException("Unable to obtain the integration-test management bootstrap token.");
            }
            this.AccessToken = bootstrapToken.GetValueOrThrow().AccessToken;
        }

        String Serialise(Object arg)
        {
            return StringSerialiser.Serialise<Object>(arg, new SerialiserOptions(SerialiserPropertyFormat.SnakeCase));
        }

        Object Deserialise(String arg, Type type)
        {
            return StringSerialiser.DeserializeObject<Object>(arg, type, new SerialiserOptions(SerialiserPropertyFormat.SnakeCase));
        }

        String Serialise_CamelCase(Object arg)
        {
            return StringSerialiser.Serialise<Object>(arg, new SerialiserOptions(SerialiserPropertyFormat.CamelCase));
        }

        Object Deserialise_CamelCase(String arg, Type type)
        {
            return StringSerialiser.DeserializeObject<Object>(arg, type, new SerialiserOptions(SerialiserPropertyFormat.CamelCase));
        }

        #endregion
    }
}
