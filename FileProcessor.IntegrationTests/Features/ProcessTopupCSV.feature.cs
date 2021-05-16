﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (https://www.specflow.org/).
//      SpecFlow Version:3.7.0.0
//      SpecFlow Generator Version:3.7.0.0
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
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "3.7.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [Xunit.TraitAttribute("Category", "base")]
    [Xunit.TraitAttribute("Category", "shared")]
    [Xunit.TraitAttribute("Category", "processtopupcsv")]
    public partial class ProcessTopupCSVFilesFeature : object, Xunit.IClassFixture<ProcessTopupCSVFilesFeature.FixtureData>, System.IDisposable
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
        private string[] _featureTags = new string[] {
                "base",
                "shared",
                "processtopupcsv"};
        
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
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Features", "Process Topup CSV Files", null, ProgrammingLanguage.CSharp, new string[] {
                        "base",
                        "shared",
                        "processtopupcsv"});
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
#line 54
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
#line 59
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
#line 63
 testRunner.Given("I make the following manual merchant deposits", ((string)(null)), table13, "Given ");
#line hidden
        }
        
        void System.IDisposable.Dispose()
        {
            this.TestTearDown();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Process Safaricom Topup File with 1 detail row")]
        [Xunit.TraitAttribute("FeatureTitle", "Process Topup CSV Files")]
        [Xunit.TraitAttribute("Description", "Process Safaricom Topup File with 1 detail row")]
        public virtual void ProcessSafaricomTopupFileWith1DetailRow()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Process Safaricom Topup File with 1 detail row", null, tagsOfScenario, argumentsOfScenario, this._featureTags);
#line 67
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
#line 68
 testRunner.Given("I have a safaricom topup file with the following contents", ((string)(null)), table14, "Given ");
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
#line 73
 testRunner.And("I upload this file for processing", ((string)(null)), table15, "And ");
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
        [Xunit.TraitAttribute("Category", "PRTest")]
        public virtual void ProcessSafaricomTopupFileWith2DetailRows()
        {
            string[] tagsOfScenario = new string[] {
                    "PRTest"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Process Safaricom Topup File with 2 detail rows", null, tagsOfScenario, argumentsOfScenario, this._featureTags);
#line 80
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
                            "07777777775",
                            "100"});
                table16.AddRow(new string[] {
                            "D",
                            "07777777776",
                            "200"});
                table16.AddRow(new string[] {
                            "T",
                            "2",
                            ""});
#line 81
testRunner.Given("I have a safaricom topup file with the following contents", ((string)(null)), table16, "Given ");
#line hidden
                TechTalk.SpecFlow.Table table17 = new TechTalk.SpecFlow.Table(new string[] {
                            "EstateName",
                            "MerchantName",
                            "FileProfileId",
                            "UserId"});
                table17.AddRow(new string[] {
                            "Test Estate 1",
                            "Test Merchant 1",
                            "B2A59ABF-293D-4A6B-B81B-7007503C3476",
                            "ABA59ABF-293D-4A6B-B81B-7007503C3476"});
#line 87
 testRunner.And("I upload this file for processing", ((string)(null)), table17, "And ");
#line hidden
#line 91
 testRunner.When("As merchant \"Test Merchant 1\" on Estate \"Test Estate 1\" I get my transactions 2 t" +
                        "ransaction should be returned", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Process 2 Safaricom Topup Files")]
        [Xunit.TraitAttribute("FeatureTitle", "Process Topup CSV Files")]
        [Xunit.TraitAttribute("Description", "Process 2 Safaricom Topup Files")]
        public virtual void Process2SafaricomTopupFiles()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Process 2 Safaricom Topup Files", null, tagsOfScenario, argumentsOfScenario, this._featureTags);
#line 93
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
                TechTalk.SpecFlow.Table table18 = new TechTalk.SpecFlow.Table(new string[] {
                            "Column1",
                            "Column2",
                            "Column3"});
                table18.AddRow(new string[] {
                            "H",
                            "20210508",
                            ""});
                table18.AddRow(new string[] {
                            "D",
                            "07777777775",
                            "100"});
                table18.AddRow(new string[] {
                            "T",
                            "1",
                            ""});
#line 94
 testRunner.Given("I have a safaricom topup file with the following contents", ((string)(null)), table18, "Given ");
#line hidden
                TechTalk.SpecFlow.Table table19 = new TechTalk.SpecFlow.Table(new string[] {
                            "EstateName",
                            "MerchantName",
                            "FileProfileId",
                            "UserId"});
                table19.AddRow(new string[] {
                            "Test Estate 1",
                            "Test Merchant 1",
                            "B2A59ABF-293D-4A6B-B81B-7007503C3476",
                            "ABA59ABF-293D-4A6B-B81B-7007503C3476"});
#line 99
 testRunner.And("I upload this file for processing", ((string)(null)), table19, "And ");
#line hidden
                TechTalk.SpecFlow.Table table20 = new TechTalk.SpecFlow.Table(new string[] {
                            "Column1",
                            "Column2",
                            "Column3"});
                table20.AddRow(new string[] {
                            "H",
                            "20210508",
                            ""});
                table20.AddRow(new string[] {
                            "D",
                            "07777777776",
                            "150"});
                table20.AddRow(new string[] {
                            "D",
                            "07777777777",
                            "50"});
                table20.AddRow(new string[] {
                            "T",
                            "2",
                            ""});
#line 103
 testRunner.Given("I have a safaricom topup file with the following contents", ((string)(null)), table20, "Given ");
#line hidden
                TechTalk.SpecFlow.Table table21 = new TechTalk.SpecFlow.Table(new string[] {
                            "EstateName",
                            "MerchantName",
                            "FileProfileId",
                            "UserId"});
                table21.AddRow(new string[] {
                            "Test Estate 1",
                            "Test Merchant 1",
                            "B2A59ABF-293D-4A6B-B81B-7007503C3476",
                            "ABA59ABF-293D-4A6B-B81B-7007503C3476"});
#line 109
 testRunner.And("I upload this file for processing", ((string)(null)), table21, "And ");
#line hidden
#line 113
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
        public virtual void ProcessDuplicateSafaricomTopupFileWith1DetailRow()
        {
            string[] tagsOfScenario = new string[] {
                    "PRTest"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Process Duplicate Safaricom Topup File with 1 detail row", null, tagsOfScenario, argumentsOfScenario, this._featureTags);
#line 116
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
                TechTalk.SpecFlow.Table table22 = new TechTalk.SpecFlow.Table(new string[] {
                            "Column1",
                            "Column2",
                            "Column3"});
                table22.AddRow(new string[] {
                            "H",
                            "20210508",
                            ""});
                table22.AddRow(new string[] {
                            "D",
                            "07777777775",
                            "100"});
                table22.AddRow(new string[] {
                            "T",
                            "1",
                            ""});
#line 117
 testRunner.Given("I have a safaricom topup file with the following contents", ((string)(null)), table22, "Given ");
#line hidden
                TechTalk.SpecFlow.Table table23 = new TechTalk.SpecFlow.Table(new string[] {
                            "EstateName",
                            "MerchantName",
                            "FileProfileId",
                            "UserId"});
                table23.AddRow(new string[] {
                            "Test Estate 1",
                            "Test Merchant 1",
                            "B2A59ABF-293D-4A6B-B81B-7007503C3476",
                            "ABA59ABF-293D-4A6B-B81B-7007503C3476"});
#line 122
 testRunner.And("I upload this file for processing", ((string)(null)), table23, "And ");
#line hidden
#line 126
 testRunner.When("As merchant \"Test Merchant 1\" on Estate \"Test Estate 1\" I get my transactions 1 t" +
                        "ransaction should be returned", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
                TechTalk.SpecFlow.Table table24 = new TechTalk.SpecFlow.Table(new string[] {
                            "Column1",
                            "Column2",
                            "Column3"});
                table24.AddRow(new string[] {
                            "H",
                            "20210508",
                            ""});
                table24.AddRow(new string[] {
                            "D",
                            "07777777775",
                            "100"});
                table24.AddRow(new string[] {
                            "T",
                            "1",
                            ""});
#line 128
 testRunner.Given("I have a safaricom topup file with the following contents", ((string)(null)), table24, "Given ");
#line hidden
                TechTalk.SpecFlow.Table table25 = new TechTalk.SpecFlow.Table(new string[] {
                            "EstateName",
                            "MerchantName",
                            "FileProfileId",
                            "UserId"});
                table25.AddRow(new string[] {
                            "Test Estate 1",
                            "Test Merchant 1",
                            "B2A59ABF-293D-4A6B-B81B-7007503C3476",
                            "ABA59ABF-293D-4A6B-B81B-7007503C3476"});
#line 133
 testRunner.And("I upload this file for processing an error should be returned indicating the file" +
                        " is a duplicate", ((string)(null)), table25, "And ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "3.7.0.0")]
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
