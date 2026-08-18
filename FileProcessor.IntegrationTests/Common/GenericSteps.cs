using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileProcessor.IntegrationTests.Common
{
    using System.Threading;
    using NLog;
    using Reqnroll;
    using Shared.IntegrationTesting;
    using Shared.Logger;
    
    [Binding]
    [Scope(Tag = "base")]
    public class GenericSteps
    {
        private readonly ScenarioContext ScenarioContext;

        private readonly TestingContext TestingContext;

        public GenericSteps(ScenarioContext scenarioContext,
                            TestingContext testingContext)
        {
            this.ScenarioContext = scenarioContext;
            this.TestingContext = testingContext;
        }

        [BeforeScenario]
        public async Task StartSystem()
        {
            // Initialise a logger
            String scenarioName = this.ScenarioContext.ScenarioInfo.Title.Replace(" ", "");
            NlogLogger logger = new NlogLogger();
            logger.Initialise(LogManager.GetLogger(scenarioName), scenarioName);
            LogManager.AddHiddenAssembly(typeof(NlogLogger).Assembly);

            DockerServices dockerServices = this.GetDockerServicesForScenario();

            this.TestingContext.DockerHelper = new DockerHelper();
            this.TestingContext.DockerHelper.Logger = logger;
            this.TestingContext.Logger = logger;
            this.TestingContext.DockerHelper.RequiredDockerServices = dockerServices;
            this.TestingContext.Logger.LogInformation("About to Start Global Setup");

            await Setup.GlobalSetup(this.TestingContext.DockerHelper);

            this.TestingContext.DockerHelper.DockerCredentials = Setup.DockerCredentials;
            this.TestingContext.DockerHelper.SqlCredentials = Setup.SqlCredentials;
            this.TestingContext.DockerHelper.SqlServerContainerName = "sharedsqlserver";

            this.TestingContext.DockerHelper.SetImageDetails(ContainerType.FileProcessor, ("fileprocessor", false));

            this.TestingContext.Logger = logger;
            this.TestingContext.Logger.LogInformation("About to Start Containers for Scenario Run");
            await this.TestingContext.DockerHelper.StartContainersForScenarioRun(scenarioName, dockerServices).ConfigureAwait(false);
            this.TestingContext.Logger.LogInformation("Containers for Scenario Run Started");
        }

        private DockerServices GetDockerServicesForScenario()
        {
            String scenarioTitle = this.ScenarioContext.ScenarioInfo.Title;

            // File profile CRUD scenarios only need the API, auth service, event store, and SQL.
            // The FileProcessor health check still depends on the TransactionProcessor service, but
            // the rest of the transaction-processing stack is unnecessary for these scenarios.
            if (scenarioTitle.Contains("file profile", StringComparison.OrdinalIgnoreCase))
            {
                return DockerServices.EventStore | DockerServices.FileProcessor | DockerServices.SecurityService |
                       DockerServices.SqlServer | DockerServices.TransactionProcessor;
            }

            return DockerServices.EventStore | DockerServices.FileProcessor | DockerServices.MessagingService |
                   DockerServices.SecurityService | DockerServices.SqlServer | DockerServices.TestHost | DockerServices.TransactionProcessor;
        }

        [AfterScenario]
        public async Task StopSystem(){
            DockerServices shareDockerServices = DockerServices.None;
            
            this.TestingContext.Logger.LogInformation("About to Stop Containers for Scenario Run");
            await this.TestingContext.DockerHelper.StopContainersForScenarioRun(shareDockerServices).ConfigureAwait(false);
            this.TestingContext.Logger.LogInformation("Containers for Scenario Run Stopped");
        }
    }
}
