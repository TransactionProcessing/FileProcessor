﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (https://www.specflow.org/).
//      SpecFlow Version:3.5.0.0
//      SpecFlow Generator Version:3.5.0.0
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace FileProcessor.IntegrationTests.Features
{
    using TechTalk.SpecFlow;
    using System;
    using System.Linq;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "3.5.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [Xunit.TraitAttribute("Category", "base")]
    [Xunit.TraitAttribute("Category", "shared")]
    [Xunit.TraitAttribute("Category", "getfileimportdetails")]
    public partial class GetFileImportDetailsFeature : object, Xunit.IClassFixture<GetFileImportDetailsFeature.FixtureData>, System.IDisposable
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
        private string[] _featureTags = new string[] {
                "base",
                "shared",
                "getfileimportdetails"};
        
        private Xunit.Abstractions.ITestOutputHelper _testOutputHelper;
        
#line 1 "GetFileImportDetails.feature"
#line hidden
        
        public GetFileImportDetailsFeature(GetFileImportDetailsFeature.FixtureData fixtureData, FileProcessor_IntegrationTests_XUnitAssemblyFixture assemblyFixture, Xunit.Abstractions.ITestOutputHelper testOutputHelper)
        {
            this._testOutputHelper = testOutputHelper;
            this.TestInitialize();
        }
        
        public static void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Features", "GetFileImportDetails", null, ProgrammingLanguage.CSharp, new string[] {
                        "base",
                        "shared",
                        "getfileimportdetails"});
            testRunner.OnFeatureStart(featureInfo);
        }
        
        public static void FeatureTearDown()
        {
            testRunner.OnFeatureEnd();
            testRunner = null;
        }
        
        public virtual void TestInitialize()
        {
        }
        
        public virtual void TestTearDown()
        {
            testRunner.OnScenarioEnd();
        }
        
        public virtual void ScenarioInitialize(TechTalk.SpecFlow.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioInitialize(scenarioInfo);
            testRunner.ScenarioContext.ScenarioContainer.RegisterInstanceAs<Xunit.Abstractions.ITestOutputHelper>(_testOutputHelper);
        }
        
        public virtual void ScenarioStart()
        {
            testRunner.OnScenarioStart();
        }
        
        public virtual void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
        
        public virtual void FeatureBackground()
        {
#line 4
#line hidden
            TechTalk.SpecFlow.Table table1 = new TechTalk.SpecFlow.Table(new string[] {
                        "Name",
                        "DisplayName",
                        "Description"});
            table1.AddRow(new string[] {
                        "estateManagement",
                        "Estate Managememt REST Scope",
                        "A scope for Estate Managememt REST"});
            table1.AddRow(new string[] {
                        "transactionProcessor",
                        "Transaction Processor REST  Scope",
                        "A scope for Transaction Processor REST"});
            table1.AddRow(new string[] {
                        "voucherManagement",
                        "Voucher Management REST  Scope",
                        "A scope for Voucher Management REST"});
            table1.AddRow(new string[] {
                        "fileProcessor",
                        "File Processor REST Scope",
                        "A scope for File Processor REST"});
#line 5
 testRunner.Given("I create the following api scopes", ((string)(null)), table1, "Given ");
#line hidden
            TechTalk.SpecFlow.Table table2 = new TechTalk.SpecFlow.Table(new string[] {
                        "ResourceName",
                        "DisplayName",
                        "Secret",
                        "Scopes",
                        "UserClaims"});
            table2.AddRow(new string[] {
                        "estateManagement",
                        "Estate Managememt REST",
                        "Secret1",
                        "estateManagement",
                        "MerchantId, EstateId, role"});
            table2.AddRow(new string[] {
                        "transactionProcessor",
                        "Transaction Processor REST",
                        "Secret1",
                        "transactionProcessor",
                        ""});
            table2.AddRow(new string[] {
                        "voucherManagement",
                        "Voucher Management REST",
                        "Secret1",
                        "voucherManagement",
                        ""});
            table2.AddRow(new string[] {
                        "fileProcessor",
                        "File Processor REST",
                        "Secret1",
                        "fileProcessor",
                        ""});
#line 12
 testRunner.Given("the following api resources exist", ((string)(null)), table2, "Given ");
#line hidden
            TechTalk.SpecFlow.Table table3 = new TechTalk.SpecFlow.Table(new string[] {
                        "ClientId",
                        "ClientName",
                        "Secret",
                        "AllowedScopes",
                        "AllowedGrantTypes"});
            table3.AddRow(new string[] {
                        "serviceClient",
                        "Service Client",
                        "Secret1",
                        "estateManagement,transactionProcessor,voucherManagement,fileProcessor",
                        "client_credentials"});
#line 19
 testRunner.Given("the following clients exist", ((string)(null)), table3, "Given ");
#line hidden
            TechTalk.SpecFlow.Table table4 = new TechTalk.SpecFlow.Table(new string[] {
                        "ClientId"});
            table4.AddRow(new string[] {
                        "serviceClient"});
#line 23
 testRunner.Given("I have a token to access the estate management and transaction processor resource" +
                    "s", ((string)(null)), table4, "Given ");
#line hidden
            TechTalk.SpecFlow.Table table5 = new TechTalk.SpecFlow.Table(new string[] {
                        "EstateName"});
            table5.AddRow(new string[] {
                        "Test Estate 1"});
#line 27
 testRunner.Given("I have created the following estates", ((string)(null)), table5, "Given ");
#line hidden
            TechTalk.SpecFlow.Table table6 = new TechTalk.SpecFlow.Table(new string[] {
                        "EstateName",
                        "OperatorName",
                        "RequireCustomMerchantNumber",
                        "RequireCustomTerminalNumber"});
            table6.AddRow(new string[] {
                        "Test Estate 1",
                        "Safaricom",
                        "True",
                        "True"});
            table6.AddRow(new string[] {
                        "Test Estate 1",
                        "Voucher",
                        "True",
                        "True"});
#line 31
 testRunner.Given("I have created the following operators", ((string)(null)), table6, "Given ");
#line hidden
            TechTalk.SpecFlow.Table table7 = new TechTalk.SpecFlow.Table(new string[] {
                        "EstateName",
                        "OperatorName",
                        "ContractDescription"});
            table7.AddRow(new string[] {
                        "Test Estate 1",
                        "Safaricom",
                        "Safaricom Contract"});
            table7.AddRow(new string[] {
                        "Test Estate 1",
                        "Voucher",
                        "Hospital 1 Contract"});
#line 36
 testRunner.Given("I create a contract with the following values", ((string)(null)), table7, "Given ");
#line hidden
            TechTalk.SpecFlow.Table table8 = new TechTalk.SpecFlow.Table(new string[] {
                        "EstateName",
                        "OperatorName",
                        "ContractDescription",
                        "ProductName",
                        "DisplayText",
                        "Value"});
            table8.AddRow(new string[] {
                        "Test Estate 1",
                        "Safaricom",
                        "Safaricom Contract",
                        "Variable Topup",
                        "Custom",
                        ""});
            table8.AddRow(new string[] {
                        "Test Estate 1",
                        "Voucher",
                        "Hospital 1 Contract",
                        "10 KES",
                        "10 KES",
                        ""});
#line 41
 testRunner.When("I create the following Products", ((string)(null)), table8, "When ");
#line hidden
            TechTalk.SpecFlow.Table table9 = new TechTalk.SpecFlow.Table(new string[] {
                        "EstateName",
                        "OperatorName",
                        "ContractDescription",
                        "ProductName",
                        "CalculationType",
                        "FeeDescription",
                        "Value"});
            table9.AddRow(new string[] {
                        "Test Estate 1",
                        "Safaricom",
                        "Safaricom Contract",
                        "Variable Topup",
                        "Fixed",
                        "Merchant Commission",
                        "2.50"});
#line 46
 testRunner.When("I add the following Transaction Fees", ((string)(null)), table9, "When ");
#line hidden
            TechTalk.SpecFlow.Table table10 = new TechTalk.SpecFlow.Table(new string[] {
                        "MerchantName",
                        "AddressLine1",
                        "Town",
                        "Region",
                        "Country",
                        "ContactName",
                        "EmailAddress",
                        "EstateName"});
            table10.AddRow(new string[] {
                        "Test Merchant 1",
                        "Address Line 1",
                        "TestTown",
                        "Test Region",
                        "United Kingdom",
                        "Test Contact 1",
                        "testcontact1@merchant1.co.uk",
                        "Test Estate 1"});
            table10.AddRow(new string[] {
                        "Test Merchant 2",
                        "Address Line 1",
                        "TestTown",
                        "Test Region",
                        "United Kingdom",
                        "Test Contact 1",
                        "testcontact1@merchant2.co.uk",
                        "Test Estate 1"});
#line 50
 testRunner.Given("I create the following merchants", ((string)(null)), table10, "Given ");
#line hidden
            TechTalk.SpecFlow.Table table11 = new TechTalk.SpecFlow.Table(new string[] {
                        "OperatorName",
                        "MerchantName",
                        "MerchantNumber",
                        "TerminalNumber",
                        "EstateName"});
            table11.AddRow(new string[] {
                        "Safaricom",
                        "Test Merchant 1",
                        "00000001",
                        "10000001",
                        "Test Estate 1"});
            table11.AddRow(new string[] {
                        "Voucher",
                        "Test Merchant 1",
                        "00000001",
                        "10000001",
                        "Test Estate 1"});
            table11.AddRow(new string[] {
                        "Safaricom",
                        "Test Merchant 2",
                        "00000002",
                        "10000002",
                        "Test Estate 1"});
            table11.AddRow(new string[] {
                        "Voucher",
                        "Test Merchant 2",
                        "00000002",
                        "10000002",
                        "Test Estate 1"});
#line 55
 testRunner.Given("I have assigned the following  operator to the merchants", ((string)(null)), table11, "Given ");
#line hidden
            TechTalk.SpecFlow.Table table12 = new TechTalk.SpecFlow.Table(new string[] {
                        "DeviceIdentifier",
                        "MerchantName",
                        "EstateName"});
            table12.AddRow(new string[] {
                        "123456780",
                        "Test Merchant 1",
                        "Test Estate 1"});
            table12.AddRow(new string[] {
                        "123456781",
                        "Test Merchant 2",
                        "Test Estate 1"});
#line 62
 testRunner.Given("I have assigned the following devices to the merchants", ((string)(null)), table12, "Given ");
#line hidden
            TechTalk.SpecFlow.Table table13 = new TechTalk.SpecFlow.Table(new string[] {
                        "Reference",
                        "Amount",
                        "DateTime",
                        "MerchantName",
                        "EstateName"});
            table13.AddRow(new string[] {
                        "Deposit1",
                        "300.00",
                        "Today",
                        "Test Merchant 1",
                        "Test Estate 1"});
            table13.AddRow(new string[] {
                        "Deposit1",
                        "300.00",
                        "Today",
                        "Test Merchant 2",
                        "Test Estate 1"});
#line 67
 testRunner.Given("I make the following manual merchant deposits", ((string)(null)), table13, "Given ");
#line hidden
        }
        
        void System.IDisposable.Dispose()
        {
            this.TestTearDown();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Get File Import Log Details")]
        [Xunit.TraitAttribute("FeatureTitle", "GetFileImportDetails")]
        [Xunit.TraitAttribute("Description", "Get File Import Log Details")]
        [Xunit.TraitAttribute("Category", "PRTest")]
        public virtual void GetFileImportLogDetails()
        {
            string[] tagsOfScenario = new string[] {
                    "PRTest"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Get File Import Log Details", null, tagsOfScenario, argumentsOfScenario);
#line 73
this.ScenarioInitialize(scenarioInfo);
#line hidden
            bool isScenarioIgnored = default(bool);
            bool isFeatureIgnored = default(bool);
            if ((tagsOfScenario != null))
            {
                isScenarioIgnored = tagsOfScenario.Where(__entry => __entry != null).Where(__entry => String.Equals(__entry, "ignore", StringComparison.CurrentCultureIgnoreCase)).Any();
            }
            if ((this._featureTags != null))
            {
                isFeatureIgnored = this._featureTags.Where(__entry => __entry != null).Where(__entry => String.Equals(__entry, "ignore", StringComparison.CurrentCultureIgnoreCase)).Any();
            }
            if ((isScenarioIgnored || isFeatureIgnored))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 4
this.FeatureBackground();
#line hidden
                TechTalk.SpecFlow.Table table14 = new TechTalk.SpecFlow.Table(new string[] {
                            "Column1",
                            "Column2",
                            "Column3"});
                table14.AddRow(new string[] {
                            "H",
                            "20210508",
                            ""});
                table14.AddRow(new string[] {
                            "D",
                            "07777777775",
                            "100"});
                table14.AddRow(new string[] {
                            "T",
                            "1",
                            ""});
#line 74
 testRunner.Given("I have a file named \'SafarcomTopup1.txt\' with the following contents", ((string)(null)), table14, "Given ");
#line hidden
                TechTalk.SpecFlow.Table table15 = new TechTalk.SpecFlow.Table(new string[] {
                            "EstateName",
                            "MerchantName",
                            "FileProfileId",
                            "UserId"});
                table15.AddRow(new string[] {
                            "Test Estate 1",
                            "Test Merchant 1",
                            "B2A59ABF-293D-4A6B-B81B-7007503C3476",
                            "ABA59ABF-293D-4A6B-B81B-7007503C3476"});
#line 79
 testRunner.And("I upload this file for processing", ((string)(null)), table15, "And ");
#line hidden
                TechTalk.SpecFlow.Table table16 = new TechTalk.SpecFlow.Table(new string[] {
                            "Column1",
                            "Column2",
                            "Column3"});
                table16.AddRow(new string[] {
                            "H",
                            "20210508",
                            ""});
                table16.AddRow(new string[] {
                            "D",
                            "07777777776",
                            "150"});
                table16.AddRow(new string[] {
                            "T",
                            "1",
                            ""});
#line 83
 testRunner.Given("I have a file named \'SafarcomTopup2.txt\' with the following contents", ((string)(null)), table16, "Given ");
#line hidden
                TechTalk.SpecFlow.Table table17 = new TechTalk.SpecFlow.Table(new string[] {
                            "EstateName",
                            "MerchantName",
                            "FileProfileId",
                            "UserId"});
                table17.AddRow(new string[] {
                            "Test Estate 1",
                            "Test Merchant 2",
                            "B2A59ABF-293D-4A6B-B81B-7007503C3476",
                            "ABA59ABF-293D-4A6B-B81B-7007503C3476"});
#line 88
 testRunner.And("I upload this file for processing", ((string)(null)), table17, "And ");
#line hidden
                TechTalk.SpecFlow.Table table18 = new TechTalk.SpecFlow.Table(new string[] {
                            "Column1",
                            "Column2",
                            "Column3",
                            "Column4"});
                table18.AddRow(new string[] {
                            "H",
                            "20210508",
                            "",
                            ""});
                table18.AddRow(new string[] {
                            "D",
                            "Hospital 1",
                            "07777777775",
                            "10"});
                table18.AddRow(new string[] {
                            "D",
                            "Hospital 1",
                            "testrecipient1@recipient.com",
                            "10"});
                table18.AddRow(new string[] {
                            "T",
                            "1",
                            "",
                            ""});
#line 92
 testRunner.Given("I have a file named \'VoucherIssue1.txt\' with the following contents", ((string)(null)), table18, "Given ");
#line hidden
                TechTalk.SpecFlow.Table table19 = new TechTalk.SpecFlow.Table(new string[] {
                            "EstateName",
                            "MerchantName",
                            "FileProfileId",
                            "UserId"});
                table19.AddRow(new string[] {
                            "Test Estate 1",
                            "Test Merchant 1",
                            "8806EDBC-3ED6-406B-9E5F-A9078356BE99",
                            "ABA59ABF-293D-4A6B-B81B-7007503C3476"});
#line 98
 testRunner.And("I upload this file for processing", ((string)(null)), table19, "And ");
#line hidden
                TechTalk.SpecFlow.Table table20 = new TechTalk.SpecFlow.Table(new string[] {
                            "ImportLogDate",
                            "FileCount"});
                table20.AddRow(new string[] {
                            "Today",
                            "3"});
#line 102
 testRunner.When("I get the \'Test Estate 1\' import logs between \'Yesterday\' and \'Today\' the followi" +
                        "ng data is returned", ((string)(null)), table20, "When ");
#line hidden
                TechTalk.SpecFlow.Table table21 = new TechTalk.SpecFlow.Table(new string[] {
                            "MerchantName",
                            "OriginalFileName"});
                table21.AddRow(new string[] {
                            "Test Merchant 1",
                            "SafarcomTopup1.txt"});
                table21.AddRow(new string[] {
                            "Test Merchant 2",
                            "SafarcomTopup2.txt"});
                table21.AddRow(new string[] {
                            "Test Merchant 1",
                            "VoucherIssue1.txt"});
#line 106
 testRunner.When("I get the \'Test Estate 1\' import log for \'Today\' the following file information i" +
                        "s returned", ((string)(null)), table21, "When ");
#line hidden
                TechTalk.SpecFlow.Table table22 = new TechTalk.SpecFlow.Table(new string[] {
                            "ProcessingCompleted",
                            "NumberOfLines",
                            "TotaLines",
                            "SuccessfulLines",
                            "IgnoredLines",
                            "FailedLines",
                            "NotProcessedLines"});
                table22.AddRow(new string[] {
                            "True",
                            "3",
                            "3",
                            "1",
                            "2",
                            "0",
                            "0"});
#line 112
 testRunner.When("I get the file \'SafarcomTopup1.txt\' for Estate \'Test Estate 1\' the following file" +
                        " information is returned", ((string)(null)), table22, "When ");
#line hidden
                TechTalk.SpecFlow.Table table23 = new TechTalk.SpecFlow.Table(new string[] {
                            "LineNumber",
                            "Data",
                            "Result"});
                table23.AddRow(new string[] {
                            "1",
                            "H,20210508,",
                            "Ignored"});
                table23.AddRow(new string[] {
                            "2",
                            "D,07777777775,100",
                            "Successful"});
                table23.AddRow(new string[] {
                            "3",
                            "T,1,",
                            "Ignored"});
#line 116
 testRunner.When("I get the file \'SafarcomTopup1.txt\' for Estate \'Test Estate 1\' the following file" +
                        " lines are returned", ((string)(null)), table23, "When ");
#line hidden
                TechTalk.SpecFlow.Table table24 = new TechTalk.SpecFlow.Table(new string[] {
                            "ProcessingCompleted",
                            "NumberOfLines",
                            "TotaLines",
                            "SuccessfulLines",
                            "IgnoredLines",
                            "FailedLines",
                            "NotProcessedLines"});
                table24.AddRow(new string[] {
                            "True",
                            "3",
                            "3",
                            "1",
                            "2",
                            "0",
                            "0"});
#line 122
 testRunner.When("I get the file \'SafarcomTopup2.txt\' for Estate \'Test Estate 1\' the following file" +
                        " information is returned", ((string)(null)), table24, "When ");
#line hidden
                TechTalk.SpecFlow.Table table25 = new TechTalk.SpecFlow.Table(new string[] {
                            "LineNumber",
                            "Data",
                            "Result"});
                table25.AddRow(new string[] {
                            "1",
                            "H,20210508,",
                            "Ignored"});
                table25.AddRow(new string[] {
                            "2",
                            "D,07777777776,150",
                            "Successful"});
                table25.AddRow(new string[] {
                            "3",
                            "T,1,",
                            "Ignored"});
#line 126
 testRunner.When("I get the file \'SafarcomTopup2.txt\' for Estate \'Test Estate 1\' the following file" +
                        " lines are returned", ((string)(null)), table25, "When ");
#line hidden
                TechTalk.SpecFlow.Table table26 = new TechTalk.SpecFlow.Table(new string[] {
                            "ProcessingCompleted",
                            "NumberOfLines",
                            "TotaLines",
                            "SuccessfulLines",
                            "IgnoredLines",
                            "FailedLines",
                            "NotProcessedLines"});
                table26.AddRow(new string[] {
                            "True",
                            "4",
                            "4",
                            "2",
                            "2",
                            "0",
                            "0"});
#line 132
 testRunner.When("I get the file \'VoucherIssue1.txt\' for Estate \'Test Estate 1\' the following file " +
                        "information is returned", ((string)(null)), table26, "When ");
#line hidden
                TechTalk.SpecFlow.Table table27 = new TechTalk.SpecFlow.Table(new string[] {
                            "LineNumber",
                            "Data",
                            "Result"});
                table27.AddRow(new string[] {
                            "1",
                            "H,20210508,,",
                            "Ignored"});
                table27.AddRow(new string[] {
                            "2",
                            "D,Hospital 1,07777777775,10",
                            "Successful"});
                table27.AddRow(new string[] {
                            "3",
                            "D,Hospital 1,testrecipient1@recipient.com,10",
                            "Successful"});
                table27.AddRow(new string[] {
                            "4",
                            "T,1,,",
                            "Ignored"});
#line 136
 testRunner.When("I get the file \'VoucherIssue1.txt\' for Estate \'Test Estate 1\' the following file " +
                        "lines are returned", ((string)(null)), table27, "When ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "3.5.0.0")]
        [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
        public class FixtureData : System.IDisposable
        {
            
            public FixtureData()
            {
                GetFileImportDetailsFeature.FeatureSetup();
            }
            
            void System.IDisposable.Dispose()
            {
                GetFileImportDetailsFeature.FeatureTearDown();
            }
        }
    }
}
#pragma warning restore
#endregion
