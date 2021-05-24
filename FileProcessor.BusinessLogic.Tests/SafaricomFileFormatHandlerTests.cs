using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileProcessor.BusinessLogic.Tests
{
    using System.IO;
    using FileFormatHandlers;
    using Shouldly;
    using Testing;
    using Xunit;

    public class SafaricomFileFormatHandlerTests
    {
        [Theory]
        [InlineData("H,20210430", true)]
        [InlineData("D,017245,100", false)]
        [InlineData("T,1", true)]
        public void SafaricomFileFormatHandler_FileLineCanBeIgnored_ResultIsAsExpected(String fileLine, Boolean isIgnored)
        {
            SafaricomFileFormatHandler safaricomFileFormatHandler = new SafaricomFileFormatHandler();

            safaricomFileFormatHandler.FileLineCanBeIgnored(fileLine).ShouldBe(isIgnored);
        }

        [Fact]
        public void SafaricomFileFormatHandler_ParseFileLine_LineIsParsed()
        {
            SafaricomFileFormatHandler safaricomFileFormatHandler = new SafaricomFileFormatHandler();

            var transactionMetaData = safaricomFileFormatHandler.ParseFileLine(TestData.SafaricomDetailLine);

            transactionMetaData.TryGetValue("Amount", out String amount);
            transactionMetaData.TryGetValue("CustomerAccountNumber", out String customerAccountNumber);

            amount.ShouldNotBeNullOrEmpty();
            amount.ShouldBe(TestData.SafaricomDetailLineAmount);
            customerAccountNumber.ShouldNotBeNullOrEmpty();
            customerAccountNumber.ShouldBe(TestData.SafaricomDetailLineCustomerAccountNumber);
        }

        [Theory]
        [InlineData("1,2,3")]
        [InlineData("D,1")]
        [InlineData("D,1,2,3")]
        public void SafaricomFileFormatHandler_ParseFileLine_InvalidLineData_ErrorIsThrown(String lineData)
        {
            SafaricomFileFormatHandler safaricomFileFormatHandler = new SafaricomFileFormatHandler();

            Should.Throw<InvalidDataException>(() =>
                                               {
                                                   safaricomFileFormatHandler.ParseFileLine(lineData);
                                               });
        }
    }

    public class VoucherFileFormatHandlerTests
    {
        [Theory]
        [InlineData("H,20210430", true)]
        [InlineData("D,IssuerName,07777777705 ,100", false)]
        [InlineData("D,IssuerName,1@2.com ,100", false)]
        [InlineData("T,1", true)]
        public void VoucherFileFormatHandler_FileLineCanBeIgnored_ResultIsAsExpected(String fileLine, Boolean isIgnored)
        {
            VoucherFileFormatHandler voucherFileFormatHandler = new VoucherFileFormatHandler();

            voucherFileFormatHandler.FileLineCanBeIgnored(fileLine).ShouldBe(isIgnored);
        }

        [Fact]
        public void VoucherFileFormatHandler_ParseFileLine_EmailAddress_LineIsParsed()
        {
            VoucherFileFormatHandler voucherFileFormatHandler = new VoucherFileFormatHandler();

            Dictionary<String, String> transactionMetaData = voucherFileFormatHandler.ParseFileLine(TestData.VoucherDetailLineWithEmailAddress);

            transactionMetaData.TryGetValue("OperatorName", out String operatorName);
            transactionMetaData.TryGetValue("Amount", out String amount);
            transactionMetaData.TryGetValue("RecipientEmail", out String recipientEmail);

            operatorName.ShouldNotBeNullOrEmpty();
            operatorName.ShouldBe(TestData.VoucherOperatorIdentifier);
            amount.ShouldNotBeNullOrEmpty();
            amount.ShouldBe(TestData.VoucherDetailLineAmount);
            recipientEmail.ShouldNotBeNullOrEmpty();
            recipientEmail.ShouldBe(TestData.VoucherRecipientEmail);
        }

        [Fact]
        public void VoucherFileFormatHandler_ParseFileLine_MobileNumber_LineIsParsed()
        {
            VoucherFileFormatHandler voucherFileFormatHandler = new VoucherFileFormatHandler();

            Dictionary<String, String> transactionMetaData = voucherFileFormatHandler.ParseFileLine(TestData.VoucherDetailLineWithMobileNumber);

            transactionMetaData.TryGetValue("OperatorName", out String operatorName);
            transactionMetaData.TryGetValue("Amount", out String amount);
            transactionMetaData.TryGetValue("RecipientMobile", out String recipientMobile);

            operatorName.ShouldNotBeNullOrEmpty();
            operatorName.ShouldBe(TestData.VoucherOperatorIdentifier);
            amount.ShouldNotBeNullOrEmpty();
            amount.ShouldBe(TestData.VoucherDetailLineAmount);
            recipientMobile.ShouldNotBeNullOrEmpty();
            recipientMobile.ShouldBe(TestData.VoucherRecipientMobile);
        }

        [Theory]
        [InlineData("1,2,3,4")]
        [InlineData("D,1,2")]
        [InlineData("D,1,2,3,4")]
        public void VoucherFileFormatHandler_ParseFileLine_InvalidLineData_ErrorIsThrown(String lineData)
        {
            VoucherFileFormatHandler voucherFileFormatHandler = new VoucherFileFormatHandler();

            Should.Throw<InvalidDataException>(() =>
                                               {
                                                   voucherFileFormatHandler.ParseFileLine(lineData);
                                               });
        }
    }
}
