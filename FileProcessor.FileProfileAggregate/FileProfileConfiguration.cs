using System;

namespace FileProcessor.FileProfileAggregate;

public static class FileProfileConfiguration
{
    public static string SafaricomTopupRequestType => "SafaricomTopupRequest";
    public static string VoucherRequestType => "VoucherRequest";
    public static string SafaricomFileFormatHandler => "SafaricomFileFormatHandler";
    public static string VoucherFileFormatHandler => "VoucherFileFormatHandler";

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
