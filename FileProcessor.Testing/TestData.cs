using System;

namespace FileProcessor.Testing
{
    using System.Collections.Generic;
    using BusinessLogic.Managers;
    using EstateManagement.DataTransferObjects.Responses;
    using EstateReporting.Database.Entities;
    using File.DomainEvents;
    using FileImportLog.DomainEvents;
    using FileAggregate;
    using FileImportLogAggregate;
    using FIleProcessor.Models;
    using Newtonsoft.Json;
    using SecurityService.DataTransferObjects.Responses;
    using TransactionProcessor.DataTransferObjects;
    using ContractProduct = EstateManagement.DataTransferObjects.Responses.ContractProduct;
    using FileImportLog = EstateReporting.Database.Entities.FileImportLog;
    using FileLine = EstateReporting.Database.Entities.FileLine;

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
        public static String FileLine1 = "D,124567,100";
        public static String FileLine2 = "D,124568,100";
        public static String FileLine3 = "D,124569,100";
        public static String FileLine4 = "D,124560,100";

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

        public static String GetSafaricomDetailLine(String customerAccountNumber, String amount)
        {
            return $"D,{customerAccountNumber},{amount}";
        }

        public static String GetVoucherDetailLine(String voucherOperatorIdentifier, String recipient, String amount)
        {
            return $"D,{voucherOperatorIdentifier},{recipient},{amount}";
        }

        public static String SafaricomDetailLine => $"D,{TestData.SafaricomDetailLineCustomerAccountNumber},{TestData.SafaricomDetailLineAmount}";

        public static String VoucherDetailLineWithEmailAddress => $"D,{TestData.VoucherOperatorIdentifier}, {TestData.VoucherRecipientEmail},{TestData.VoucherDetailLineAmount}";

        public static String VoucherDetailLineWithMobileNumber => $"D,{TestData.VoucherOperatorIdentifier},{TestData.VoucherRecipientMobile},{TestData.VoucherDetailLineAmount}";

        public static FileProfile FileProfileNull => null;

        public static FileProfile GetFileProfile(String operatorName)
        {
            if (operatorName == "Safaricom")
                return FileProfileSafaricom;
            if (operatorName == "Voucher")
                return FileProfileVoucher;

            return null;
        }

        public static FileProfile FileProfileSafaricom =>
            new FileProfile(TestData.SafaricomFileProfileId,
                            TestData.SafaricomProfileName,
                            TestData.SafaricomListeningDirectory,
                            TestData.SafaricomRequestType,
                            TestData.SafaricomOperatorIdentifier,
                            TestData.SafaricomLineTerminator,
                            TestData.SafaricomFileFormatHandler);

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

        public static String SafaricomOperatorIdentifier = "Safaricom";

        public static String VoucherOperatorIdentifier = "Voucher";

        public static String OperatorIdentifierNotFile = "Other Operator";

        public static Guid SafaricomOperatorId = Guid.Parse("68B5745B-2D77-41F1-8310-FDAB060C0001");

        public static Guid VoucherOperatorId = Guid.Parse("70744470-37CD-4175-8869-86A225108BED");

        public static String MerchantNumber = "12345678";

        public static String TerminalNumber = "00000001";

        public static Guid SafaricomContractId = Guid.Parse("835D421B-6F34-4369-AC27-6E2365B11D29");
        public static Guid VoucherContractId = Guid.Parse("EFD4C87B-C6F8-4DF2-BFCA-0AE39E4B9511");

        public static String SafaricomContractDescription = "Safaricom Contract";
        public static String VoucherContractDescription = "Voucher Contract";

        public static String SafaricomContractProductWithValueName = "100 KES Topup";

        public static Guid SafaricomContractWithValueProductId = Guid.Parse("7F8FF091-E127-429D-8D92-5B8597320AE1");

        public static Decimal? SafaricomContractProductWithValueValue = 100.00m;

        public static String SafaricomContractProductWithValueDisplayText = "100 KES";

