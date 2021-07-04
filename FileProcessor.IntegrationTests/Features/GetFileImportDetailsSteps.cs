using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileProcessor.IntegrationTests.Features
{
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using Common;
    using DataTransferObjects.Responses;
    using Newtonsoft.Json;
    using Shouldly;
    using TechTalk.SpecFlow;

    [Binding]
    [Scope(Tag = "getfileimportdetails")]
    public class GetFileImportDetailsSteps
    {
        private readonly ScenarioContext ScenarioContext;

        private readonly TestingContext TestingContext;

        public GetFileImportDetailsSteps(ScenarioContext scenarioContext,
                                         TestingContext testingContext)
        {
            this.ScenarioContext = scenarioContext;
            this.TestingContext = testingContext;
        }

        [When(@"I get the '(.*)' import log for '(.*)' the following data is returned")]
        public async Task WhenIGetTheImportLogForTheFollowingDataIsReturned(String estateName,String date, Table table)
        {
            var queryDate = SpecflowTableHelper.GetDateForDateString(date, DateTime.Now);
            var estateDetails = this.TestingContext.GetEstateDetails(estateName);

            String requestUri =
                $"{this.TestingContext.DockerHelper.FileProcessorClient.BaseAddress}api/fileImportLogs/{estateDetails.EstateId}/?startDate={queryDate.Date:yyyy-MM-dd}&endDate={queryDate.Date:yyyy-MM-dd}";
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, requestUri);
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", this.TestingContext.AccessToken);

            var responseMessage = await this.TestingContext.DockerHelper.FileProcessorClient.SendAsync(requestMessage);

            responseMessage.StatusCode.ShouldBe(HttpStatusCode.OK);
            var content = await responseMessage.Content.ReadAsStringAsync();
            content.ShouldNotBeNull();
            content.ShouldNotBeEmpty();

            var importLogList = JsonConvert.DeserializeObject<FileImportLogList>(content);
            importLogList.ShouldNotBeNull();
            importLogList.FileImportLogs.ShouldNotBeNull();
            importLogList.FileImportLogs.ShouldNotBeEmpty();

            foreach (TableRow tableRow in table.Rows)
            {
                var importLogDateTime = SpecflowTableHelper.GetDateForDateString(SpecflowTableHelper.GetStringRowValue(tableRow, "ImportLogDate"), DateTime.Now);
                var fileCount = SpecflowTableHelper.GetIntValue(tableRow, "FileCount");

                // Find the import log now
                var importLog = importLogList.FileImportLogs.SingleOrDefault(fil => fil.ImportLogDate == importLogDateTime.Date && fil.FileCount == fileCount);

                importLog.ShouldNotBeNull();
            }
        }

    }
}
