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
    using DataTransferObjects;
    using Shared.IntegrationTesting;
    using Shouldly;
    using TechTalk.SpecFlow;
    using SpecflowTableHelper = Common.SpecflowTableHelper;

    [Binding]
    [Scope(Tag = "shared")]
    public class SharedSteps
    {
        private readonly ScenarioContext ScenarioContext;

        private readonly TestingContext TestingContext;

        public SharedSteps(ScenarioContext scenarioContext,
                           TestingContext testingContext)
        {
            this.ScenarioContext = scenarioContext;
            this.TestingContext = testingContext;
        }

        [Given(@"I have a file named '(.*)' with the following contents")]
        public void GivenIHaveAFileNamedWithTheFollowingContents(String fileName, Table table)
        {
            this.TestingContext.GenerateFile(fileName, table);
        }

        [Given(@"I upload this file for processing")]
        public async Task GivenIUploadThisFileForProcessing(Table table)
        {
            var fileId = await this.UploadFile(table);

            fileId.ShouldNotBe(Guid.Empty);
        }

        [Given(@"I upload this file for processing an error should be returned indicating the file is a duplicate")]
        public async Task GivenIUploadThisFileForProcessingAnErrorShouldBeReturnedIndicatingTheFileIsADuplicate(Table table)
        {
            Should.Throw<Exception>(async () =>
                                    {
                                        await this.UploadFile(table);
                                    });
        }

        private async Task<Guid> UploadFile(Table table)
        {
            var row = table.Rows.First();
            String merchantName = SpecflowTableHelper.GetStringRowValue(row, "MerchantName");
            String fileProfileId = SpecflowTableHelper.GetStringRowValue(row, "FileProfileId");
            String userId = SpecflowTableHelper.GetStringRowValue(row, "UserId");

            var estate = this.TestingContext.GetEstateDetails(row);
            Guid estateId = estate.EstateId;
            var merchantId = estate.GetMerchantId(merchantName);
            String filePath = this.TestingContext.UploadFile;
            var fileData = await File.ReadAllBytesAsync(filePath);

            UploadFileRequest uploadFileRequest = new UploadFileRequest
                                                  {
                                                      EstateId = estateId,
                                                      FileProfileId = Guid.Parse(fileProfileId),
                                                      MerchantId = merchantId,
                                                      UserId = Guid.Parse(userId)
                                                  };
            
            var fileId = await this.TestingContext.DockerHelper.FileProcessorClient.UploadFile(this.TestingContext.AccessToken,
                                                                            Path.GetFileName(filePath),
                                                                            fileData,
                                                                            uploadFileRequest,
                                                                            CancellationToken.None);

            // Now we need to wait some time to let the file be processed
            await Task.Delay(TimeSpan.FromMinutes(1));

            return fileId;
        }


        [When(@"As merchant ""(.*)"" on Estate ""(.*)"" I get my transactions (.*) transaction should be returned")]
        public async Task WhenAsMerchantOnEstateIGetMyTransactionsTransactionShouldBeReturned(string merchantName, string estateName, int numberOfTransactions)
        {
            // We will need to give the processing some time to complete
            var estate = this.TestingContext.GetEstateDetails(estateName);
            Guid estateId = estate.EstateId;
            var merchantId = estate.GetMerchantId(merchantName);
            String accessToken = this.TestingContext.AccessToken;

            await Retry.For(async () =>
            {
                var t = await this.TestingContext.DockerHelper.EstateReportingClient
                                  .GetTransactionsForMerchantByDate(accessToken,
                                                                    estateId,
                                                                    merchantId,
                                                                    DateTime.Now.ToString("yyyyMMdd"),
                                                                    DateTime.Now.ToString("yyyyMMdd"),
                                                                    CancellationToken.None).ConfigureAwait(false);

                t.ShouldNotBeNull();
                t.TransactionDayResponses.ShouldHaveSingleItem();
                t.TransactionDayResponses.Single().NumberOfTransactions.ShouldBe(numberOfTransactions);
            }, TimeSpan.FromMinutes(5), TimeSpan.FromSeconds(30));
        }
    }
}
