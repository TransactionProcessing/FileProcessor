using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FileProcessor.BusinessLogic.FileFormatHandlers;
using Shouldly;
using Xunit;

namespace FileProcessor.BusinessLogic.Tests;

public class FileFormatValidationTests
{
    [Fact]
    public void Safaricom_ValidFile_IsAccepted()
    {
        SafaricomFileFormatHandler handler = new();

        FileValidationResult result = handler.ValidateFile(new StringReader("H,20210508,\nD,07777770001,10\nT,1,"), "\n");

        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    [Theory]
    [InlineData("EmptyFile", new string[0], "EmptyFile")]
    [InlineData("MissingHeader", new[] { "D,07777770001,10", "T,1," }, "MissingHeader")]
    [InlineData("MissingTrailer", new[] { "H,20210508,", "D,07777770001,10" }, "MissingTrailer")]
    [InlineData("HeaderNotFirst", new[] { "D,07777770001,10", "H,20210508,", "T,1," }, "HeaderNotFirst")]
    [InlineData("TrailerNotLast", new[] { "H,20210508,", "T,1,", "D,07777770001,10" }, "TrailerNotLast")]
    [InlineData("TrailerCountMismatch", new[] { "H,20210508,", "D,07777770001,10", "T,2," }, "TrailerDetailCountMismatch")]
    public void Safaricom_InvalidFile_ReturnsSpecificError(string scenario,
                                                           IReadOnlyList<string> lines,
                                                           string expectedCode)
    {
        _ = scenario;
        SafaricomFileFormatHandler handler = new();

        FileValidationResult result = handler.ValidateFile(new StringReader(string.Join("\n", lines)), "\n");

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(error => error.Code == expectedCode);
    }

    [Fact]
    public void Voucher_MalformedDetail_ReturnsRowValidationError()
    {
        VoucherFileFormatHandler handler = new();

        FileValidationResult result = handler.ValidateFile(new StringReader("H,20210508,\nD,Issuer,recipient\nT,1,"), "\n");

        result.IsValid.ShouldBeFalse();
        result.Errors.OfType<RowValidationError>().ShouldContain(error => error.LineNumber == 2 &&
                                                                            error.Code == "InvalidFieldCount");
    }

    [Fact]
    public void Voucher_TrailerValue_DoesNotRepresentDetailRowCount()
    {
        VoucherFileFormatHandler handler = new();

        FileValidationResult result = handler.ValidateFile(new StringReader("H,20210508,\nD,Issuer,07777777775,10\nD,Issuer,testrecipient1@recipient.com,20\nT,1,"), "\n");

        result.IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData("T")]
    [InlineData("Trailer")]
    public void Safaricom_TrailerWithoutDetailCount_IsRejected(String trailer)
    {
        SafaricomFileFormatHandler handler = new();

        FileValidationResult result = handler.ValidateFile(new StringReader($"H,20210508,\nD,07777770001,10\n{trailer}"), "\n");

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(error => error.Code == "InvalidTrailer");
    }
}
