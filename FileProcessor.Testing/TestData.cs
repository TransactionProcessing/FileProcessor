using System;

namespace FileProcessor.Testing
{
    using System.Collections.Generic;
    using BusinessLogic.Managers;
    using EstateManagement.DataTransferObjects.Responses;
    using File.DomainEvents;
    using FileImportLog.DomainEvents;
    using FileAggregate;
    using FileImportLogAggregate;
    using FIleProcessor.Models;
    using Newtonsoft.Json;
    using SecurityService.DataTransferObjects.Responses;
    using TransactionProcessor.DataTransferObjects;

    public class TestData
    {
        public static Guid EstateId = Guid.Parse("56B7693B-3BB3-45A8-B382-E6F289551267");

        public static Guid MerchantId = Guid.Parse("20E13552-7AE6-4BB4-856A-C466E82B6B54");

        public static Guid FileId = Guid.Parse("5F7F45D6-0604-46C7-AA88-EAA885A6B208");

        public static Guid UserId = Guid.Parse("BE52C5AC-72E5-4976-BAA0-98699E36C1EB");

        public static String FilePath = "home/txnproc/bulkfiles";
        public static String FilePathWithName = "home/txnproc/bulkfiles/Example.csv";
        public static String InProgressSafaricomFilePathWithName = "home/txnproc/bulkfiles/safaricom/inprogress/Example.csv";

        public static Guid FileProfileId = Guid.Parse("D0D3A4E5-870E-42F6-AD0E-5E24252BC95E");

        public static String FileName = "testfile1.csv";

        public static Int32 LineNumber = 1;

        public static Int32 NotFoundLineNumber = 100;
        
        public static String FileLine = "D,124567,100";

        public static String FileLocation = "home/txnproc/bulkfiles/safaricom/ExampleFile.csv";

        public static String OriginalFileName = "ExampleFile.csv";

        public static Guid TransactionId = Guid.Parse("EFAA401D-9ED7-4588-9451-0E3372CBC4CB");

        public static String ResponseMessage = "SUCCESS";

        public static String ResponseCode = "0000";

        public static String ResponseMessageFailed = "SUCCESS";

        public static String ResponseCodeFailed = "1001";

        public static String SafaricomDetailLineAmount = "100";

        public static String VoucherDetailLineAmount = "10";

        public static String SafaricomDetailLineCustomerAccountNumber = "07777771234";

        public static FileAddedToImportLogEvent FileAddedToImportLogEvent =>
            new FileAddedToImportLogEvent(TestData.FileImportLogId,
                                          TestData.FileId,
                                          TestData.EstateId,
                                          TestData.MerchantId,
                                          TestData.UserId,
                                          TestData.FileProfileId,
                                          TestData.OriginalFileName,
                                          TestData.FilePath,
                                          TestData.FileUploadedDateTime);

        public static FileLineAddedEvent FileLineAddedEvent => new FileLineAddedEvent(TestData.FileId, TestData.EstateId, TestData.LineNumber, TestData.FileLine);

        public static String SafaricomDetailLine => $"D,{TestData.SafaricomDetailLineCustomerAccountNumber},{TestData.SafaricomDetailLineAmount}";

        public static String VoucherDetailLineWithEmailAddress => $"D,{TestData.VoucherOperatorIdentifier}, {TestData.VoucherRecipientEmail},{TestData.VoucherDetailLineAmount}";

        public static String VoucherDetailLineWithMobileNumber => $"D,{TestData.VoucherOperatorIdentifier},{TestData.VoucherRecipientMobile},{TestData.VoucherDetailLineAmount}";

        public static FileProfile FileProfileNull => null;

        public static FileProfile FileProfileSafaricom =>
            new FileProfile(TestData.SafaricomFileProfileId,
                            TestData.SafaricomProfileName,
                            TestData.SafaricomListeningDirectory,
                            TestData.SafaricomRequestType,
                            TestData.OperatorIdentifier,
                            TestData.SafaricomLineTerminator);

