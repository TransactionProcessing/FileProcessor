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
    using System.Threading;
    using Common;
    using DataTransferObjects.Responses;
    using Newtonsoft.Json;
    using Shared.IntegrationTesting;
    using Shouldly;
    using TechTalk.SpecFlow;
    using SpecflowTableHelper = Common.SpecflowTableHelper;

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
        public async Task WhenIGetTheImportLogsBetweenAndTheFollowingDataIsReturned(string estateName, string startDate, string endDate, Table table){
            foreach (TableRow tableRow in table.Rows){
                await Retry.For(async () => {
                                    FileImportLogList importLogList = await this.GetFileImportLogList(estateName, startDate, endDate, CancellationToken.None);


                                    DateTime importLogDateTime = SpecflowTableHelper.GetDateForDateString(SpecflowTableHelper.GetStringRowValue(tableRow, "ImportLogDate"), DateTime.Now);
                                    Int32 fileCount = SpecflowTableHelper.GetIntValue(tableRow, "FileCount");


                                    // Find the import log now
                                    FileImportLog? importLog = importLogList.FileImportLogs.SingleOrDefault(fil => fil.ImportLogDate == importLogDateTime.Date && fil.FileCount == fileCount);

                                    importLog.ShouldNotBeNull();
                                });
            }
        }

        private async Task<FileDetails> GetFile(String estateName, Guid fileId, CancellationToken cancellationToken)
        {
            var estateDetails = this.TestingContext.GetEstateDetails(estateName);
            
            var fileDetails = await this.TestingContext.DockerHelper.FileProcessorClient.GetFile(this.TestingContext.AccessToken, estateDetails.EstateId, fileId, cancellationToken);
            fileDetails.ShouldNotBeNull();
            
            return fileDetails;
        }

        private async Task<FileImportLogList> GetFileImportLogList(String estateName,
                                                               String startDate,
                                                               String endDate, 
                                                               CancellationToken cancellationToken)
        {
            var queryStartDate = SpecflowTableHelper.GetDateForDateString(startDate, DateTime.Now);
            var queryEndDate = SpecflowTableHelper.GetDateForDateString(endDate, DateTime.Now);
            var estateDetails = this.TestingContext.GetEstateDetails(estateName);

            var importLogList = await this.TestingContext.DockerHelper.FileProcessorClient.GetFileImportLogs(this.TestingContext.AccessToken,
                                                                                                             estateDetails.EstateId,
                                                                                                             queryStartDate,
                                                                                                             queryEndDate,
                                                                                                             null,
                                                                                                             cancellationToken);
            importLogList.ShouldNotBeNull();
            importLogList.FileImportLogs.ShouldNotBeNull();
            importLogList.FileImportLogs.ShouldNotBeEmpty();

            return importLogList;
        }

        private async Task<FileImportLog> GetFileImportLog(String estateName,
                                                               Guid fileImportLogId,
                                                               CancellationToken cancellationToken)
        {
            var estateDetails = this.TestingContext.GetEstateDetails(estateName);

            var fileImportLog = await this.TestingContext.DockerHelper.FileProcessorClient.GetFileImportLog(this.TestingContext.AccessToken,
                                                                                                            fileImportLogId,
                                                                                                            estateDetails.EstateId,
                                                                                                            null,
                                                                                                            cancellationToken);
            fileImportLog.ShouldNotBeNull();
            fileImportLog.Files.ShouldNotBeNull();
            fileImportLog.Files.ShouldNotBeEmpty();

            return fileImportLog;
        }

        [When(@"I get the '(.*)' import log for '(.*)' the following file information is returned")]
        public async Task WhenIGetTheImportLogForTheFollowingFileInformationIsReturned(string estateName, string startDate, Table table)
        {
            EstateDetails estateDetails = this.TestingContext.GetEstateDetails(estateName);
            FileImportLogList importLogList = await this.GetFileImportLogList(estateName, startDate, startDate, CancellationToken.None);

            importLogList.FileImportLogs.ShouldHaveSingleItem();

            var fileImportLog = await this.GetFileImportLog(estateName, importLogList.FileImportLogs.Single().FileImportLogId, CancellationToken.None);

            foreach (TableRow tableRow in table.Rows)
            {
                //| MerchantName    | OriginalFileName | NumberOfLines |
                var merchantName = SpecflowTableHelper.GetStringRowValue(tableRow, "MerchantName");
                var originalFileName = SpecflowTableHelper.GetStringRowValue(tableRow, "OriginalFileName");

                var merchantId = this.TestingContext.GetEstateDetails(estateName).GetMerchantId(merchantName);

                var file = fileImportLog.Files.SingleOrDefault(f => f.OriginalFileName == originalFileName && f.MerchantId == merchantId);

                file.ShouldNotBeNull();

                estateDetails.AddFileImportLogFile(file);
            }
        }

        [When(@"I get the file '(.*)' for Estate '(.*)' the following file information is returned")]
        public async Task WhenIGetTheFileForEstateTheFollowingFileInformationIsReturned(string fileName, string estateName, Table table)
        {
            EstateDetails estateDetails = this.TestingContext.GetEstateDetails(estateName);

            Guid fileId = estateDetails.GetFileId(fileName);
            
            TableRow tableRow = table.Rows.First();
            Boolean processingCompleted = SpecflowTableHelper.GetBooleanValue(tableRow, "ProcessingCompleted");
            Int32 numberOfLines = SpecflowTableHelper.GetIntValue(tableRow, "NumberOfLines");
            Int32 totaLines = SpecflowTableHelper.GetIntValue(tableRow, "TotaLines");
            Int32 successfulLines = SpecflowTableHelper.GetIntValue(tableRow, "SuccessfulLines");
            Int32 ignoredLines = SpecflowTableHelper.GetIntValue(tableRow, "IgnoredLines");
            Int32 failedLines = SpecflowTableHelper.GetIntValue(tableRow, "FailedLines");
            Int32 notProcessedLines = SpecflowTableHelper.GetIntValue(tableRow, "NotProcessedLines");

            await Retry.For(async () =>
                            {
                                var fileDetails = await this.GetFile(estateName, fileId, CancellationToken.None);
                                fileDetails.ProcessingCompleted.ShouldBe(processingCompleted);
                                fileDetails.FileLines.Count.ShouldBe(numberOfLines);
                                fileDetails.ProcessingSummary.TotalLines.ShouldBe(totaLines);
                                fileDetails.ProcessingSummary.SuccessfullyProcessedLines.ShouldBe(successfulLines);
                                fileDetails.ProcessingSummary.IgnoredLines.ShouldBe(ignoredLines);
                                fileDetails.ProcessingSummary.FailedLines.ShouldBe(failedLines);
                                fileDetails.ProcessingSummary.NotProcessedLines.ShouldBe(notProcessedLines);
                            }, TimeSpan.FromMinutes(4), TimeSpan.FromSeconds(30));
            }

        [When(@"I get the file '(.*)' for Estate '(.*)' the following file lines are returned")]
        public async Task WhenIGetTheFileForEstateTheFollowingFileLinesAreReturned(string fileName, string estateName, Table table)
        {
            EstateDetails estateDetails = this.TestingContext.GetEstateDetails(estateName);

            Guid fileId = estateDetails.GetFileId(fileName);

            var fileDetails = await this.GetFile(estateName, fileId, CancellationToken.None);

            foreach (TableRow tableRow in table.Rows)
            {
                var lineNumber = SpecflowTableHelper.GetIntValue(tableRow, "LineNumber");
                var lineData = SpecflowTableHelper.GetStringRowValue(tableRow, "Data");
                var processingResult = SpecflowTableHelper.GetEnumValue<FileLineProcessingResult>(tableRow, "Result");

                var lineToVerify = fileDetails.FileLines.SingleOrDefault(fl => fl.LineNumber == lineNumber);
                lineToVerify.ShouldNotBeNull();
                lineToVerify.LineData.ShouldBe(lineData);
                lineToVerify.ProcessingResult.ShouldBe(processingResult);
            }
        }

    }
}
