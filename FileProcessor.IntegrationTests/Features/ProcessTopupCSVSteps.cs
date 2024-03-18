using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileProcessor.IntegrationTests.Features
{
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading;
    using Common;
    using Reqnroll;
    using Shared.IntegrationTesting;
    using Shouldly;

    [Binding]
    [Scope(Tag = "processtopupcsv")]
    public class ProcessTopupCSVSteps
    {
        private readonly ScenarioContext ScenarioContext;

        private readonly TestingContext TestingContext;

        public ProcessTopupCSVSteps(ScenarioContext scenarioContext,
                                    TestingContext testingContext)
        {
            this.ScenarioContext = scenarioContext;
            this.TestingContext = testingContext;
        }

        // Github Actions - Environment vars 
        // CI - always try when under CI
        // HOME - action home
        
        
        
    }
}