        public static List<FileProfile> FileProfiles => new List<FileProfile>
                                                       {
                                                           FileProfileSafaricom
                                                       };
        public static Guid SafaricomFileProfileId = Guid.Parse("079F1FF5-F51E-4BE0-AF4F-2D4862E6D34F");
        public static String SafaricomProfileName = "Safaricom Profile";

        public static String SafaricomListeningDirectory = "/home/txnproc/bulkfiles/safaricom";

        public static String SafaricomRequestType = "SafaricomTopupRequest";

        public static String SafaricomLineTerminator = "\n";
        
        public static Decimal AvailableBalance = 1000.00m;

        public static String MerchantName = "Test Merchant 1";

        public static Guid DeviceId = Guid.Parse("D0FA3C20-055A-4EB2-B049-978559155588");

        public static String DeviceIdentifier = "testdevice1";

        public static String OperatorIdentifier = "Safaricom";

        public static String VoucherOperatorIdentifier = "Healthcare Centre 1";

        public static String OperatorIdentifierNotFile = "Other Operator";

        public static Guid OperatorId = Guid.Parse("68B5745B-2D77-41F1-8310-FDAB060C0001");

        public static String MerchantNumber = "12345678";

        public static String TerminalNumber = "00000001";

        public static Guid ContractId = Guid.Parse("835D421B-6F34-4369-AC27-6E2365B11D29");

        public static String ContractDescription = "Safaricom Contract";

        public static String ContractProductWithValueName = "100 KES Topup";

        public static Guid ContractWithValueProductId = Guid.Parse("7F8FF091-E127-429D-8D92-5B8597320AE1");

        public static Decimal? ContractProductWithValueValue = 100.00m;

        public static String ContractProductWithValueDisplayText = "100 KES";

        public static String ContractProductWithNullValueName = "Custom Topup";

        public static Guid ContractWithNullValueProductId = Guid.Parse("435CBB20-1ADC-4646-8DEC-5341E877220D");

        public static Decimal? ContractProductWithNullValueValue = null;

        public static String ContractProductWithNullValueDisplayText = "Custom";

        public static Guid FileImportLogId = Guid.Parse("5F1149F8-0313-45E4-BE3A-3D7B07EEB414");

        public static DateTime ImportLogDateTime = new DateTime(2021,5,7);

        public static DateTime FileUploadedDateTime = new DateTime(2021, 5, 7);

        public static DateTime ProcessingCompletedDateTime = new DateTime(2021, 5, 7);

        public static String VoucherRecipientEmail = "voucherrecipient1@myemail.com";

        public static String VoucherRecipientMobile = "07777777705";

        public static List<ContractResponse> GetEmptyMerchantContractsResponse => new List<ContractResponse>();

        public static Dictionary<String, String> TransactionMetadata =>
            new Dictionary<String, String>
            {
                {"Amount", "100"},
                {"CustomerAccountNumber", "123456789"}
            };

        public static Dictionary<String, String> TransactionMetadataWithOperatorName =>
            new Dictionary<String, String>
            {
                {"Amount", "100"},
                {"CustomerAccountNumber", "123456789"},
                {"OperatorName", TestData.OperatorIdentifier}
            };

        public static FileImportLogAggregate GetEmptyFileImportLogAggregate()
        {
            return new FileImportLogAggregate();
        }

        public static FileImportLogAggregate GetCreatedFileImportLogAggregate()
        {
            FileImportLogAggregate fileImportLogAggregate = new FileImportLogAggregate();
            fileImportLogAggregate.CreateImportLog(TestData.EstateId, TestData.ImportLogDateTime);
            return fileImportLogAggregate;
        }

        public static FileAggregate GetEmptyFileAggregate()
        {
            return new FileAggregate();
        }

        public static FileAggregate GetCreatedFileAggregate()
        {
            FileAggregate fileAggregate = new FileAggregate();

            fileAggregate.CreateFile( TestData.FileImportLogId,TestData.EstateId, TestData.MerchantId, TestData.UserId, TestData.FileProfileId, TestData.OriginalFileName, TestData.FileUploadedDateTime);

            return fileAggregate;
        }

