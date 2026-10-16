using System;

namespace FileProcessor.FileProfileAggregate;

public static class FileProfileConfiguration
{
    public const string SafaricomTopupRequestType = "SafaricomTopupRequest";
    public const string VoucherRequestType = "VoucherRequest";
    public const string SafaricomFileFormatHandler = "SafaricomFileFormatHandler";
    public const string VoucherFileFormatHandler = "VoucherFileFormatHandler";

    public static bool IsSupportedRequestType(string requestType)
    {
        return String.Equals(requestType, SafaricomTopupRequestType, StringComparison.Ordinal) ||
               String.Equals(requestType, VoucherRequestType, StringComparison.Ordinal);
    }

    public static bool IsSupportedFileFormatHandler(string fileFormatHandler)
    {
        return String.Equals(fileFormatHandler, SafaricomFileFormatHandler, StringComparison.Ordinal) ||
               String.Equals(fileFormatHandler, VoucherFileFormatHandler, StringComparison.Ordinal);
    }
}
