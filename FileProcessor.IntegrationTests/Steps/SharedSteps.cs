using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileProcessor.IntegrationTests.Steps
{
    using System.IO;
    using System.Linq.Expressions;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading;
    using Common;
    using DataTransferObjects;
    using DataTransferObjects.Responses;
    using EstateManagement.DataTransferObjects.Requests;
    using EstateManagement.DataTransferObjects.Responses;
    using EstateManagement.DataTransferObjects;
    using EstateManagement.IntegrationTesting.Helpers;
    using IntegrationTesting.Helpers;
    using IntegrationTesting.Helpers.FileProcessor.IntegrationTests.Steps;
    using Newtonsoft.Json;
    using SecurityService.DataTransferObjects.Requests;
    using SecurityService.IntegrationTesting.Helpers;
    using Shared.IntegrationTesting;
    using Shouldly;
    using TransactionProcessor.DataTransferObjects;
    using Contract = Common.Contract;
    using Product = Common.Product;
    using Newtonsoft.Json.Linq;
    using System.Text.Json;
    using Reqnroll;

    [Binding]
    [Scope(Tag = "shared")]
    public class SharedSteps
    {
        private readonly ScenarioContext ScenarioContext;

        private readonly TestingContext TestingContext;

        private readonly SecurityServiceSteps SecurityServiceSteps;

        private readonly EstateManagementSteps EstateManagementSteps;

        private readonly FileProcessorSteps FileProcessorSteps;

        public SharedSteps(ScenarioContext scenarioContext,
                           TestingContext testingContext)
        {
            ScenarioContext = scenarioContext;
            TestingContext = testingContext;
            this.SecurityServiceSteps = new SecurityServiceSteps(testingContext.DockerHelper.SecurityServiceClient);
            this.EstateManagementSteps = new EstateManagementSteps(testingContext.DockerHelper.EstateClient, testingContext.DockerHelper.TestHostHttpClient);
            this.FileProcessorSteps = new FileProcessorSteps(testingContext.DockerHelper.FileProcessorClient);
        }

        [Given(@"I have a file named '(.*)' with the following contents")]
        public void GivenIHaveAFileNamedWithTheFollowingContents(string fileName, DataTable table){
            String fileData = table.Rows.ToFileData();
            this.TestingContext.UploadFile = this.FileProcessorSteps.WriteDileToDisk(fileName, fileData);
        }

        [Given(@"I upload this file for processing")]
        public async Task GivenIUploadThisFileForProcessing(DataTable table)
        {
            String filePath = TestingContext.UploadFile;
            Byte[] fileData = await File.ReadAllBytesAsync(filePath);
            (EstateDetails1, UploadFileRequest) uploadFileRequest = table.Rows.ToUploadFileRequest(this.TestingContext.Estates, fileData);

            await this.FileProcessorSteps.GivenIUploadThisFileForProcessing(this.TestingContext.AccessToken, filePath, fileData, uploadFileRequest.Item1, uploadFileRequest.Item2);
        }

        [Given(@"I upload this file for processing an error should be returned indicating the file is a duplicate")]
        public async Task GivenIUploadThisFileForProcessingAnErrorShouldBeReturnedIndicatingTheFileIsADuplicate(DataTable table)
        {
            String filePath = TestingContext.UploadFile;
            Byte[] fileData = await File.ReadAllBytesAsync(filePath);
            (EstateDetails1, UploadFileRequest) uploadFileRequest = table.Rows.ToUploadFileRequest(this.TestingContext.Estates, fileData);
            await this.FileProcessorSteps.GivenIUploadThisFileForProcessingAnErrorShouldBeReturnedIndicatingTheFileIsADuplicate(this.TestingContext.AccessToken, filePath, fileData, uploadFileRequest.Item2);
        }

        [When(@"I get the import log for estate '([^']*)' the date on the import log is '([^']*)'")]
        public async Task WhenIGetTheImportLogForEstateTheDateOnTheImportLogIs(string estateName, string expectedDateString){
            await this.FileProcessorSteps.WhenIGetTheImportLogForEstateTheDateOnTheImportLogIs(this.TestingContext.AccessToken, estateName, expectedDateString, this.TestingContext.Estates);
        }

        //[When(@"As merchant ""(.*)"" on Estate ""(.*)"" I get my transactions (.*) transaction should be returned")]
        //public async Task WhenAsMerchantOnEstateIGetMyTransactionsTransactionShouldBeReturned(string merchantName, string estateName, int numberOfTransactions)
        //{
        //    // We will need to give the processing some time to complete
        //    var estate = this.TestingContext.GetEstateDetails(estateName);
        //    Guid estateId = estate.EstateId;
        //    var merchantId = estate.GetMerchantId(merchantName);
        //    String accessToken = this.TestingContext.AccessToken;

        //    await Retry.For(async () =>
        //    {
        //        var t = await this.TestingContext.DockerHelper.EstateReportingClient
        //                          .GetTransactionsForMerchantByDate(accessToken,
        //                                                            estateId,
        //                                                            merchantId,
        //                                                            DateTime.Now.ToString("yyyyMMdd"),
        //                                                            DateTime.Now.ToString("yyyyMMdd"),
        //                                                            CancellationToken.None).ConfigureAwait(false);

        //        t.ShouldNotBeNull();
        //        t.TransactionDayResponses.ShouldHaveSingleItem();
        //        t.TransactionDayResponses.Single().NumberOfTransactions.ShouldBe(numberOfTransactions);
        //    }, TimeSpan.FromMinutes(10), TimeSpan.FromSeconds(30));
        //}

        [Given(@"I have created the following estates")]
        [When(@"I create the following estates")]
        public async Task WhenICreateTheFollowingEstates(DataTable table)
        {
            List<CreateEstateRequest> requests = table.Rows.ToCreateEstateRequests();
            
            List<EstateResponse> verifiedEstates = await this.EstateManagementSteps.WhenICreateTheFollowingEstates(this.TestingContext.AccessToken, requests);

            foreach (EstateResponse verifiedEstate in verifiedEstates)
            {
                this.TestingContext.AddEstateDetails(verifiedEstate.EstateId, verifiedEstate.EstateName, verifiedEstate.EstateReference);
                this.TestingContext.Logger.LogInformation($"Estate {verifiedEstate.EstateName} created with Id {verifiedEstate.EstateId}");


            }
        }

        [Given(@"I create the following api scopes")]
        public async Task GivenICreateTheFollowingApiScopes(DataTable table)
        {
            List<CreateApiScopeRequest> requests = table.Rows.ToCreateApiScopeRequests();
            await this.SecurityServiceSteps.GivenICreateTheFollowingApiScopes(requests);
        }
        
        [Given(@"I have assigned the following  operator to the merchants")]
        [When(@"I assign the following  operator to the merchants")]
        public async Task WhenIAssignTheFollowingOperatorToTheMerchants(DataTable table){
            var estates = this.TestingContext.Estates.Select(e => e.EstateDetails).ToList();
            List<(EstateDetails, Guid, AssignOperatorRequest)> requests = table.Rows.ToAssignOperatorRequests(estates);

            List<(EstateDetails, MerchantOperatorResponse)> results = await this.EstateManagementSteps.WhenIAssignTheFollowingOperatorToTheMerchants(this.TestingContext.AccessToken, requests);

            foreach ((EstateDetails, MerchantOperatorResponse) result in results)
            {
                this.TestingContext.Logger.LogInformation($"Operator {result.Item2.Name} assigned to Estate {result.Item1.EstateName}");
            }
        }
        
        [Given(@"I have created the following operators")]
        [When(@"I create the following operators")]
        public async Task WhenICreateTheFollowingOperators(DataTable table)
        {
            var estates = this.TestingContext.Estates.Select(e => e.EstateDetails).ToList();
            List<(EstateDetails estate, CreateOperatorRequest request)> requests = table.Rows.ToCreateOperatorRequests(estates);

            List<(Guid, EstateOperatorResponse)> results = await this.EstateManagementSteps.WhenICreateTheFollowingOperators(this.TestingContext.AccessToken, requests);

            foreach ((Guid, EstateOperatorResponse) result in results)
            {
                this.TestingContext.Logger.LogInformation($"Operator {result.Item2.Name} created with Id {result.Item2.OperatorId} for Estate {result.Item1}");
            }
        }

        [Given("I create the following merchants")]
        [When(@"I create the following merchants")]
        public async Task WhenICreateTheFollowingMerchants(DataTable table)
        {
            var estates = this.TestingContext.Estates.Select(e => e.EstateDetails).ToList();
            List<(EstateDetails estate, CreateMerchantRequest)> requests = table.Rows.ToCreateMerchantRequests(estates);

            List<MerchantResponse> verifiedMerchants = await this.EstateManagementSteps.WhenICreateTheFollowingMerchants(this.TestingContext.AccessToken, requests);

            foreach (MerchantResponse verifiedMerchant in verifiedMerchants)
            {
                EstateDetails estateDetails = estates.SingleOrDefault(e => e.EstateId==verifiedMerchant.EstateId);
                estateDetails.AddMerchant(verifiedMerchant);
                this.TestingContext.Logger.LogInformation($"Merchant {verifiedMerchant.MerchantName} created with Id {verifiedMerchant.MerchantId} for Estate {estateDetails.EstateName}");
            }
        }

        [Given(@"I create a contract with the following values")]
        public async Task GivenICreateAContractWithTheFollowingValues(DataTable table)
        {
            var estates = this.TestingContext.Estates.Select(e => e.EstateDetails).ToList();
            List<(EstateDetails, CreateContractRequest)> requests = table.Rows.ToCreateContractRequests(estates);
            List<ContractResponse> responses = await this.EstateManagementSteps.GivenICreateAContractWithTheFollowingValues(this.TestingContext.AccessToken, requests);
        }

        [When(@"I create the following Products")]
        public async Task WhenICreateTheFollowingProducts(DataTable table)
        {
            var estates = this.TestingContext.Estates.Select(e => e.EstateDetails).ToList();

            List<(EstateDetails, EstateManagement.IntegrationTesting.Helpers.Contract, AddProductToContractRequest)> requests = table.Rows.ToAddProductToContractRequest(estates);
            await this.EstateManagementSteps.WhenICreateTheFollowingProducts(this.TestingContext.AccessToken, requests);
        }

        [When(@"I add the following Transaction Fees")]
        public async Task WhenIAddTheFollowingTransactionFees(DataTable table)
        {
            var estates = this.TestingContext.Estates.Select(e => e.EstateDetails).ToList();
            List<(EstateDetails, EstateManagement.IntegrationTesting.Helpers.Contract, EstateManagement.IntegrationTesting.Helpers.Product, AddTransactionFeeForProductToContractRequest)> requests = table.Rows.ToAddTransactionFeeForProductToContractRequests(estates);
            await this.EstateManagementSteps.WhenIAddTheFollowingTransactionFees(this.TestingContext.AccessToken, requests);
        }

        [Given(@"the following api resources exist")]
        public async Task GivenTheFollowingApiResourcesExist(DataTable table)
        {
            List<CreateApiResourceRequest> requests = table.Rows.ToCreateApiResourceRequests();
            await this.SecurityServiceSteps.GivenTheFollowingApiResourcesExist(requests);
        }

        [Given(@"the following clients exist")]
        public async Task GivenTheFollowingClientsExist(DataTable table)
        {
            List<CreateClientRequest> requests = table.Rows.ToCreateClientRequests();
            List<(String clientId, String secret, List<String> allowedGrantTypes)> clients = await this.SecurityServiceSteps.GivenTheFollowingClientsExist(requests);
            foreach ((String clientId, String secret, List<String> allowedGrantTypes) client in clients)
            {
                this.TestingContext.AddClientDetails(client.clientId, client.secret, String.Join(",", client.allowedGrantTypes));
            }
        }

        [Given(@"I have a token to access the estate management and transaction processor resources")]
        public async Task GivenIHaveATokenToAccessTheEstateManagementAndTransactionProcessorResources(DataTable table)
        {
            DataTableRow firstRow = table.Rows.First();
            String clientId = ReqnrollTableHelper.GetStringRowValue(firstRow, "ClientId");
            ClientDetails clientDetails = this.TestingContext.GetClientDetails(clientId);

            this.TestingContext.AccessToken = await this.SecurityServiceSteps.GetClientToken(clientDetails.ClientId, clientDetails.ClientSecret, CancellationToken.None);
        }

        [Given(@"I have assigned the following devices to the merchants")]
        public async Task GivenIHaveAssignedTheFollowingDevicesToTheMerchants(DataTable table)
        {
            var estates = this.TestingContext.Estates.Select(e => e.EstateDetails).ToList();
            List<(EstateDetails, Guid, AddMerchantDeviceRequest)> requests = table.Rows.ToAddMerchantDeviceRequests(estates);

            List<(EstateDetails, MerchantResponse, String)> results = await this.EstateManagementSteps.GivenIHaveAssignedTheFollowingDevicesToTheMerchants(this.TestingContext.AccessToken, requests);
            foreach ((EstateDetails, MerchantResponse, String) result in results)
            {
                this.TestingContext.Logger.LogInformation($"Device {result.Item3} assigned to Merchant {result.Item2.MerchantName} Estate {result.Item1.EstateName}");
            }
        }

        [When(@"I add the following contracts to the following merchants")]
        public async Task WhenIAddTheFollowingContractsToTheFollowingMerchants(DataTable table)
        {
            List<EstateDetails> estates = this.TestingContext.Estates.Select(e => e.EstateDetails).ToList();
            List<(EstateDetails, Guid, Guid)> requests = table.Rows.ToAddContractToMerchantRequests(estates);
            await this.EstateManagementSteps.WhenIAddTheFollowingContractsToTheFollowingMerchants(this.TestingContext.AccessToken, requests);
        }

        private async Task<Decimal> GetMerchantBalance(Guid merchantId)
        {
            JsonElement jsonElement = (JsonElement)await this.TestingContext.DockerHelper.ProjectionManagementClient.GetStateAsync<dynamic>("MerchantBalanceProjection", $"MerchantBalance-{merchantId:N}");
            JObject jsonObject = JObject.Parse(jsonElement.GetRawText());
            decimal balanceValue = jsonObject.SelectToken("merchant.balance").Value<decimal>();
            return balanceValue;
        }

        [Given(@"I make the following manual merchant deposits")]
        public async Task GivenIMakeTheFollowingManualMerchantDeposits(DataTable table)
        {
            var estates = this.TestingContext.Estates.Select(e => e.EstateDetails).ToList();
            List<(EstateDetails, Guid, MakeMerchantDepositRequest)> requests = table.Rows.ToMakeMerchantDepositRequest(estates);

            foreach ((EstateDetails, Guid, MakeMerchantDepositRequest) request in requests)
            {
                Decimal previousMerchantBalance = await this.GetMerchantBalance(request.Item2);

                await this.EstateManagementSteps.GivenIMakeTheFollowingManualMerchantDeposits(this.TestingContext.AccessToken, request);

                await Retry.For(async () => {
                    Decimal currentMerchantBalance = await this.GetMerchantBalance(request.Item2);

                    currentMerchantBalance.ShouldBe(previousMerchantBalance + request.Item3.Amount);

                    this.TestingContext.Logger.LogInformation($"Deposit Reference {request.Item3.Reference} made for Merchant Id {request.Item2}");
                                });
            }
        }
    }
}