        public static String SafaricomContractProductWithNullValueName = "Custom Topup";

        public static Guid SafarciomContractWithNullValueProductId = Guid.Parse("435CBB20-1ADC-4646-8DEC-5341E877220D");

        public static Decimal? SafaricomContractProductWithNullValueValue = null;

        public static String SafaricomContractProductWithNullValueDisplayText = "Custom";


        public static String VoucherContractProductWithValueName = "10 KES Voucher";

        public static Guid VoucherContractWithValueProductId = Guid.Parse("D564FFF0-04B0-41EC-AA58-2AD981D352B3");

        public static Decimal? VoucherContractProductWithValueValue = 10.00m;

        public static String VoucherContractProductWithValueDisplayText = "10 KES";

        public static String VoucherContractProductWithNullValueName = "Custom Voucher";

        public static Guid VoucherContractWithNullValueProductId = Guid.Parse("0B648D1A-F057-4055-90E8-3B106DB391E0");

        public static Decimal? VoucherContractProductWithNullValueValue = null;

        public static String VoucherContractProductWithNullValueDisplayText = "Custom";

        public static Guid FileImportLogId = Guid.Parse("5F1149F8-0313-45E4-BE3A-3D7B07EEB414");
        public static Guid FileImportLogId1 = Guid.Parse("7EF0D557-2148-4DED-83F5-2521E8422391");
        public static Guid FileImportLogId2 = Guid.Parse("13931CB7-241A-472A-A8E9-213565BF2A2A");

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
                {"OperatorName", TestData.SafaricomOperatorIdentifier}
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

        public static FileAggregate GetFileAggregateWithBlankLine()
        {
            FileAggregate fileAggregate = new FileAggregate();

            fileAggregate.CreateFile(TestData.FileImportLogId, TestData.EstateId, TestData.MerchantId, TestData.UserId, TestData.FileProfileId, TestData.OriginalFileName, TestData.FileUploadedDateTime);
            fileAggregate.AddFileLine(String.Empty);

            return fileAggregate;
        }

        public static FileAggregate GetFileAggregateWithLinesAlreadyProcessed()
        {
            FileAggregate fileAggregate = new FileAggregate();

            fileAggregate.CreateFile(TestData.FileImportLogId,TestData.EstateId, TestData.MerchantId, TestData.UserId, TestData.FileProfileId, TestData.OriginalFileName, TestData.FileUploadedDateTime);
            fileAggregate.AddFileLine("D,1,2");
            fileAggregate.AddFileLine("D,1,3");
            fileAggregate.AddFileLine("D,1,4");
            fileAggregate.AddFileLine("D,1,5");
            fileAggregate.RecordFileLineAsSuccessful(1, TestData.TransactionId);
            fileAggregate.RecordFileLineAsRejected(2, TestData.RejectionReason);
            fileAggregate.RecordFileLineAsFailed(3, TestData.TransactionId, "-1","Failed");
            fileAggregate.RecordFileLineAsIgnored(4);
            
            return fileAggregate;
        }

        public static IReadOnlyDictionary<String, String> DefaultAppSettings =>
            new Dictionary<String, String>
            {
                ["AppSettings:ClientId"] = "clientId",
                ["AppSettings:ClientSecret"] = "clientSecret",
                ["AppSettings:TemporaryFileLocation"] = "C:\\Temp",
                ["AppSettings:FileProfilePollingWindowInSeconds"] = "30",
                ["ConnectionStrings:HealthCheck"] = "HeathCheckConnString",
                ["SecurityConfiguration:Authority"] = "https://127.0.0.1",
                ["EventStoreSettings:ConnectionString"] = "https://127.0.0.1:2113",
                ["EventStoreSettings:ConnectionName"] = "UnitTestConnection",
                ["EventStoreSettings:UserName"] = "admin",
                ["EventStoreSettings:Password"] = "changeit",
                ["AppSettings:UseConnectionStringConfig"] = "false",
                ["AppSettings:SecurityService"] = "http://127.0.0.1",
                ["AppSettings:MessagingServiceApi"] = "http://127.0.0.1",
                ["AppSettings:EstateManagementApi"] = "http://127.0.0.1",
                ["AppSettings:TransactionProcessorApi"] = "http://127.0.0.1",
                ["AppSettings:DatabaseEngine"] = "SqlServer"
            };


        
        

