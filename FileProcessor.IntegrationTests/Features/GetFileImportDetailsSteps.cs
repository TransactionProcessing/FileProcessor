using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileProcessor.IntegrationTests.Features
{
    using System.Configuration;
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

        [When(@"I get the '(.*)' import logs between '(.*)' and '(.*)' the following data is returned")]
        public async Task WhenIGetTheImportLogsBetweenAndTheFollowingDataIsReturned(string estateName, string startDate, string endDate, Table table)
        {
            FileImportLogList importLogList = await this.GetFileImportLogList(estateName, startDate, endDate);

            foreach (TableRow tableRow in table.Rows)
            {
                DateTime importLogDateTime = SpecflowTableHelper.GetDateForDateString(SpecflowTableHelper.GetStringRowValue(tableRow, "ImportLogDate"), DateTime.Now);
                Int32 fileCount = SpecflowTableHelper.GetIntValue(tableRow, "FileCount");

                // Find the import log now
                FileImportLog? importLog = importLogList.FileImportLogs.SingleOrDefault(fil => fil.ImportLogDate == importLogDateTime.Date && fil.FileCount == fileCount);

                importLog.ShouldNotBeNull();
            }
        }

        private async Task<FileImportLogList> GetFileImportLogList(String estateName,
                                                               String startDate,
                                                               String endDate)
        {
            var queryStartDate = SpecflowTableHelper.GetDateForDateString(startDate, DateTime.Now);
            var queryEndDate = SpecflowTableHelper.GetDateForDateString(endDate, DateTime.Now);
            var estateDetails = this.TestingContext.GetEstateDetails(estateName);

            String requestUri =
                $"{this.TestingContext.DockerHelper.FileProcessorClient.BaseAddress}api/fileImportLogs?estateId={estateDetails.EstateId}&startDate={queryStartDate.Date:yyyy-MM-dd}&endDate={queryEndDate.Date:yyyy-MM-dd}";
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
            return importLogList;
        }

        private async Task<FileImportLog> GetFileImportLog(String estateName,
                                                               Guid fileImportLogId)
        {
            var estateDetails = this.TestingContext.GetEstateDetails(estateName);

            String requestUri =
                $"{this.TestingContext.DockerHelper.FileProcessorClient.BaseAddress}api/fileImportLogs/{fileImportLogId}?estateId={estateDetails.EstateId}";
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, requestUri);
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", this.TestingContext.AccessToken);

            var responseMessage = await this.TestingContext.DockerHelper.FileProcessorClient.SendAsync(requestMessage);

            responseMessage.StatusCode.ShouldBe(HttpStatusCode.OK);
            var content = await responseMessage.Content.ReadAsStringAsync();
            content.ShouldNotBeNull();
            content.ShouldNotBeEmpty();

            var fileImportLog = JsonConvert.DeserializeObject<FileImportLog>(content);
            fileImportLog.ShouldNotBeNull();
            fileImportLog.Files.ShouldNotBeNull();
            fileImportLog.Files.ShouldNotBeEmpty();
            return fileImportLog;
        }

        [When(@"I get the '(.*)' import log for '(.*)' the following file information is returned")]
        public async Task WhenIGetTheImportLogForTheFollowingFileInformationIsReturned(string estateName, string startDate, Table table)
        {
            FileImportLogList importLogList = await this.GetFileImportLogList(estateName, startDate, startDate);

            importLogList.FileImportLogs.ShouldHaveSingleItem();

            var fileImportLog = await this.GetFileImportLog(estateName, importLogList.FileImportLogs.Single().FileImportLogId);

            foreach (TableRow tableRow in table.Rows)
            {
                //| MerchantName    | OriginalFileName | NumberOfLines |
                var merchantName = SpecflowTableHelper.GetStringRowValue(tableRow, "MerchantName");
                var originalFileName = SpecflowTableHelper.GetStringRowValue(tableRow, "OriginalFileName");
                var numberOfLines = SpecflowTableHelper.GetIntValue(tableRow, "NumberOfLines");

                var merchantId = this.TestingContext.GetEstateDetails(estateName).GetMerchantId(merchantName);

                var file = fileImportLog.Files.SingleOrDefault(f => f.OriginalFileName == originalFileName && f.MerchantId == merchantId);

                file.ShouldNotBeNull();

            }
        }

    }
}