        public static FileAggregate GetFileAggregateWithLines()
        {
            FileAggregate fileAggregate = new FileAggregate();

            fileAggregate.CreateFile(TestData.FileImportLogId,TestData.EstateId, TestData.MerchantId, TestData.UserId, TestData.FileProfileId, TestData.OriginalFileName, TestData.FileUploadedDateTime);
            fileAggregate.AddFileLine("D,1,2");

            return fileAggregate;
        }

        public static FileAggregate GetFileAggregateWithLinesAlreadyProcessed()
        {
            FileAggregate fileAggregate = new FileAggregate();

            fileAggregate.CreateFile(TestData.FileImportLogId,TestData.EstateId, TestData.MerchantId, TestData.UserId, TestData.FileProfileId, TestData.OriginalFileName, TestData.FileUploadedDateTime);
            fileAggregate.AddFileLine("D,1,2");
            fileAggregate.RecordFileLineAsSuccessful(TestData.LineNumber, TestData.TransactionId);
            return fileAggregate;
        }

        public static IReadOnlyDictionary<String, String> DefaultAppSettings =>
            new Dictionary<String, String>
            {
                ["AppSettings:ClientId"] = "clientId",
                ["AppSettings:ClientSecret"] = "clientSecret"
            };

        public static TokenResponse TokenResponse()
        {
            return SecurityService.DataTransferObjects.Responses.TokenResponse.Create("AccessToken", string.Empty, 100);
        }

        public static MerchantResponse GetMerchantResponseWithOperator =>
            new MerchantResponse
            {
                AvailableBalance = TestData.AvailableBalance,
                EstateId = TestData.EstateId,
                MerchantId = TestData.MerchantId,
                MerchantName = TestData.MerchantName,
                Devices = new Dictionary<Guid, String>
                          {
                              {TestData.DeviceId, TestData.DeviceIdentifier}
                          },
                Operators = new List<MerchantOperatorResponse>
                            {
                                new MerchantOperatorResponse
                                {
                                    Name = TestData.OperatorIdentifier,
                                    OperatorId = TestData.OperatorId,
                                    MerchantNumber = TestData.MerchantNumber,
                                    TerminalNumber = TestData.TerminalNumber
                                }
                            }
            };

        public static List<ContractResponse> GetMerchantContractsResponse()
        {
            List<ContractResponse> contractResponses = new List<ContractResponse>();

            contractResponses.Add(new ContractResponse
                                  {
                                      EstateId = TestData.EstateId,
                                      OperatorName = TestData.OperatorIdentifier,
                                      ContractId = TestData.ContractId,
                                      Description = TestData.ContractDescription,
                                      OperatorId = TestData.OperatorId,
                                      Products = new List<ContractProduct>
                                                 {
                                                     new ContractProduct
                                                     {
                                                         Name = TestData.ContractProductWithValueName,
                                                         ProductId = TestData.ContractWithValueProductId,
                                                         Value = TestData.ContractProductWithValueValue,
                                                         DisplayText = TestData.ContractProductWithValueDisplayText,
                                                         TransactionFees = null
                                                     },
                                                     new ContractProduct
                                                     {
                                                         Name = TestData.ContractProductWithNullValueName,
                                                         ProductId = TestData.ContractWithNullValueProductId,
                                                         Value = TestData.ContractProductWithNullValueValue,
                                                         DisplayText = TestData.ContractProductWithNullValueDisplayText,
                                                         TransactionFees = null
                                                     }

                                                 }
                                  });
            
            return contractResponses;
        }

        public static List<ContractResponse> GetMerchantContractsNoFileOperatorResponse()
        {
            List<ContractResponse> contractResponses = new List<ContractResponse>();

            contractResponses.Add(new ContractResponse
            {
                EstateId = TestData.EstateId,
                OperatorName = TestData.OperatorIdentifierNotFile,
                ContractId = TestData.ContractId,
                Description = TestData.ContractDescription,
                OperatorId = TestData.OperatorId,
                Products = new List<ContractProduct>
                                                 {
                                                     new ContractProduct
                                                     {
                                                         Name = TestData.ContractProductWithValueName,
                                                         ProductId = TestData.ContractWithValueProductId,
                                                         Value = TestData.ContractProductWithValueValue,
                                                         DisplayText = TestData.ContractProductWithValueDisplayText,
                                                         TransactionFees = null
                                                     },
                                                     new ContractProduct
                                                     {
                                                         Name = TestData.ContractProductWithNullValueName,
                                                         ProductId = TestData.ContractWithNullValueProductId,
                                                         Value = TestData.ContractProductWithNullValueValue,
                                                         DisplayText = TestData.ContractProductWithNullValueDisplayText,
                                                         TransactionFees = null
                                                     }

                                                 }
            });

            return contractResponses;
        }