        public static TokenResponse TokenResponse()
        {
            return SecurityService.DataTransferObjects.Responses.TokenResponse.Create("AccessToken", string.Empty, 100);
        }

        public static MerchantResponse GetMerchantResponseWithOperator =>
            new MerchantResponse
            {
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
                                    Name = TestData.SafaricomOperatorIdentifier,
                                    OperatorId = TestData.SafaricomOperatorId,
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
                                      OperatorName = TestData.SafaricomOperatorIdentifier,
                                      ContractId = TestData.SafaricomContractId,
                                      Description = TestData.SafaricomContractDescription,
                                      OperatorId = TestData.SafaricomOperatorId,
                                      Products = new List<ContractProduct>
                                                 {
                                                     new ContractProduct
                                                     {
                                                         Name = TestData.SafaricomContractProductWithValueName,
                                                         ProductId = TestData.SafaricomContractWithValueProductId,
                                                         Value = TestData.SafaricomContractProductWithValueValue,
                                                         DisplayText = TestData.SafaricomContractProductWithValueDisplayText,
                                                         TransactionFees = null
                                                     },
                                                     new ContractProduct
                                                     {
                                                         Name = TestData.SafaricomContractProductWithNullValueName,
                                                         ProductId = TestData.SafarciomContractWithNullValueProductId,
                                                         Value = TestData.SafaricomContractProductWithNullValueValue,
                                                         DisplayText = TestData.SafaricomContractProductWithNullValueDisplayText,
                                                         TransactionFees = null
                                                     }

                                                 }
                                  });

            contractResponses.Add(new ContractResponse
            {
                EstateId = TestData.EstateId,
                OperatorName = TestData.VoucherOperatorIdentifier,
                ContractId = TestData.VoucherContractId,
                Description = TestData.VoucherContractDescription,
                OperatorId = TestData.VoucherOperatorId,
                Products = new List<ContractProduct>
                                                 {
                                                     new ContractProduct
                                                     {
                                                         Name = TestData.VoucherContractProductWithValueName,
                                                         ProductId = TestData.VoucherContractWithValueProductId,
                                                         Value = TestData.VoucherContractProductWithValueValue,
                                                         DisplayText = TestData.VoucherContractProductWithValueDisplayText,
                                                         TransactionFees = null
                                                     },
                                                     new ContractProduct
                                                     {
                                                         Name = TestData.VoucherContractProductWithNullValueName,
                                                         ProductId = TestData.VoucherContractWithNullValueProductId,
                                                         Value = TestData.VoucherContractProductWithNullValueValue,
                                                         DisplayText = TestData.VoucherContractProductWithNullValueDisplayText,
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
                ContractId = TestData.SafaricomContractId,
                Description = TestData.SafaricomContractDescription,
                OperatorId = TestData.SafaricomOperatorId,
                Products = new List<ContractProduct>
                                                 {
                                                     new ContractProduct
                                                     {
                                                         Name = TestData.SafaricomContractProductWithValueName,
                                                         ProductId = TestData.SafaricomContractWithValueProductId,
                                                         Value = TestData.SafaricomContractProductWithValueValue,
                                                         DisplayText = TestData.SafaricomContractProductWithValueDisplayText,
                                                         TransactionFees = null
                                                     },
                                                     new ContractProduct
                                                     {
                                                         Name = TestData.SafaricomContractProductWithNullValueName,
                                                         ProductId = TestData.SafarciomContractWithNullValueProductId,
                                                         Value = TestData.SafaricomContractProductWithNullValueValue,
                                                         DisplayText = TestData.SafaricomContractProductWithNullValueDisplayText,
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
                OperatorName = TestData.SafaricomOperatorIdentifier,
                ContractId = TestData.SafaricomContractId,
                Description = TestData.SafaricomContractDescription,
                OperatorId = TestData.SafaricomOperatorId,
                Products = new List<ContractProduct>
                                                 {
                                                     new ContractProduct
                                                     {
                                                         Name = TestData.SafaricomContractProductWithValueName,
                                                         ProductId = TestData.SafaricomContractWithValueProductId,
                                                         Value = TestData.SafaricomContractProductWithValueValue,
                                                         DisplayText = TestData.SafaricomContractProductWithValueDisplayText,
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
                            TestData.VoucherLineTerminator,
                            TestData.VoucherFileFormatHandler);

        public static Guid VoucherFileProfileId = Guid.Parse("079F1FF5-F51E-4BE0-AF4F-2D4862E6D34F");
        public static String VoucherProfileName = "Voucher Profile";

        public static String VoucherListeningDirectory = "/home/txnproc/bulkfiles/voucher";

        public static String VoucherRequestType = "VoucherRequest";

        public static String VoucherLineTerminator = "\n";

        public static String SafaricomFileFormatHandler = "SafaricomFileFormatHandler";

        public static String VoucherFileFormatHandler = "VoucherFileFormatHandler";

        public static DateTime ImportLogStartDate = new DateTime(2021,7,1);
        public static DateTime ImportLogEndDate = new DateTime(2021,7,2);

        public static String RejectionReason = "Invalid Line Format";
        public static string UserEmailAddress = "securityuser@testemail.com";

        public static FileDetails FileDetailsModel => new FileDetails
                                                      {
                                                          EstateId = TestData.EstateId,
                                                          FileId = TestData.FileId,
                                                          FileImportLogId = TestData.FileImportLogId,
                                                          FileLocation = TestData.FileLocation,
                                                          FileProfileId = TestData.FileProfileId,
                                                          MerchantId = TestData.MerchantId,
                                                          ProcessingCompleted = true,
                                                          UserId = TestData.UserId,
                                                          ProcessingSummary = new ProcessingSummary
                                                                              {
                                                                                  FailedLines = 1,
                                                                                  TotalLines = 10,
                                                                                  IgnoredLines = 2,
                                                                                  NotProcessedLines = 1,
                                                                                  SuccessfullyProcessedLines = 4,
                                                                                  RejectedLines = 1
                                                                              },
                                                          FileLines = new List<FIleProcessor.Models.FileLine>
                                                                      {
                                                                          new FIleProcessor.Models.FileLine
                                                                          {
                                                                              ProcessingResult = ProcessingResult.Ignored,
                                                                              LineNumber = 1,
                                                                              LineData = "H",
                                                                              TransactionId = Guid.Empty
                                                                          },
                                                                          new FIleProcessor.Models.FileLine
                                                                          {
                                                                              ProcessingResult = ProcessingResult.Successful,
                                                                              LineNumber = 2,
                                                                              LineData = TestData.GetSafaricomDetailLine("2","200"),
                                                                              TransactionId = Guid.NewGuid()
                                                                          },
                                                                          new FIleProcessor.Models.FileLine
                                                                          {
                                                                              ProcessingResult = ProcessingResult.Successful,
                                                                              LineNumber = 3,
                                                                              LineData = TestData.GetSafaricomDetailLine("3","300"),
                                                                              TransactionId = Guid.NewGuid()
                                                                          },
                                                                          new FIleProcessor.Models.FileLine
                                                                          {
                                                                              ProcessingResult = ProcessingResult.Successful,
                                                                              LineNumber = 4,
                                                                              LineData = TestData.GetSafaricomDetailLine("4","400"),
                                                                              TransactionId = Guid.NewGuid()
                                                                          },
                                                                          new FIleProcessor.Models.FileLine
                                                                          {
                                                                              ProcessingResult = ProcessingResult.Successful,
                                                                              LineNumber = 5,
                                                                              LineData = TestData.GetSafaricomDetailLine("5","500"),
                                                                              TransactionId = Guid.NewGuid()
                                                                          },
                                                                          new FIleProcessor.Models.FileLine
                                                                          {
                                                                              ProcessingResult = ProcessingResult.Failed,
                                                                              LineNumber = 6,
                                                                              LineData = TestData.GetSafaricomDetailLine("6","600"),
                                                                              TransactionId = Guid.Empty
                                                                          },
                                                                          new FIleProcessor.Models.FileLine
                                                                          {
                                                                              ProcessingResult = ProcessingResult.NotProcessed,
                                                                              LineNumber = 7,
                                                                              LineData = TestData.GetSafaricomDetailLine("7","700"),
                                                                              TransactionId = Guid.Empty
                                                                          },
                                                                          new FIleProcessor.Models.FileLine
                                                                          {
                                                                              ProcessingResult = ProcessingResult.Rejected,
                                                                              LineNumber = 8,
                                                                              LineData = TestData.GetSafaricomDetailLine("7","700"),
                                                                              TransactionId = Guid.Empty
                                                                          },
                                                                          new FIleProcessor.Models.FileLine
                                                                          {
                                                                              ProcessingResult = (ProcessingResult)99, // Invalid status
                                                                              LineNumber = 9,
                                                                              LineData = TestData.GetSafaricomDetailLine("8","800"),
                                                                              TransactionId = Guid.Empty
                                                                          },
                                                                          new FIleProcessor.Models.FileLine
                                                                          {
                                                                              ProcessingResult = ProcessingResult.Ignored,
                                                                              LineNumber = 10,
                                                                              LineData = "T",
                                                                              TransactionId = Guid.Empty
                                                                          }
                                                                      }
                                                      };

        public static List<FileImportLog> FileImportLogs =>
            new List<FileImportLog>
            {
                TestData.FileImportLog1,
                TestData.FileImportLog2
            };

        public static List<FIleProcessor.Models.FileImportLog> FileImportLogModels =>
            new List<FIleProcessor.Models.FileImportLog>
            {
                TestData.FileImportLogModel1,
                TestData.FileImportLogModel2
            };

        public static FIleProcessor.Models.FileImportLog FileImportLogModel1 =>
            new FIleProcessor.Models.FileImportLog
            {
                Files = TestData.FileImportLogModel1Files,
                FileImportLogId = TestData.FileImportLogId1,
                EstateId = TestData.EstateId,
                FileImportLogDateTime = TestData.ImportLogStartDate
            };

        public static List<ImportLogFile> FileImportLogModel1Files =>
            new List<ImportLogFile>()
            {
                new ImportLogFile
                {
                    MerchantId = TestData.MerchantId,
                    EstateId = TestData.EstateId,
                    FileId = Guid.NewGuid(),
                    FilePath = "/home/txnproc/file1.csv",
                    FileProfileId = TestData.FileProfileId,
                    OriginalFileName = "Testfile1.csv",
                    UserId = TestData.UserId,
                    UploadedDateTime = TestData.FileUploadedDateTime
                },
                new ImportLogFile
                {
                    MerchantId = TestData.MerchantId,
                    EstateId = TestData.EstateId,
                    FileId = Guid.NewGuid(),
                    FilePath = "/home/txnproc/file2.csv",
                    FileProfileId = TestData.FileProfileId,
                    OriginalFileName = "Testfile2.csv",
                    UserId = TestData.UserId,
                    UploadedDateTime = TestData.FileUploadedDateTime
                }
            };

        public static FIleProcessor.Models.FileImportLog FileImportLogModel2 =>
            new FIleProcessor.Models.FileImportLog
            {
                Files = FileImportLogModel2Files,
                FileImportLogId = TestData.FileImportLogId2,
                EstateId = TestData.EstateId,
                FileImportLogDateTime = TestData.ImportLogEndDate
            };

        public static List<ImportLogFile> FileImportLogModel2Files =>
            new List<ImportLogFile>()
            {
                new ImportLogFile
                {
                    MerchantId = TestData.MerchantId,
                    EstateId = TestData.EstateId,
                    FileId = Guid.NewGuid(),
                    FilePath = "/home/txnproc/file3.csv",
                    FileProfileId = TestData.FileProfileId,
                    OriginalFileName = "Testfile3.csv",
                    UserId = TestData.UserId,
                    UploadedDateTime = TestData.FileUploadedDateTime
                },
                new ImportLogFile
                {
                    MerchantId = TestData.MerchantId,
                    EstateId = TestData.EstateId,
                    FileId = Guid.NewGuid(),
                    FilePath = "/home/txnproc/file2.csv",
                    FileProfileId = TestData.FileProfileId,
                    OriginalFileName = "Testfile2.csv",
                    UserId = TestData.UserId,
                    UploadedDateTime = TestData.FileUploadedDateTime
                }
            };


        public static FileImportLog FileImportLog1 => new FileImportLog
                                                      {
                                                          FileImportLogId = TestData.FileImportLogId1,
                                                          ImportLogDateTime = TestData.ImportLogStartDate,
                                                          EstateId = TestData.EstateId
                                                      };

        public static List<FileImportLogFile> FileImportLog1Files => new List<FileImportLogFile>
                                                                     {
                                                           new FileImportLogFile
                                                           {
                                                               FileImportLogId = TestData.FileImportLogId1,
                                                               MerchantId = TestData.MerchantId,
                                                               EstateId = TestData.EstateId,
                                                               FileId = Guid.NewGuid(),
                                                               FilePath = "/home/txnproc/file1.csv",
                                                               FileProfileId = TestData.FileProfileId,
                                                               OriginalFileName = "Testfile1.csv",
                                                               UserId = TestData.UserId,
                                                               FileUploadedDateTime = TestData.FileUploadedDateTime
                                                           },
                                                           new FileImportLogFile
                                                           {
                                                               FileImportLogId = TestData.FileImportLogId1,
                                                               MerchantId = TestData.MerchantId,
                                                               EstateId = TestData.EstateId,
                                                               FileId = Guid.NewGuid(),
                                                               FilePath = "/home/txnproc/file2.csv",
                                                               FileProfileId = TestData.FileProfileId,
                                                               OriginalFileName = "Testfile2.csv",
                                                               UserId = TestData.UserId,
                                                               FileUploadedDateTime = TestData.FileUploadedDateTime
                                                           },

                                                                     };
        
        public static FileImportLog FileImportLog2 => new FileImportLog
                                                      {
                                                          FileImportLogId = TestData.FileImportLogId2,
                                                          ImportLogDateTime = TestData.ImportLogEndDate,
                                                          EstateId = TestData.EstateId
                                                      };

        public static List<FileImportLogFile> FileImportLog2Files => new List<FileImportLogFile>
                                                                     {
                                                           new FileImportLogFile
                                                           {
                                                               FileImportLogId = TestData.FileImportLogId2,
                                                               MerchantId = TestData.MerchantId,
                                                               EstateId = TestData.EstateId,
                                                               FileId = Guid.NewGuid(),
                                                               FilePath = "/home/txnproc/file3.csv",
                                                               FileProfileId = TestData.FileProfileId,
                                                               OriginalFileName = "Testfile3.csv",
                                                               UserId = TestData.UserId,
                                                               FileUploadedDateTime = TestData.FileUploadedDateTime
                                                           },
                                                           new FileImportLogFile
                                                           {
                                                               FileImportLogId = TestData.FileImportLogId2,
                                                               MerchantId = TestData.MerchantId,
                                                               EstateId = TestData.EstateId,
                                                               FileId = Guid.NewGuid(),
                                                               FilePath = "/home/txnproc/file4.csv",
                                                               FileProfileId = TestData.FileProfileId,
                                                               OriginalFileName = "Testfile4.csv",
                                                               UserId = TestData.UserId,
                                                               FileUploadedDateTime = TestData.FileUploadedDateTime
                                                           },

                                                                     };
    }
}
