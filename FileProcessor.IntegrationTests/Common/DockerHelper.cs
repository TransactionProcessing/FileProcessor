using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FileProcessor.IntegrationTests.Common
{
    using System.Net.Http;
    using Client;
    using EventStore.Client;
    using SecurityService.Client;
    using Shared.IntegrationTesting;
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
            this.SecurityServiceClient = new SecurityServiceClient(SecurityServiceBaseAddressResolver, httpClient);
            this.FileProcessorClient = new FileProcessorClient(FileProcessorBaseAddressResolver, httpClient);
            this.TransactionProcessorClient = new TransactionProcessorClient(TransactionProcessorBaseAddressResolver, httpClient);
            this.TestHostHttpClient = new HttpClient(clientHandler);
            this.TestHostHttpClient.BaseAddress = new Uri($"http://127.0.0.1:{this.TestHostServicePort}");

            this.ProjectionManagementClient = new EventStoreProjectionManagementClient(ConfigureEventStoreSettings());
        }
        
        #endregion
    }
}
