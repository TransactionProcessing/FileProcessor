using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileProcessor.IntegrationTests.Common
{
    using System.Threading;
    using EstateManagement.DataTransferObjects;
    using EstateManagement.DataTransferObjects.Requests;
    using EstateManagement.DataTransferObjects.Responses;
    using Newtonsoft.Json;
    using SecurityService.DataTransferObjects;
    using SecurityService.DataTransferObjects.Requests;
    using SecurityService.DataTransferObjects.Responses;
    using Shared.IntegrationTesting;
    using Shouldly;
    using TechTalk.SpecFlow;

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

        [Given(@"I have created the following estates")]
        [When(@"I create the following estates")]
        public async Task WhenICreateTheFollowingEstates(Table table)
        {
            foreach (TableRow tableRow in table.Rows)
            {
                String estateName = SpecflowTableHelper.GetStringRowValue(tableRow, "EstateName");
                // Setup the subscriptions for the estate
                await Retry.For(async () => {
                                    await this.TestingContext.DockerHelper
                                              .PopulateSubscriptionServiceConfiguration(estateName, this.TestingContext.DockerHelper.IsSecureEventStore)
                                              .ConfigureAwait(false);},
                                retryFor:TimeSpan.FromMinutes(2),
                                retryInterval:TimeSpan.FromSeconds(30));
            }

            foreach (TableRow tableRow in table.Rows)
            {
                String estateName = SpecflowTableHelper.GetStringRowValue(tableRow, "EstateName");

                CreateEstateRequest createEstateRequest = new CreateEstateRequest
                                                          {
                                                              EstateId = Guid.NewGuid(),
                                                              EstateName = estateName
                                                          };
                CreateEstateResponse response = null;
                await Retry.For(async () =>
                                {
                                    response = await this.TestingContext.DockerHelper.EstateClient
                                                         .CreateEstate(this.TestingContext.AccessToken, createEstateRequest, CancellationToken.None)
                                                         .ConfigureAwait(false);

                                    response.ShouldNotBeNull();
                                    response.EstateId.ShouldNotBe(Guid.Empty);

                                }, retryFor: TimeSpan.FromMinutes(1),
                                retryInterval: TimeSpan.FromSeconds(30));

                
                this.TestingContext.Logger.LogInformation($"Estate {estateName} created with Id {response.EstateId}");
            
            EstateResponse estate = null;
                await Retry.For(async () =>
                                {
                                    estate = await this.TestingContext.DockerHelper.EstateClient
                                                       .GetEstate(this.TestingContext.AccessToken, response.EstateId, CancellationToken.None).ConfigureAwait(false);
                                    estate.ShouldNotBeNull();

                                    // Cache the estate id
                                    this.TestingContext.AddEstateDetails(estate.EstateId, estate.EstateName);
                                },
                                TimeSpan.FromMinutes(3),
                                TimeSpan.FromSeconds(30)).ConfigureAwait(false);

                estate.EstateName.ShouldBe(estateName);
            }
        }

        [Given(@"I create the following api scopes")]
        public async Task GivenICreateTheFollowingApiScopes(Table table)
        {
            foreach (TableRow tableRow in table.Rows)
            {
                CreateApiScopeRequest createApiScopeRequest = new CreateApiScopeRequest
                {
                    Name = SpecflowTableHelper.GetStringRowValue(tableRow, "Name"),
                    Description = SpecflowTableHelper.GetStringRowValue(tableRow, "Description"),
                    DisplayName = SpecflowTableHelper.GetStringRowValue(tableRow, "DisplayName")
                };
                var createApiScopeResponse =
                    await this.CreateApiScope(createApiScopeRequest, CancellationToken.None).ConfigureAwait(false);

                createApiScopeResponse.ShouldNotBeNull();
                createApiScopeResponse.ApiScopeName.ShouldNotBeNullOrEmpty();
            }
        }

        private async Task<CreateApiScopeResponse> CreateApiScope(CreateApiScopeRequest createApiScopeRequest,
                                                                  CancellationToken cancellationToken)
        {
            CreateApiScopeResponse createApiScopeResponse = await this.TestingContext.DockerHelper.SecurityServiceClient.CreateApiScope(createApiScopeRequest, cancellationToken).ConfigureAwait(false);
            return createApiScopeResponse;
        }

        [Given(@"I have assigned the following  operator to the merchants")]
        [When(@"I assign the following  operator to the merchants")]
        public async Task WhenIAssignTheFollowingOperatorToTheMerchants(Table table)
        {
            foreach (TableRow tableRow in table.Rows)
            {
                EstateDetails estateDetails = this.TestingContext.GetEstateDetails(tableRow);

                String token = this.TestingContext.AccessToken;
                if (String.IsNullOrEmpty(estateDetails.AccessToken) == false)
                {
                    token = estateDetails.AccessToken;
                }

                // Lookup the merchant id
                String merchantName = SpecflowTableHelper.GetStringRowValue(tableRow, "MerchantName");
                Guid merchantId = estateDetails.GetMerchantId(merchantName);

                // Lookup the operator id
                String operatorName = SpecflowTableHelper.GetStringRowValue(tableRow, "OperatorName");
                Guid operatorId = estateDetails.GetOperatorId(operatorName);

                AssignOperatorRequest assignOperatorRequest = new AssignOperatorRequest
                {
                    OperatorId = operatorId,
                    MerchantNumber = SpecflowTableHelper.GetStringRowValue(tableRow, "MerchantNumber"),
                    TerminalNumber = SpecflowTableHelper.GetStringRowValue(tableRow, "TerminalNumber"),
                };

                AssignOperatorResponse assignOperatorResponse = await this.TestingContext.DockerHelper.EstateClient.AssignOperatorToMerchant(token, estateDetails.EstateId, merchantId, assignOperatorRequest, CancellationToken.None).ConfigureAwait(false);

                assignOperatorResponse.EstateId.ShouldBe(estateDetails.EstateId);
                assignOperatorResponse.MerchantId.ShouldBe(merchantId);
                assignOperatorResponse.OperatorId.ShouldBe(operatorId);

                this.TestingContext.Logger.LogInformation($"Operator {operatorName} assigned to Estate {estateDetails.EstateName}");
            }
        }


        [Given(@"I have created the following operators")]
        [When(@"I create the following operators")]
        public async Task WhenICreateTheFollowingOperators(Table table)
        {
            foreach (TableRow tableRow in table.Rows)
            {
                String operatorName = SpecflowTableHelper.GetStringRowValue(tableRow, "OperatorName");
                Boolean requireCustomMerchantNumber = SpecflowTableHelper.GetBooleanValue(tableRow, "RequireCustomMerchantNumber");
                Boolean requireCustomTerminalNumber = SpecflowTableHelper.GetBooleanValue(tableRow, "RequireCustomTerminalNumber");

                CreateOperatorRequest createOperatorRequest = new CreateOperatorRequest
                {
                    Name = operatorName,
                    RequireCustomMerchantNumber = requireCustomMerchantNumber,
                    RequireCustomTerminalNumber = requireCustomTerminalNumber
                };

                // lookup the estate id based on the name in the table
                EstateDetails estateDetails = this.TestingContext.GetEstateDetails(tableRow);

                CreateOperatorResponse response = await this.TestingContext.DockerHelper.EstateClient.CreateOperator(this.TestingContext.AccessToken, estateDetails.EstateId, createOperatorRequest, CancellationToken.None).ConfigureAwait(false);

                response.ShouldNotBeNull();
                response.EstateId.ShouldNotBe(Guid.Empty);
                response.OperatorId.ShouldNotBe(Guid.Empty);

                // Cache the estate id
                estateDetails.AddOperator(response.OperatorId, operatorName);

                this.TestingContext.Logger.LogInformation($"Operator {operatorName} created with Id {response.OperatorId} for Estate {estateDetails.EstateName}");
            }
        }

        [Given("I create the following merchants")]
        [When(@"I create the following merchants")]
        public async Task WhenICreateTheFollowingMerchants(Table table)
        {
            foreach (TableRow tableRow in table.Rows)
            {
                // lookup the estate id based on the name in the table
                EstateDetails estateDetails = this.TestingContext.GetEstateDetails(tableRow);
                String token = this.TestingContext.AccessToken;
                if (String.IsNullOrEmpty(estateDetails.AccessToken) == false)
                {
                    token = estateDetails.AccessToken;
                }

                String merchantName = SpecflowTableHelper.GetStringRowValue(tableRow, "MerchantName");
                CreateMerchantRequest createMerchantRequest = new CreateMerchantRequest
                {
                    Name = merchantName,
                    Contact = new Contact
                    {
                        ContactName = SpecflowTableHelper.GetStringRowValue(tableRow, "ContactName"),
                        EmailAddress = SpecflowTableHelper.GetStringRowValue(tableRow, "EmailAddress")
                    },
                    Address = new Address
                    {
                        AddressLine1 = SpecflowTableHelper.GetStringRowValue(tableRow, "AddressLine1"),
                        Town = SpecflowTableHelper.GetStringRowValue(tableRow, "Town"),
                        Region = SpecflowTableHelper.GetStringRowValue(tableRow, "Region"),
                        Country = SpecflowTableHelper.GetStringRowValue(tableRow, "Country")
                    }
                };

                CreateMerchantResponse response = await this.TestingContext.DockerHelper.EstateClient
                                                            .CreateMerchant(token, estateDetails.EstateId, createMerchantRequest, CancellationToken.None).ConfigureAwait(false);

                response.ShouldNotBeNull();
                response.EstateId.ShouldBe(estateDetails.EstateId);
                response.MerchantId.ShouldNotBe(Guid.Empty);

                // Cache the merchant id
                estateDetails.AddMerchant(response.MerchantId, merchantName);

                this.TestingContext.Logger.LogInformation($"Merchant {merchantName} created with Id {response.MerchantId} for Estate {estateDetails.EstateName}");
            }

            foreach (TableRow tableRow in table.Rows)
            {
                EstateDetails estateDetails = this.TestingContext.GetEstateDetails(tableRow);

                String merchantName = SpecflowTableHelper.GetStringRowValue(tableRow, "MerchantName");

                Guid merchantId = estateDetails.GetMerchantId(merchantName);

                String token = this.TestingContext.AccessToken;
                if (String.IsNullOrEmpty(estateDetails.AccessToken) == false)
                {
                    token = estateDetails.AccessToken;
                }

                await Retry.For(async () =>
                {
                    MerchantResponse merchant = await this.TestingContext.DockerHelper.EstateClient
                                                          .GetMerchant(token, estateDetails.EstateId, merchantId, CancellationToken.None)
                                                          .ConfigureAwait(false);

                    merchant.MerchantName.ShouldBe(merchantName);
                });
            }
        }

        [Given(@"I create a contract with the following values")]
        public async Task GivenICreateAContractWithTheFollowingValues(Table table)
        {
            foreach (TableRow tableRow in table.Rows)
            {
                EstateDetails estateDetails = this.TestingContext.GetEstateDetails(tableRow);

                String token = this.TestingContext.AccessToken;
                if (String.IsNullOrEmpty(estateDetails.AccessToken) == false)
                {
                    token = estateDetails.AccessToken;
                }

                String operatorName = SpecflowTableHelper.GetStringRowValue(tableRow, "OperatorName");
                Guid operatorId = estateDetails.GetOperatorId(operatorName);

                CreateContractRequest createContractRequest = new CreateContractRequest
                {
                    OperatorId = operatorId,
                    Description = SpecflowTableHelper.GetStringRowValue(tableRow, "ContractDescription")
                };

                CreateContractResponse contractResponse =
                    await this.TestingContext.DockerHelper.EstateClient.CreateContract(token, estateDetails.EstateId, createContractRequest, CancellationToken.None);

                estateDetails.AddContract(contractResponse.ContractId, createContractRequest.Description, operatorId);
            }
        }

        [When(@"I create the following Products")]
        public async Task WhenICreateTheFollowingProducts(Table table)
        {
            foreach (TableRow tableRow in table.Rows)
            {
                EstateDetails estateDetails = this.TestingContext.GetEstateDetails(tableRow);

                String token = this.TestingContext.AccessToken;
                if (String.IsNullOrEmpty(estateDetails.AccessToken) == false)
                {
                    token = estateDetails.AccessToken;
                }

                String contractName = SpecflowTableHelper.GetStringRowValue(tableRow, "ContractDescription");
                Contract contract = estateDetails.GetContract(contractName);
                String productValue = SpecflowTableHelper.GetStringRowValue(tableRow, "Value");

                AddProductToContractRequest addProductToContractRequest = new AddProductToContractRequest
                {
                    ProductName = SpecflowTableHelper.GetStringRowValue(tableRow, "ProductName"),
                    DisplayText = SpecflowTableHelper.GetStringRowValue(tableRow, "DisplayText"),
                    Value = null
                };
                if (String.IsNullOrEmpty(productValue) == false)
                {
                    addProductToContractRequest.Value = Decimal.Parse(productValue);
                }

                AddProductToContractResponse addProductToContractResponse = await this.TestingContext.DockerHelper.EstateClient.AddProductToContract(token,
                                                                                                                                                     estateDetails.EstateId,
                                                                                                                                                     contract.ContractId,
                                                                                                                                                     addProductToContractRequest,
                                                                                                                                                     CancellationToken.None);

                contract.AddProduct(addProductToContractResponse.ProductId, addProductToContractRequest.ProductName, addProductToContractRequest.DisplayText,
                                    addProductToContractRequest.Value);
            }
        }

        [When(@"I add the following Transaction Fees")]
        public async Task WhenIAddTheFollowingTransactionFees(Table table)
        {
            foreach (TableRow tableRow in table.Rows)
            {
                EstateDetails estateDetails = this.TestingContext.GetEstateDetails(tableRow);

                String token = this.TestingContext.AccessToken;
                if (String.IsNullOrEmpty(estateDetails.AccessToken) == false)
                {
                    token = estateDetails.AccessToken;
                }

                String contractName = SpecflowTableHelper.GetStringRowValue(tableRow, "ContractDescription");
                String productName = SpecflowTableHelper.GetStringRowValue(tableRow, "ProductName");
                Contract contract = estateDetails.GetContract(contractName);

                Product product = contract.GetProduct(productName);

                AddTransactionFeeForProductToContractRequest addTransactionFeeForProductToContractRequest = new AddTransactionFeeForProductToContractRequest
                {
                    Value =
                                                                                                                    SpecflowTableHelper
                                                                                                                        .GetDecimalValue(tableRow, "Value"),
                    Description =
                                                                                                                    SpecflowTableHelper.GetStringRowValue(tableRow,
                                                                                                                                                          "FeeDescription"),
                    CalculationType =
                                                                                                                    SpecflowTableHelper
                                                                                                                        .GetEnumValue<CalculationType>(tableRow,
                                                                                                                                                       "CalculationType")
                };

                AddTransactionFeeForProductToContractResponse addTransactionFeeForProductToContractResponse =
                    await this.TestingContext.DockerHelper.EstateClient.AddTransactionFeeForProductToContract(token,
                                                                                                              estateDetails.EstateId,
                                                                                                              contract.ContractId,
                                                                                                              product.ProductId,
                                                                                                              addTransactionFeeForProductToContractRequest,
                                                                                                              CancellationToken.None);

                product.AddTransactionFee(addTransactionFeeForProductToContractResponse.TransactionFeeId,
                                          addTransactionFeeForProductToContractRequest.CalculationType,
                                          addTransactionFeeForProductToContractRequest.Description,
                                          addTransactionFeeForProductToContractRequest.Value);
            }
        }
        
        [Given(@"the following api resources exist")]
        public async Task GivenTheFollowingApiResourcesExist(Table table)
        {
            foreach (TableRow tableRow in table.Rows)
            {
                String resourceName = SpecflowTableHelper.GetStringRowValue(tableRow, "ResourceName");
                String displayName = SpecflowTableHelper.GetStringRowValue(tableRow, "DisplayName");
                String secret = SpecflowTableHelper.GetStringRowValue(tableRow, "Secret");
                String scopes = SpecflowTableHelper.GetStringRowValue(tableRow, "Scopes");
                String userClaims = SpecflowTableHelper.GetStringRowValue(tableRow, "UserClaims");

                List<String> splitScopes = scopes.Split(",").ToList();
                List<String> splitUserClaims = userClaims.Split(",").ToList();

                CreateApiResourceRequest createApiResourceRequest = new CreateApiResourceRequest
                {
                    Description = String.Empty,
                    DisplayName = displayName,
                    Name = resourceName,
                    Scopes = new List<String>(),
                    Secret = secret,
                    UserClaims = new List<String>()
                };
                splitScopes.ForEach(a =>
                {
                    createApiResourceRequest.Scopes.Add(a.Trim());
                });
                splitUserClaims.ForEach(a =>
                {
                    createApiResourceRequest.UserClaims.Add(a.Trim());
                });

                CreateApiResourceResponse createApiResourceResponse = await this.TestingContext.DockerHelper.SecurityServiceClient.CreateApiResource(createApiResourceRequest, CancellationToken.None).ConfigureAwait(false);

                createApiResourceResponse.ApiResourceName.ShouldBe(resourceName);
            }
        }

        [Given(@"the following clients exist")]
        public async Task GivenTheFollowingClientsExist(Table table)
        {
            foreach (TableRow tableRow in table.Rows)
            {
                String clientId = SpecflowTableHelper.GetStringRowValue(tableRow, "ClientId");
                String clientName = SpecflowTableHelper.GetStringRowValue(tableRow, "ClientName");
                String secret = SpecflowTableHelper.GetStringRowValue(tableRow, "Secret");
                String allowedScopes = SpecflowTableHelper.GetStringRowValue(tableRow, "AllowedScopes");
                String allowedGrantTypes = SpecflowTableHelper.GetStringRowValue(tableRow, "AllowedGrantTypes");

                List<String> splitAllowedScopes = allowedScopes.Split(",").ToList();
                List<String> splitAllowedGrantTypes = allowedGrantTypes.Split(",").ToList();

                CreateClientRequest createClientRequest = new CreateClientRequest
                {
                    Secret = secret,
                    AllowedGrantTypes = new List<String>(),
                    AllowedScopes = new List<String>(),
                    ClientDescription = String.Empty,
                    ClientId = clientId,
                    ClientName = clientName
                };

                splitAllowedScopes.ForEach(a =>
                {
                    createClientRequest.AllowedScopes.Add(a.Trim());
                });
                splitAllowedGrantTypes.ForEach(a =>
                {
                    createClientRequest.AllowedGrantTypes.Add(a.Trim());
                });

                CreateClientResponse createClientResponse = await this.TestingContext.DockerHelper.SecurityServiceClient.CreateClient(createClientRequest, CancellationToken.None).ConfigureAwait(false);

                createClientResponse.ClientId.ShouldBe(clientId);

                this.TestingContext.AddClientDetails(clientId, secret, allowedGrantTypes);
            }
        }

        [Given(@"I have a token to access the estate management and transaction processor resources")]
        public async Task GivenIHaveATokenToAccessTheEstateManagementAndTransactionProcessorResources(Table table)
        {
            foreach (TableRow tableRow in table.Rows)
            {
                String clientId = SpecflowTableHelper.GetStringRowValue(tableRow, "ClientId");

                ClientDetails clientDetails = this.TestingContext.GetClientDetails(clientId);

                if (clientDetails.GrantType == "client_credentials")
                {
                    TokenResponse tokenResponse = await this.TestingContext.DockerHelper.SecurityServiceClient.GetToken(clientId, clientDetails.ClientSecret, CancellationToken.None).ConfigureAwait(false);

                    this.TestingContext.AccessToken = tokenResponse.AccessToken;
                }
            }
        }

        [Given(@"I have assigned the following devices to the merchants")]
        public async Task GivenIHaveAssignedTheFollowingDevicesToTheMerchants(Table table)
        {
            foreach (TableRow tableRow in table.Rows)
            {
                EstateDetails estateDetails = this.TestingContext.GetEstateDetails(tableRow);

                String token = this.TestingContext.AccessToken;
                if (String.IsNullOrEmpty(estateDetails.AccessToken) == false)
                {
                    token = estateDetails.AccessToken;
                }

                // Lookup the merchant id
                String merchantName = SpecflowTableHelper.GetStringRowValue(tableRow, "MerchantName");
                Guid merchantId = estateDetails.GetMerchantId(merchantName);

                // Lookup the operator id
                String deviceIdentifier = SpecflowTableHelper.GetStringRowValue(tableRow, "DeviceIdentifier");

                AddMerchantDeviceRequest addMerchantDeviceRequest = new AddMerchantDeviceRequest
                {
                    DeviceIdentifier = deviceIdentifier
                };

                AddMerchantDeviceResponse addMerchantDeviceResponse = await this.TestingContext.DockerHelper.EstateClient.AddDeviceToMerchant(token, estateDetails.EstateId, merchantId, addMerchantDeviceRequest, CancellationToken.None).ConfigureAwait(false);

                addMerchantDeviceResponse.EstateId.ShouldBe(estateDetails.EstateId);
                addMerchantDeviceResponse.MerchantId.ShouldBe(merchantId);
                addMerchantDeviceResponse.DeviceId.ShouldNotBe(Guid.Empty);

                this.TestingContext.Logger.LogInformation($"Device {deviceIdentifier} assigned to Merchant {merchantName} Estate {estateDetails.EstateName}");
            }
        }

        [Given(@"I make the following manual merchant deposits")]
        public async Task GivenIMakeTheFollowingManualMerchantDeposits(Table table)
        {
            foreach (TableRow tableRow in table.Rows)
            {
                EstateDetails estateDetails = this.TestingContext.GetEstateDetails(tableRow);

                String token = this.TestingContext.AccessToken;
                if (String.IsNullOrEmpty(estateDetails.AccessToken) == false)
                {
                    token = estateDetails.AccessToken;
                }

                // Lookup the merchant id
                String merchantName = SpecflowTableHelper.GetStringRowValue(tableRow, "MerchantName");
                Guid merchantId = estateDetails.GetMerchantId(merchantName);

                // Get current balance
                MerchantBalanceResponse previousMerchantBalance = await this.TestingContext.DockerHelper.EstateClient.GetMerchantBalance(token, estateDetails.EstateId, merchantId, CancellationToken.None);

                MakeMerchantDepositRequest makeMerchantDepositRequest = new MakeMerchantDepositRequest
                {
                    DepositDateTime = SpecflowTableHelper.GetDateForDateString(SpecflowTableHelper.GetStringRowValue(tableRow, "DateTime"), DateTime.Now),
                    Reference = SpecflowTableHelper.GetStringRowValue(tableRow, "Reference"),
                    Amount = SpecflowTableHelper.GetDecimalValue(tableRow, "Amount")
                };

                MakeMerchantDepositResponse makeMerchantDepositResponse = await this.TestingContext.DockerHelper.EstateClient.MakeMerchantDeposit(token, estateDetails.EstateId, merchantId, makeMerchantDepositRequest, CancellationToken.None).ConfigureAwait(false);

                makeMerchantDepositResponse.EstateId.ShouldBe(estateDetails.EstateId);
                makeMerchantDepositResponse.MerchantId.ShouldBe(merchantId);
                makeMerchantDepositResponse.DepositId.ShouldNotBe(Guid.Empty);

                this.TestingContext.Logger.LogInformation($"Deposit Reference {makeMerchantDepositRequest.Reference} made for Merchant {merchantName}");

                // Check the merchant balance
                await Retry.For(async () =>
                          {
                              MerchantBalanceResponse currentMerchantBalance = await this.TestingContext.DockerHelper.EstateClient.GetMerchantBalance(token, estateDetails.EstateId, merchantId, CancellationToken.None);

                              currentMerchantBalance.AvailableBalance.ShouldBe(previousMerchantBalance.AvailableBalance + makeMerchantDepositRequest.Amount);
                          }, TimeSpan.FromMinutes(2));
                

            }
        }
    }
}
