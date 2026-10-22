using System.ComponentModel;
namespace FileProcessor.DataTransferObjects.Requests;

/// <summary>Configuration values to apply when updating a file profile.</summary>
[Description("Configuration values to apply when updating a file profile.")]
public class UpdateFileProfileRequest
{
    /// <summary>Gets or sets the profile's display name.</summary>
    [Description("The profile's display name.")]
    public string Name { get; set; }

    /// <summary>Gets or sets the directory from which files for this profile are collected.</summary>
    [Description("The directory from which files for this profile are collected.")]
    public string ListeningDirectory { get; set; }

    /// <summary>Gets or sets the request or transaction type associated with files using this profile.</summary>
    [Description("The request or transaction type associated with files using this profile.")]
    public string RequestType { get; set; }

    /// <summary>Gets or sets the operator name recorded for the profile.</summary>
    [Description("The operator name recorded for the profile.")]
    public string OperatorName { get; set; }

    /// <summary>Gets or sets the line ending used to separate records. When omitted, the profile's current value is retained.</summary>
    [Description("The line ending used to separate records. When omitted, the profile's current value is retained.")]
    public LineTerminatorType? LineTerminator { get; set; }

    /// <summary>Gets or sets the registered file format handler used to parse files for this profile.</summary>
    [Description("The registered file format handler used to parse files for this profile.")]
    public string FileFormatHandler { get; set; }
}
