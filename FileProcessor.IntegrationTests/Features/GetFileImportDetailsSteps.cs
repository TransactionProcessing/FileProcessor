﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FileProcessor.IntegrationTests.Features
{
    using Common;
    using IntegrationTesting.Helpers.FileProcessor.IntegrationTests.Steps;
    using Reqnroll;
    using ReqnrollExtensions = IntegrationTesting.Helpers.FileProcessor.IntegrationTests.Steps.ReqnrollExtensions;

    [Binding]
    [Scope(Tag = "getfileimportdetails")]
    public class GetFileImportDetailsSteps
    {
        private readonly ScenarioContext ScenarioContext;

        private readonly TestingContext TestingContext;

        private readonly FileProcessorSteps FileProcessorSteps;

        public GetFileImportDetailsSteps(ScenarioContext scenarioContext,
                                         TestingContext testingContext)
        {
            this.ScenarioContext = scenarioContext;
            this.TestingContext = testingContext;
            this.FileProcessorSteps = new FileProcessorSteps(testingContext.DockerHelper.FileProcessorClient);
        }

        [When(@"I get the '(.*)' import logs between '(.*)' and '(.*)' the following data is returned")]
        public async Task WhenIGetTheImportLogsBetweenAndTheFollowingDataIsReturned(string estateName, string startDate, string endDate, DataTable table){

            List<(DateTime, Int32)> expectedImportLogData = table.Rows.ToExpectedImportLogData();
            await this.FileProcessorSteps.WhenIGetTheImportLogsBetweenAndTheFollowingDataIsReturned(this.TestingContext.AccessToken, startDate, endDate, estateName, this.TestingContext.Estates, expectedImportLogData);
        }

        [When(@"I get the '(.*)' import log for '(.*)' the following file information is returned")]
        public async Task WhenIGetTheImportLogForTheFollowingFileInformationIsReturned(string estateName, string startDate, DataTable table){

            List<(String, Guid)> fileDetails = table.Rows.ToFileDetails(estateName, this.TestingContext.Estates);
            await this.FileProcessorSteps.WhenIGetTheImportLogForTheFollowingFileInformationIsReturned(this.TestingContext.AccessToken, estateName, startDate, this.TestingContext.Estates, fileDetails);
        }

        [When(@"I get the file '(.*)' for Estate '(.*)' the following file information is returned")]
        public async Task WhenIGetTheFileForEstateTheFollowingFileInformationIsReturned(string fileName, string estateName, DataTable table){
            ReqnrollExtensions.FileProcessingSummary fileSummary = table.Rows.ToFileProcessingSummary();
            await this.FileProcessorSteps.WhenIGetTheFileForEstateTheFollowingFileInformationIsReturned(this.TestingContext.AccessToken, fileName, estateName, this.TestingContext.Estates, fileSummary);
        }

        [When(@"I get the file '(.*)' for Estate '(.*)' the following file lines are returned")]
        public async Task WhenIGetTheFileForEstateTheFollowingFileLinesAreReturned(string fileName, string estateName, DataTable table){
            List<ReqnrollExtensions.FileLineDetails> fileLineDetails = table.Rows.ToFileLineDetails();
            await this.FileProcessorSteps.WhenIGetTheFileForEstateTheFollowingFileLinesAreReturned(this.TestingContext.AccessToken, fileName, estateName, this.TestingContext.Estates, fileLineDetails);
        }
    }
}