        public static List<ContractResponse> GetMerchantContractsResponseNoNullValueProduct()
        {
            List<ContractResponse> contractResponses = new List<ContractResponse>();

            contractResponses.Add(new ContractResponse
            {
                EstateId = TestData.EstateId,
                OperatorName = TestData.OperatorIdentifier,
                ContractId = TestData.ContractId,
                Description = TestData.ContractDescription,
                OperatorId = TestData.OperatorId,
                Products = new List<ContractProduct>
                                                 {
                                                     new ContractProduct
                                                     {
                                                         Name = TestData.ContractProductWithValueName,
                                                         ProductId = TestData.ContractWithValueProductId,
                                                         Value = TestData.ContractProductWithValueValue,
                                                         DisplayText = TestData.ContractProductWithValueDisplayText,
                                                         TransactionFees = null
                                                     }
                                                 }
            });

            return contractResponses;
        }

        public static SerialisedMessage SerialisedMessageResponseSale => new SerialisedMessage
                                                                        {
                                                                            Metadata = new Dictionary<String, String>
                                                                                       {
                                                                                           {"EstateId", TestData.EstateId.ToString()},
                                                                                           {"MerchantId", TestData.MerchantId.ToString()},
                                                                                       },
                                                                            SerialisedData = JsonConvert.SerializeObject(TestData.ClientSaleTransactionResponse, new JsonSerializerSettings
                                                                                {
                                                                                    TypeNameHandling = TypeNameHandling.All
                                                                                })
                                                                        };

        public static SaleTransactionResponse ClientSaleTransactionResponse => new SaleTransactionResponse
                                                                              {
                                                                                  TransactionId = TestData.TransactionId,
                                                                                  ResponseMessage = TestData.ResponseMessage,
                                                                                  ResponseCode = TestData.ResponseCode,
                                                                                  EstateId = TestData.EstateId,
                                                                                  MerchantId = TestData.MerchantId
                                                                              };

        public static SerialisedMessage SerialisedMessageResponseFailedSale => new SerialisedMessage
        {
            Metadata = new Dictionary<String, String>
                                                                                       {
                                                                                           {"EstateId", TestData.EstateId.ToString()},
                                                                                           {"MerchantId", TestData.MerchantId.ToString()},
                                                                                       },
            SerialisedData = JsonConvert.SerializeObject(TestData.ClientSaleTransactionFailedResponse, new JsonSerializerSettings
                                                                                                       {
                                                                                                           TypeNameHandling = TypeNameHandling.All
                                                                                                       })
        };

        public static SaleTransactionResponse ClientSaleTransactionFailedResponse => new SaleTransactionResponse
        {
            TransactionId = TestData.TransactionId,
            ResponseMessage = TestData.ResponseMessageFailed,
            ResponseCode = TestData.ResponseCodeFailed,
            EstateId = TestData.EstateId,
            MerchantId = TestData.MerchantId
        };

        public static FileProfile FileProfileVoucher =>
            new FileProfile(TestData.VoucherFileProfileId,
                            TestData.VoucherProfileName,
                            TestData.VoucherListeningDirectory,
                            TestData.VoucherRequestType,
                            TestData.VoucherOperatorIdentifier,
                            TestData.VoucherLineTerminator);

        public static Guid VoucherFileProfileId = Guid.Parse("079F1FF5-F51E-4BE0-AF4F-2D4862E6D34F");
        public static String VoucherProfileName = "Safaricom Profile";

        public static String VoucherListeningDirectory = "/home/txnproc/bulkfiles/voucher";

        public static String VoucherRequestType = "VoucherRequest";

        public static String VoucherLineTerminator = "\n";
    }
}
