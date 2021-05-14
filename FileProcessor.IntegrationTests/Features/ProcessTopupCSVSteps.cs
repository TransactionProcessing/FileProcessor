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
    using Shared.IntegrationTesting;
    using Shouldly;
    using TechTalk.SpecFlow;
    using SpecflowTableHelper = Common.SpecflowTableHelper;

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

        [Given(@"I have a safaricom topup file with the following contents")]
        public void GivenIHaveASafaricomTopupFileWithTheFollowingContents(Table table)
        {
            StringBuilder fileBuilder = new StringBuilder();
            
            Int32 currentRow = 1;
            foreach (var row in table.Rows)
            {
                StringBuilder rowBuilder = new StringBuilder();
                foreach (String rowValue in row.Values)
                {
                    rowBuilder.Append($"{rowValue},");
                }
                // remove the trailing comma
                rowBuilder.Remove(rowBuilder.Length - 1, 1);
                if (currentRow < table.Rows.Count)
                {
                    rowBuilder.Append("\n");
                }

                fileBuilder.Append(rowBuilder.ToString());
                currentRow++;
            }
            
            Directory.CreateDirectory("/home/txnproc/specflow");
            Guid fileNameId = Guid.NewGuid();
            String filepath = $"/home/txnproc/specflow/safaricomtopup_{fileNameId.ToString("N")}.txt";
            // Should have the whole file here
            using (StreamWriter sw = new StreamWriter(filepath))
            {
                sw.WriteAsync(fileBuilder.ToString());
            }

            this.TestingContext.UploadFile = filepath;
        }

        [Given(@"I upload this file for processing")]
        public async Task GivenIUploadThisFileForProcessing(Table table)
        {
            var response = await this.UploadFile(table);

            response.StatusCode.ShouldBe(HttpStatusCode.Accepted);
        }

        [Given(@"I upload this file for processing an error should be returned indicating the file is a duplicate")]
        public async Task GivenIUploadThisFileForProcessingAnErrorShouldBeReturnedIndicatingTheFileIsADuplicate(Table table)
        {
            var response = await this.UploadFile(table);
            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            var responseContent = await  response.Content.ReadAsStringAsync(CancellationToken.None);
            responseContent.ShouldContain("Duplicate file",Case.Insensitive);
        }

        private async Task<HttpResponseMessage> UploadFile(Table table)
        {
            var row = table.Rows.First();
            String merchantName = SpecflowTableHelper.GetStringRowValue(row, "MerchantName");
            String fileProfileId = SpecflowTableHelper.GetStringRowValue(row, "FileProfileId");
            String userId = SpecflowTableHelper.GetStringRowValue(row, "UserId");

            var estate = this.TestingContext.GetEstateDetails(row);
            Guid estateId = estate.EstateId;
            var merchantId = estate.GetMerchantId(merchantName);
            String filePath = this.TestingContext.UploadFile;

            var client = new HttpClient();
            var formData = new MultipartFormDataContent();

            var fileContent = new ByteArrayContent(await File.ReadAllBytesAsync(filePath));
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
            formData.Add(fileContent, "file", Path.GetFileName(filePath));
            formData.Add(new StringContent(estateId.ToString()), "request.EstateId");
            formData.Add(new StringContent(merchantId.ToString()), "request.MerchantId");
            formData.Add(new StringContent(fileProfileId), "request.FileProfileId");
            formData.Add(new StringContent(userId), "request.UserId");

            var request = new HttpRequestMessage(HttpMethod.Post, $"http://127.0.0.1:{this.TestingContext.DockerHelper.FileProcessorPort}/api/files")
                          {
                              Content = formData
                          };

            var response = await client.SendAsync(request);

            return response;
        }


        [When(@"As merchant ""(.*)"" on Estate ""(.*)"" I get my transactions (.*) transaction should be returned")]
        public async Task WhenAsMerchantOnEstateIGetMyTransactionsTransactionShouldBeReturned(string merchantName, string estateName, int numberOfTransactions)
        {
            // We will need to give the processing some time to complete
            var estate = this.TestingContext.GetEstateDetails(estateName);
            Guid estateId = estate.EstateId;
            var merchantId = estate.GetMerchantId(merchantName);
            String accessToken = estate.AccessToken;

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
