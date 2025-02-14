using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TransactionProcessor.DataTransferObjects.Requests.Contract;
using TransactionProcessor.DataTransferObjects.Requests.Estate;
using TransactionProcessor.DataTransferObjects.Requests.Merchant;
using TransactionProcessor.DataTransferObjects.Requests.Operator;
using TransactionProcessor.DataTransferObjects.Responses.Contract;
using TransactionProcessor.DataTransferObjects.Responses.Estate;
using TransactionProcessor.DataTransferObjects.Responses.Merchant;
using TransactionProcessor.IntegrationTesting.Helpers;
using AssignOperatorRequest = TransactionProcessor.DataTransferObjects.Requests.Estate.AssignOperatorRequest;

namespace FileProcessor.IntegrationTests.Steps
{
    using System.IO;
    using System.Threading;
    using Common;
    using DataTransferObjects;
    using IntegrationTesting.Helpers;
    using IntegrationTesting.Helpers.FileProcessor.IntegrationTests.Steps;
    using Newtonsoft.Json;
    using SecurityService.DataTransferObjects.Requests;
    using SecurityService.IntegrationTesting.Helpers;
    using Shared.IntegrationTesting;
    using Shouldly;
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

        private readonly TransactionProcessorSteps TransactionProcessorSteps;

        private readonly FileProcessorSteps FileProcessorSteps;

        public SharedSteps(ScenarioContext scenarioContext,
                           TestingContext testingContext)
        {
            ScenarioContext = scenarioContext;
            TestingContext = testingContext;
            this.SecurityServiceSteps = new SecurityServiceSteps(testingContext.DockerHelper.SecurityServiceClient);
            this.TransactionProcessorSteps = new TransactionProcessorSteps(testingContext.DockerHelper.TransactionProcessorClient,testingContext.DockerHelper.TestHostHttpClient, testingContext.DockerHelper.ProjectionManagementClient);
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

            var x = JsonConvert.SerializeObject(uploadFileRequest.Item2);
            
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
            
            List<EstateResponse> verifiedEstates = await this.TransactionProcessorSteps.WhenICreateTheFollowingEstatesX(this.TestingContext.AccessToken, requests);

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
            List<(EstateDetails, Guid, TransactionProcessor.DataTransferObjects.Requests.Merchant.AssignOperatorRequest)> requests = table.Rows.ToAssignOperatorRequests(estates);

            List<(EstateDetails, TransactionProcessor.DataTransferObjects.Responses.Merchant.MerchantOperatorResponse)> results = await this.TransactionProcessorSteps.WhenIAssignTheFollowingOperatorToTheMerchants(this.TestingContext.AccessToken, requests);

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

            List<(Guid, EstateOperatorResponse)> results = await this.TransactionProcessorSteps.WhenICreateTheFollowingOperators(this.TestingContext.AccessToken, requests);

            foreach ((Guid, EstateOperatorResponse) result in results)
            {
                this.TestingContext.Logger.LogInformation($"Operator {result.Item2.Name} created with Id {result.Item2.OperatorId} for Estate {result.Item1}");
            }
        }

        [Given("I have assigned the following operators to the estates")]
        public async Task GivenIHaveAssignedTheFollowingOperatorsToTheEstates(DataTable dataTable)
        {
            List<(EstateDetails estate, AssignOperatorRequest request)> requests = dataTable.Rows.ToAssignOperatorToEstateRequests(this.TestingContext.Estates.Select(s => s.EstateDetails).ToList());

            await this.TransactionProcessorSteps.GivenIHaveAssignedTheFollowingOperatorsToTheEstates(this.TestingContext.AccessToken, requests);

            // TODO Verify
        }

