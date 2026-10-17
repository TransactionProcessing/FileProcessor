using System;
using System.Globalization;
using Shared.General;

namespace FileProcessor.Common;

public static class UploadFileSettings
{
    private const Int64 DefaultMaximumFileSizeBytes = 10 * 1024 * 1024;
    private const Int64 MaximumMultipartOverheadBytes = 1024 * 1024;
    private const String DefaultAllowedFileExtensions = ".csv,.txt";

    public static Int64 GetMaximumFileSizeBytes()
    {
        String configuredValue = ConfigurationReader.GetValueOrDefault("AppSettings", "MaximumUploadFileSizeBytes", DefaultMaximumFileSizeBytes.ToString(CultureInfo.InvariantCulture));

        if (Int64.TryParse(configuredValue, NumberStyles.None, CultureInfo.InvariantCulture, out Int64 maximumFileSizeBytes) == false || maximumFileSizeBytes <= 0)
            throw new InvalidOperationException("AppSettings:MaximumUploadFileSizeBytes must be a positive integer when configured");

        return maximumFileSizeBytes;
    }

    public static Int64 GetMaximumMultipartRequestSizeBytes()
    {
        try
        {
            return checked(GetMaximumFileSizeBytes() + MaximumMultipartOverheadBytes);
        }
        catch (OverflowException exception)
        {
            throw new InvalidOperationException("AppSettings:MaximumUploadFileSizeBytes is too large", exception);
        }
    }

    public static String[] GetAllowedFileExtensions()
    {
        String value = ConfigurationReader.GetValueOrDefault("AppSettings", "AllowedUploadFileExtensions", DefaultAllowedFileExtensions);

        String[] extensions = value.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (String extension in extensions)
        {
            if (extension.StartsWith('.') == false || extension.Length < 2)
                throw new InvalidOperationException("AppSettings:AllowedUploadFileExtensions must contain comma-separated extensions beginning with a dot");
        }

        if (extensions.Length == 0)
            throw new InvalidOperationException("AppSettings:AllowedUploadFileExtensions must contain at least one extension");

        return extensions;
    }
}
