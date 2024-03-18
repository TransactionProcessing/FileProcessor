using System;

namespace FileProcessor.IntegrationTests.Features
{
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Common;
    using Reqnroll;
    using Shared.IntegrationTesting;
    using Shouldly;

    [Binding]
    [Scope(Tag = "processvouchercsv")]
    public class ProcessVoucherCSVFilesSteps
    {
        private readonly ScenarioContext ScenarioContext;

        private readonly TestingContext TestingContext;

        public ProcessVoucherCSVFilesSteps(ScenarioContext scenarioContext,
                                           TestingContext testingContext)
        {
            this.ScenarioContext = scenarioContext;
            this.TestingContext = testingContext;
        }
    }
}
