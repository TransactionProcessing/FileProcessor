﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (https://www.specflow.org/).
//      SpecFlow Version:3.9.0.0
//      SpecFlow Generator Version:3.9.0.0
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
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "3.9.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [Xunit.TraitAttribute("Category", "base")]
    [Xunit.TraitAttribute("Category", "shared")]
    public partial class ProcessTopupCSVFilesFeature : object, Xunit.IClassFixture<ProcessTopupCSVFilesFeature.FixtureData>, System.IDisposable
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
        private static string[] featureTags = new string[] {
                "base",
                "shared"};
        
        private Xunit.Abstractions.ITestOutputHelper _testOutputHelper;
        
#line 1 "ProcessTopupCSV.feature"
#line hidden
        
        public ProcessTopupCSVFilesFeature(ProcessTopupCSVFilesFeature.FixtureData fixtureData, FileProcessor_IntegrationTests_XUnitAssemblyFixture assemblyFixture, Xunit.Abstractions.ITestOutputHelper testOutputHelper)
        {
            this._testOutputHelper = testOutputHelper;
            this.TestInitialize();
        }
        
        public static void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Features", "Process Topup CSV Files", null, ProgrammingLanguage.CSharp, featureTags);
            testRunner.OnFeatureStart(featureInfo);
        }
        
        public static void FeatureTearDown()
        {
            testRunner.OnFeatureEnd();
            testRunner = null;
        }
        
        public void TestInitialize()
        {
        }
        
        public void TestTearDown()
        {
            testRunner.OnScenarioEnd();
        }
        
        public void ScenarioInitialize(TechTalk.SpecFlow.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioInitialize(scenarioInfo);
            testRunner.ScenarioContext.ScenarioContainer.RegisterInstanceAs<Xunit.Abstractions.ITestOutputHelper>(_testOutputHelper);
        }
        
        public void ScenarioStart()
        {
            testRunner.OnScenarioStart();
        }
        
        public void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
        
        public virtual void FeatureBackground()
        {
#line 4
#line hidden
            TechTalk.SpecFlow.Table table28 = new TechTalk.SpecFlow.Table(new string[] {
                        "Name",
                        "DisplayName",
                        "Description"});
            table28.AddRow(new string[] {
                        "estateManagement",
                        "Estate Managememt REST Scope",
                        "A scope for Estate Managememt REST"});
            table28.AddRow(new string[] {
                        "transactionProcessor",
                        "Transaction Processor REST  Scope",
                        "A scope for Transaction Processor REST"});
            table28.AddRow(new string[] {
                        "voucherManagement",
                        "Voucher Management REST  Scope",
                        "A scope for Voucher Management REST"});
            table28.AddRow(new string[] {
                        "fileProcessor",
                        "File Processor REST Scope",
                        "A scope for File Processor REST"});
#line 5
 testRunner.Given("I create the following api scopes", ((string)(null)), table28, "Given ");
#line hidden
            TechTalk.SpecFlow.Table table29 = new TechTalk.SpecFlow.Table(new string[] {
                        "ResourceName",
                        "DisplayName",
                        "Secret",
                        "Scopes",
                        "UserClaims"});
            table29.AddRow(new string[] {
                        "estateManagement",
                        "Estate Managememt REST",
                        "Secret1",
                        "estateManagement",
                        "MerchantId, EstateId, role"});
            table29.AddRow(new string[] {
                        "transactionProcessor",
                        "Transaction Processor REST",
                        "Secret1",
                        "transactionProcessor",
                        ""});
            table29.AddRow(new string[] {
                        "voucherManagement",
                        "Voucher Management REST",
                        "Secret1",
                        "voucherManagement",
                        ""});
            table29.AddRow(new string[] {
                        "fileProcessor",
                        "File Processor REST",
                        "Secret1",
                        "fileProcessor",
                        ""});
#line 12
 testRunner.Given("the following api resources exist", ((string)(null)), table29, "Given ");
#line hidden
            TechTalk.SpecFlow.Table table30 = new TechTalk.SpecFlow.Table(new string[] {
                        "ClientId",
                        "ClientName",
                        "Secret",
                        "AllowedScopes",
                        "AllowedGrantTypes"});
            table30.AddRow(new string[] {
                        "serviceClient",
                        "Service Client",
                        "Secret1",
                        "estateManagement,transactionProcessor,voucherManagement,fileProcessor",
                        "client_credentials"});
#line 19
 testRunner.Given("the following clients exist", ((string)(null)), table30, "Given ");
#line hidden
            TechTalk.SpecFlow.Table table31 = new TechTalk.SpecFlow.Table(new string[] {
                        "ClientId"});
            table31.AddRow(new string[] {
                        "serviceClient"});
#line 23
 testRunner.Given("I have a token to access the estate management and transaction processor resource" +
                    "s", ((string)(null)), table31, "Given ");
#line hidden
            TechTalk.SpecFlow.Table table32 = new TechTalk.SpecFlow.Table(new string[] {
                        "EstateName"});
            table32.AddRow(new string[] {
                        "Test Estate 1"});
#line 27
 testRunner.Given("I have created the following estates", ((string)(null)), table32, "Given ");
#line hidden
            TechTalk.SpecFlow.Table table33 = new TechTalk.SpecFlow.Table(new string[] {
                        "EstateName",
                        "OperatorName",
                        "RequireCustomMerchantNumber",
                        "RequireCustomTerminalNumber"});
            table33.AddRow(new string[] {
                        "Test Estate 1",
                        "Safaricom",
                        "True",
                        "True"});
            table33.AddRow(new string[] {
                        "Test Estate 1",
                        "Voucher",
                        "True",
                        "True"});
#line 31
 testRunner.Given("I have created the following operators", ((string)(null)), table33, "Given ");
#line hidden
            TechTalk.SpecFlow.Table table34 = new TechTalk.SpecFlow.Table(new string[] {
                        "EstateName",
                        "OperatorName",
                        "ContractDescription"});
            table34.AddRow(new string[] {
                        "Test Estate 1",
                        "Safaricom",
                        "Safaricom Contract"});
            table34.AddRow(new string[] {
                        "Test Estate 1",
                        "Voucher",
                        "Hospital 1 Contract"});
#line 36
 testRunner.Given("I create a contract with the following values", ((string)(null)), table34, "Given ");
#line hidden
            TechTalk.SpecFlow.Table table35 = new TechTalk.SpecFlow.Table(new string[] {
                        "EstateName",
                        "OperatorName",
                        "ContractDescription",
                        "ProductName",
                        "DisplayText",
                        "Value"});
            table35.AddRow(new string[] {
                        "Test Estate 1",
                        "Safaricom",
                        "Safaricom Contract",
                        "Variable Topup",
                        "Custom",
                        ""});
            table35.AddRow(new string[] {
                        "Test Estate 1",
                        "Voucher",
                        "Hospital 1 Contract",
                        "10 KES",
                        "10 KES",
                        ""});
#line 41
 testRunner.When("I create the following Products", ((string)(null)), table35, "When ");
#line hidden
            TechTalk.SpecFlow.Table table36 = new TechTalk.SpecFlow.Table(new string[] {
                        "EstateName",
                        "OperatorName",
                        "ContractDescription",
                        "ProductName",
                        "CalculationType",
                        "FeeDescription",
                        "Value"});
            table36.AddRow(new string[] {
                        "Test Estate 1",
                        "Safaricom",
                        "Safaricom Contract",
                        "Variable Topup",
                        "Fixed",
                        "Merchant Commission",
                        "2.50"});
#line 46
 testRunner.When("I add the following Transaction Fees", ((string)(null)), table36, "When ");
#line hidden
            TechTalk.SpecFlow.Table table37 = new TechTalk.SpecFlow.Table(new string[] {
                        "MerchantName",
                        "AddressLine1",
                        "Town",
                        "Region",
                        "Country",
                        "ContactName",
                        "EmailAddress",
                        "EstateName"});
            table37.AddRow(new string[] {
                        "Test Merchant 1",
                        "Address Line 1",
                        "TestTown",
                        "Test Region",
                        "United Kingdom",
                        "Test Contact 1",
                        "testcontact1@merchant1.co.uk",
                        "Test Estate 1"});
#line 50
 testRunner.Given("I create the following merchants", ((string)(null)), table37, "Given ");
#line hidden
            TechTalk.SpecFlow.Table table38 = new TechTalk.SpecFlow.Table(new string[] {
                        "OperatorName",
                        "MerchantName",
                        "MerchantNumber",
                        "TerminalNumber",
                        "EstateName"});
            table38.AddRow(new string[] {
                        "Safaricom",
                        "Test Merchant 1",
                        "00000001",
                        "10000001",
                        "Test Estate 1"});
            table38.AddRow(new string[] {
                        "Voucher",
                        "Test Merchant 1",
                        "00000001",
                        "10000001",
                        "Test Estate 1"});
#line 54
 testRunner.Given("I have assigned the following  operator to the merchants", ((string)(null)), table38, "Given ");
#line hidden
            TechTalk.SpecFlow.Table table39 = new TechTalk.SpecFlow.Table(new string[] {
                        "DeviceIdentifier",
                        "MerchantName",
                        "EstateName"});
            table39.AddRow(new string[] {
                        "123456780",
                        "Test Merchant 1",
                        "Test Estate 1"});
#line 59
 testRunner.Given("I have assigned the following devices to the merchants", ((string)(null)), table39, "Given ");
#line hidden
            TechTalk.SpecFlow.Table table40 = new TechTalk.SpecFlow.Table(new string[] {
                        "Reference",
                        "Amount",
                        "DateTime",
                        "MerchantName",
                        "EstateName"});
            table40.AddRow(new string[] {
                        "Deposit1",
                        "300.00",
                        "Today",
                        "Test Merchant 1",
                        "Test Estate 1"});
#line 63
 testRunner.Given("I make the following manual merchant deposits", ((string)(null)), table40, "Given ");
#line hidden
        }
        
        void System.IDisposable.Dispose()
        {
            this.TestTearDown();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Process Safaricom Topup File with 1 detail row")]
        [Xunit.TraitAttribute("FeatureTitle", "Process Topup CSV Files")]
        [Xunit.TraitAttribute("Description", "Process Safaricom Topup File with 1 detail row")]
        public void ProcessSafaricomTopupFileWith1DetailRow()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Process Safaricom Topup File with 1 detail row", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 67
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 4
this.FeatureBackground();
#line hidden
                TechTalk.SpecFlow.Table table41 = new TechTalk.SpecFlow.Table(new string[] {
                            "Column1",
                            "Column2",
                            "Column3"});
                table41.AddRow(new string[] {
                            "H",
                            "20210508",
                            ""});
                table41.AddRow(new string[] {
                            "D",
                            "07777777775",
                            "100"});
                table41.AddRow(new string[] {
                            "T",
                            "1",
                            ""});
#line 68
 testRunner.Given("I have a file named \'SafarcomTopup.txt\' with the following contents", ((string)(null)), table41, "Given ");
#line hidden
                TechTalk.SpecFlow.Table table42 = new TechTalk.SpecFlow.Table(new string[] {
                            "EstateName",
                            "MerchantName",
                            "FileProfileId",
                            "UserId"});
                table42.AddRow(new string[] {
                            "Test Estate 1",
                            "Test Merchant 1",
                            "B2A59ABF-293D-4A6B-B81B-7007503C3476",
                            "ABA59ABF-293D-4A6B-B81B-7007503C3476"});
#line 73
 testRunner.And("I upload this file for processing", ((string)(null)), table42, "And ");
#line hidden
#line 77
 testRunner.When("As merchant \"Test Merchant 1\" on Estate \"Test Estate 1\" I get my transactions 1 t" +
                        "ransaction should be returned", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Process Safaricom Topup File with 2 detail rows")]
        [Xunit.TraitAttribute("FeatureTitle", "Process Topup CSV Files")]
        [Xunit.TraitAttribute("Description", "Process Safaricom Topup File with 2 detail rows")]
        public void ProcessSafaricomTopupFileWith2DetailRows()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Process Safaricom Topup File with 2 detail rows", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 79
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 4
this.FeatureBackground();
#line hidden
                TechTalk.SpecFlow.Table table43 = new TechTalk.SpecFlow.Table(new string[] {
                            "Column1",
                            "Column2",
                            "Column3"});
                table43.AddRow(new string[] {
                            "H",
                            "20210508",
                            ""});
                table43.AddRow(new string[] {
                            "D",
                            "07777777775",
                            "100"});
                table43.AddRow(new string[] {
                            "D",
                            "07777777776",
                            "200"});
                table43.AddRow(new string[] {
                            "T",
                            "2",
                            ""});
#line 80
 testRunner.Given("I have a file named \'SafarcomTopup.txt\' with the following contents", ((string)(null)), table43, "Given ");
#line hidden
                TechTalk.SpecFlow.Table table44 = new TechTalk.SpecFlow.Table(new string[] {
                            "EstateName",
                            "MerchantName",
                            "FileProfileId",
                            "UserId"});
                table44.AddRow(new string[] {
                            "Test Estate 1",
                            "Test Merchant 1",
                            "B2A59ABF-293D-4A6B-B81B-7007503C3476",
                            "ABA59ABF-293D-4A6B-B81B-7007503C3476"});
#line 86
 testRunner.And("I upload this file for processing", ((string)(null)), table44, "And ");
#line hidden
#line 90
 testRunner.When("As merchant \"Test Merchant 1\" on Estate \"Test Estate 1\" I get my transactions 2 t" +
                        "ransaction should be returned", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Process 2 Safaricom Topup Files")]
        [Xunit.TraitAttribute("FeatureTitle", "Process Topup CSV Files")]
        [Xunit.TraitAttribute("Description", "Process 2 Safaricom Topup Files")]
        public void Process2SafaricomTopupFiles()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Process 2 Safaricom Topup Files", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 92
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 4
this.FeatureBackground();
#line hidden
                TechTalk.SpecFlow.Table table45 = new TechTalk.SpecFlow.Table(new string[] {
                            "Column1",
                            "Column2",
                            "Column3"});
                table45.AddRow(new string[] {
                            "H",
                            "20210508",
                            ""});
                table45.AddRow(new string[] {
                            "D",
                            "07777777775",
                            "100"});
                table45.AddRow(new string[] {
                            "T",
                            "1",
                            ""});
#line 93
 testRunner.Given("I have a file named \'SafarcomTopup1.txt\' with the following contents", ((string)(null)), table45, "Given ");
#line hidden
                TechTalk.SpecFlow.Table table46 = new TechTalk.SpecFlow.Table(new string[] {
                            "EstateName",
                            "MerchantName",
                            "FileProfileId",
                            "UserId"});
                table46.AddRow(new string[] {
                            "Test Estate 1",
                            "Test Merchant 1",
                            "B2A59ABF-293D-4A6B-B81B-7007503C3476",
                            "ABA59ABF-293D-4A6B-B81B-7007503C3476"});
#line 98
 testRunner.And("I upload this file for processing", ((string)(null)), table46, "And ");
#line hidden
                TechTalk.SpecFlow.Table table47 = new TechTalk.SpecFlow.Table(new string[] {
                            "Column1",
                            "Column2",
                            "Column3"});
                table47.AddRow(new string[] {
                            "H",
                            "20210508",
                            ""});
                table47.AddRow(new string[] {
                            "D",
                            "07777777776",
                            "150"});
                table47.AddRow(new string[] {
                            "D",
                            "07777777777",
                            "50"});
                table47.AddRow(new string[] {
                            "T",
                            "2",
                            ""});
#line 102
 testRunner.Given("I have a file named \'SafarcomTopup2.txt\' with the following contents", ((string)(null)), table47, "Given ");
#line hidden
                TechTalk.SpecFlow.Table table48 = new TechTalk.SpecFlow.Table(new string[] {
                            "EstateName",
                            "MerchantName",
                            "FileProfileId",
                            "UserId"});
                table48.AddRow(new string[] {
                            "Test Estate 1",
                            "Test Merchant 1",
                            "B2A59ABF-293D-4A6B-B81B-7007503C3476",
                            "ABA59ABF-293D-4A6B-B81B-7007503C3476"});
#line 108
 testRunner.And("I upload this file for processing", ((string)(null)), table48, "And ");
#line hidden
#line 112
 testRunner.When("As merchant \"Test Merchant 1\" on Estate \"Test Estate 1\" I get my transactions 3 t" +
                        "ransaction should be returned", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Process Duplicate Safaricom Topup File with 1 detail row")]
        [Xunit.TraitAttribute("FeatureTitle", "Process Topup CSV Files")]
        [Xunit.TraitAttribute("Description", "Process Duplicate Safaricom Topup File with 1 detail row")]
        [Xunit.TraitAttribute("Category", "PRTest")]
        public void ProcessDuplicateSafaricomTopupFileWith1DetailRow()
        {
            string[] tagsOfScenario = new string[] {
                    "PRTest"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Process Duplicate Safaricom Topup File with 1 detail row", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 115
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 4
this.FeatureBackground();
#line hidden
                TechTalk.SpecFlow.Table table49 = new TechTalk.SpecFlow.Table(new string[] {
                            "Column1",
                            "Column2",
                            "Column3"});
                table49.AddRow(new string[] {
                            "H",
                            "20210508",
                            ""});
                table49.AddRow(new string[] {
                            "D",
                            "07777777775",
                            "100"});
                table49.AddRow(new string[] {
                            "D",
                            "07777777776",
                            "200"});
                table49.AddRow(new string[] {
                            "T",
                            "1",
                            ""});
#line 116
 testRunner.Given("I have a file named \'SafarcomTopup1.txt\' with the following contents", ((string)(null)), table49, "Given ");
#line hidden
                TechTalk.SpecFlow.Table table50 = new TechTalk.SpecFlow.Table(new string[] {
                            "EstateName",
                            "MerchantName",
                            "FileProfileId",
                            "UserId"});
                table50.AddRow(new string[] {
                            "Test Estate 1",
                            "Test Merchant 1",
                            "B2A59ABF-293D-4A6B-B81B-7007503C3476",
                            "ABA59ABF-293D-4A6B-B81B-7007503C3476"});
#line 122
 testRunner.And("I upload this file for processing", ((string)(null)), table50, "And ");
#line hidden
#line 126
 testRunner.When("As merchant \"Test Merchant 1\" on Estate \"Test Estate 1\" I get my transactions 2 t" +
                        "ransaction should be returned", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
                TechTalk.SpecFlow.Table table51 = new TechTalk.SpecFlow.Table(new string[] {
                            "Column1",
                            "Column2",
                            "Column3"});
                table51.AddRow(new string[] {
                            "H",
                            "20210508",
                            ""});
                table51.AddRow(new string[] {
                            "D",
                            "07777777775",
                            "100"});
                table51.AddRow(new string[] {
                            "D",
                            "07777777776",
                            "200"});
                table51.AddRow(new string[] {
                            "T",
                            "1",
                            ""});
#line 128
 testRunner.Given("I have a file named \'SafarcomTopup2.txt\' with the following contents", ((string)(null)), table51, "Given ");
#line hidden
                TechTalk.SpecFlow.Table table52 = new TechTalk.SpecFlow.Table(new string[] {
                            "EstateName",
                            "MerchantName",
                            "FileProfileId",
                            "UserId"});
                table52.AddRow(new string[] {
                            "Test Estate 1",
                            "Test Merchant 1",
                            "B2A59ABF-293D-4A6B-B81B-7007503C3476",
                            "ABA59ABF-293D-4A6B-B81B-7007503C3476"});
#line 134
 testRunner.And("I upload this file for processing an error should be returned indicating the file" +
                        " is a duplicate", ((string)(null)), table52, "And ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Process Safaricom Topup File with Upload Date Time")]
        [Xunit.TraitAttribute("FeatureTitle", "Process Topup CSV Files")]
        [Xunit.TraitAttribute("Description", "Process Safaricom Topup File with Upload Date Time")]
        public void ProcessSafaricomTopupFileWithUploadDateTime()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Process Safaricom Topup File with Upload Date Time", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 140
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 4
this.FeatureBackground();
#line hidden
                TechTalk.SpecFlow.Table table53 = new TechTalk.SpecFlow.Table(new string[] {
                            "Column1",
                            "Column2",
                            "Column3"});
                table53.AddRow(new string[] {
                            "H",
                            "20210508",
                            ""});
                table53.AddRow(new string[] {
                            "D",
                            "07777777775",
                            "100"});
                table53.AddRow(new string[] {
                            "T",
                            "1",
                            ""});
#line 141
 testRunner.Given("I have a file named \'SafarcomTopup.txt\' with the following contents", ((string)(null)), table53, "Given ");
#line hidden
                TechTalk.SpecFlow.Table table54 = new TechTalk.SpecFlow.Table(new string[] {
                            "EstateName",
                            "MerchantName",
                            "FileProfileId",
                            "UserId",
                            "UploadDateTime"});
                table54.AddRow(new string[] {
                            "Test Estate 1",
                            "Test Merchant 1",
                            "B2A59ABF-293D-4A6B-B81B-7007503C3476",
                            "ABA59ABF-293D-4A6B-B81B-7007503C3476",
                            "Today"});
#line 146
 testRunner.And("I upload this file for processing", ((string)(null)), table54, "And ");
#line hidden
#line 150
 testRunner.When("I get the import log for estate \'Test Estate 1\' the date on the import log is \'To" +
                        "day\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
                TechTalk.SpecFlow.Table table55 = new TechTalk.SpecFlow.Table(new string[] {
                            "Column1",
                            "Column2",
                            "Column3"});
                table55.AddRow(new string[] {
                            "H",
                            "20210508",
                            ""});
                table55.AddRow(new string[] {
                            "D",
                            "07777777775",
                            "200"});
                table55.AddRow(new string[] {
                            "T",
                            "1",
                            ""});
#line 152
 testRunner.Given("I have a file named \'SafarcomTopup1.txt\' with the following contents", ((string)(null)), table55, "Given ");
#line hidden
                TechTalk.SpecFlow.Table table56 = new TechTalk.SpecFlow.Table(new string[] {
                            "EstateName",
                            "MerchantName",
                            "FileProfileId",
                            "UserId",
                            "UploadDateTime"});
                table56.AddRow(new string[] {
                            "Test Estate 1",
                            "Test Merchant 1",
                            "B2A59ABF-293D-4A6B-B81B-7007503C3476",
                            "ABA59ABF-293D-4A6B-B81B-7007503C3476",
                            "01/09/2021"});
#line 157
 testRunner.And("I upload this file for processing", ((string)(null)), table56, "And ");
#line hidden
#line 161
 testRunner.When("I get the import log for estate \'Test Estate 1\' the date on the import log is \'01" +
                        "/09/2021\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "3.9.0.0")]
        [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
        public class FixtureData : System.IDisposable
        {
            
            public FixtureData()
            {
                ProcessTopupCSVFilesFeature.FeatureSetup();
            }
            
            void System.IDisposable.Dispose()
            {
                ProcessTopupCSVFilesFeature.FeatureTearDown();
            }
        }
    }
}
#pragma warning restore
#endregion
