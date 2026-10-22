using System.ComponentModel;
using Microsoft.AspNetCore.Http;
using FileProcessor.DataTransferObjects;

namespace FileProcessor.Handlers;

/// <summary>Multipart form fields accepted by the file upload endpoint.</summary>
[Description("Multipart metadata and the file submitted to the upload endpoint.")]
public sealed class UploadFileForm : UploadFileRequest
{
    /// <summary>The single non-empty file to upload.</summary>
    [Description("The single non-empty file to upload.")]
    public IFormFile File { get; set; } = default!;
}
