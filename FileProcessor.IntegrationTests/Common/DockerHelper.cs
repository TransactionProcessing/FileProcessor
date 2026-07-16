using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;

namespace FileProcessor.IntegrationTests.Common
{
    using Client;
    using EventStore.Client;
    using SecurityService.Client;
    using Shared.IntegrationTesting;
    using Shared.Serialisation;
    using System.Net.Http;
    using TransactionProcessor.Client;

    public class DockerHelper : global::Shared.IntegrationTesting.TestContainers.DockerHelper{
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

        public EventStoreProjectionManagementClient ProjectionManagementClient;

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

        public override async Task CreateSubscriptions(){
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

            this.AdditionalVariables.Add(ContainerType.FileProcessor, variables);
            return base.SetupFileProcessorContainer();
        }
        
        public override async Task StartContainersForScenarioRun(String scenarioName, DockerServices dockerServices){
            
            await base.StartContainersForScenarioRun(scenarioName, dockerServices);

            // Setup the base address resolvers
            String SecurityServiceBaseAddressResolver(String api) => $"https://127.0.0.1:{this.SecurityServicePort}";
            String FileProcessorBaseAddressResolver(String api) => $"http://127.0.0.1:{this.FileProcessorPort}";
            String TransactionProcessorBaseAddressResolver(String api) => $"http://127.0.0.1:{this.TransactionProcessorPort}";

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

            this.ProjectionManagementClient = new EventStoreProjectionManagementClient(ConfigureEventStoreSettings());
        }

        String Serialise(Object arg)
        {
            return StringSerialiser.Serialise<Object>(arg, new SerialiserOptions(SerialiserPropertyFormat.SnakeCase));
        }

        Object Deserialise(String arg, Type type)
        {
            return StringSerialiser.DeserializeObject<Object>(arg, type, new SerialiserOptions(SerialiserPropertyFormat.SnakeCase));
        }

        #endregion
    }
}