        [Given("I create the following merchants")]
        [When(@"I create the following merchants")]
        public async Task WhenICreateTheFollowingMerchants(DataTable table)
        {
            var estates = this.TestingContext.Estates.Select(e => e.EstateDetails).ToList();
            List<(EstateDetails estate, CreateMerchantRequest)> requests = table.Rows.ToCreateMerchantRequests(estates);

            List<TransactionProcessor.DataTransferObjects.Responses.Merchant.MerchantResponse> verifiedMerchants = await this.TransactionProcessorSteps.WhenICreateTheFollowingMerchants(this.TestingContext.AccessToken, requests);

            foreach (TransactionProcessor.DataTransferObjects.Responses.Merchant.MerchantResponse verifiedMerchant in verifiedMerchants)
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
            List<ContractResponse> responses = await this.TransactionProcessorSteps.GivenICreateAContractWithTheFollowingValues(this.TestingContext.AccessToken, requests);
            foreach (ContractResponse contractResponse in responses)
            {
                EstateDetails1 estate = this.TestingContext.Estates.Single(e => e.EstateDetails.EstateId == contractResponse.EstateId);
                estate.EstateDetails.AddContract(contractResponse.ContractId, contractResponse.Description, contractResponse.OperatorId);
            }
        }

        [When(@"I create the following Products")]
        public async Task WhenICreateTheFollowingProducts(DataTable table)
        {
            var estates = this.TestingContext.Estates.Select(e => e.EstateDetails).ToList();

            List<(EstateDetails, TransactionProcessor.IntegrationTesting.Helpers.Contract, AddProductToContractRequest)> requests = table.Rows.ToAddProductToContractRequest(estates);
            await this.TransactionProcessorSteps.WhenICreateTheFollowingProducts(this.TestingContext.AccessToken, requests);
        }

        [When(@"I add the following Transaction Fees")]
        public async Task WhenIAddTheFollowingTransactionFees(DataTable table)
        {
            var estates = this.TestingContext.Estates.Select(e => e.EstateDetails).ToList();
            List<(EstateDetails, TransactionProcessor.IntegrationTesting.Helpers.Contract, TransactionProcessor.IntegrationTesting.Helpers.Product, AddTransactionFeeForProductToContractRequest)> requests = table.Rows.ToAddTransactionFeeForProductToContractRequests(estates);
            await this.TransactionProcessorSteps.WhenIAddTheFollowingTransactionFees(this.TestingContext.AccessToken, requests);
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
            List<EstateDetails> estates = this.TestingContext.Estates.Select(e => e.EstateDetails).ToList();
            List<(EstateDetails, Guid, AddMerchantDeviceRequest)> requests = table.Rows.ToAddMerchantDeviceRequests(estates);

            List<(EstateDetails, TransactionProcessor.DataTransferObjects.Responses.Merchant.MerchantResponse, String)> results = await this.TransactionProcessorSteps.GivenIHaveAssignedTheFollowingDevicesToTheMerchants(this.TestingContext.AccessToken, requests);
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
            await this.TransactionProcessorSteps.WhenIAddTheFollowingContractsToTheFollowingMerchants(this.TestingContext.AccessToken, requests);
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
            List<EstateDetails> estates = this.TestingContext.Estates.Select(e => e.EstateDetails).ToList();
            List<(EstateDetails, Guid, MakeMerchantDepositRequest)> requests = table.Rows.ToMakeMerchantDepositRequest(estates);

            foreach ((EstateDetails, Guid, MakeMerchantDepositRequest) request in requests){
                Decimal previousMerchantBalance = await this.GetMerchantBalance(request.Item2);

                await this.TransactionProcessorSteps.GivenIMakeTheFollowingManualMerchantDeposits(this.TestingContext.AccessToken, request);

                await Retry.For(async () => {
                                    Decimal currentMerchantBalance = await this.GetMerchantBalance(request.Item2);

                                    currentMerchantBalance.ShouldBe(previousMerchantBalance + request.Item3.Amount);

                                    this.TestingContext.Logger.LogInformation($"Deposit Reference {request.Item3.Reference} made for Merchant Id {request.Item2}");
                                });
            }
        }
    }
}
