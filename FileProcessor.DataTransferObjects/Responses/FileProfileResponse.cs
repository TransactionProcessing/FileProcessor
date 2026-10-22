using System;
using System.ComponentModel;

namespace FileProcessor.DataTransferObjects.Responses;

/// <summary>File profile configuration returned by the API.</summary>
[Description("File profile configuration returned by the API.")]
public class FileProfileResponse
{
    /// <summary>Gets or sets the identifier of the profile.</summary>
    [Description("The identifier of the profile.")]
    public Guid FileProfileId { get; set; }

    /// <summary>Gets or sets the identifier of this profile version.</summary>
    [Description("The identifier of this profile version.")]
    public Guid FileProfileVersionId { get; set; }

    /// <summary>Gets or sets the version number, starting at one.</summary>
    [Description("The version number, starting at one.")]
    public Int32 FileProfileVersion { get; set; }

    /// <summary>Gets or sets the profile's display name.</summary>
    [Description("The profile's display name.")]
    public String Name { get; set; }

    /// <summary>Gets or sets the operator name.</summary>
    [Description("The operator name.")]
    public String OperatorName { get; set; }

    /// <summary>Gets or sets the directory from which files are collected.</summary>
    [Description("The directory from which files are collected.")]
    public String ListeningDirectory { get; set; }

    /// <summary>Gets or sets the directory where successfully processed files are placed.</summary>
    [Description("The directory where successfully processed files are placed.")]
    public String ProcessedDirectory { get; set; }

    /// <summary>Gets or sets the directory where failed files are placed.</summary>
    [Description("The directory where failed files are placed.")]
    public String FailedDirectory { get; set; }

    /// <summary>Gets or sets the registered file format handler.</summary>
    [Description("The registered file format handler.")]
    public String FileFormatHandler { get; set; }

    /// <summary>Gets or sets the request or transaction type.</summary>
    [Description("The request or transaction type.")]
    public String RequestType { get; set; }

    /// <summary>Gets or sets the line terminator used to separate records.</summary>
    [Description("The line terminator used to separate records.")]
    public String LineTerminator { get; set; }
}
