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
        
        private readonly TestingContext TestingContext;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DockerHelper" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="testingContext">The testing context.</param>
        public DockerHelper() {
            this.TestingContext = new TestingContext();
        }

        #endregion
        
        #region Methods

        public override async Task StartContainersForScenarioRun(String scenarioName)
        {
            await base.StartContainersForScenarioRun(scenarioName);

            // Setup the base address resolvers
            String EstateManagementBaseAddressResolver(String api) => $"http://127.0.0.1:{this.EstateManagementPort}";
            String SecurityServiceBaseAddressResolver(String api) => $"https://127.0.0.1:{this.SecurityServicePort}";
            String FileProcessorBaseAddressResolver(String api) => $"http://127.0.0.1:{this.FileProcessorPort}";
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
            this.FileProcessorClient = new FileProcessorClient(FileProcessorBaseAddressResolver, httpClient);
            this.TransactionProcessorClient = new TransactionProcessorClient(TransactionProcessorBaseAddressResolver, httpClient);
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

            await base.StopContainersForScenarioRun();
        }

        #endregion
    }
}
