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
}
