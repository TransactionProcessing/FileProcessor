using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FileProcessor.BusinessLogic.Requests;
using FileProcessor.Common;
using FileProcessor.DataTransferObjects;
using FileProcessor.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.General;
using Shared.Results.Web;
using Shared.Serialisation;
using SimpleResults;

namespace FileProcessor.Handlers;

public static class FileHandlers
{
    public static async Task<IResult> UploadFile(IMediator mediator,
                                                 [FromForm, Description("Multipart form metadata and the file to upload.")] UploadFileForm request,
                                                 HttpRequest httpRequest,
                                                 CancellationToken cancellationToken)
    {
        IFormCollection formCollection = await httpRequest.ReadFormAsync(cancellationToken);
        if (TryValidateUpload(formCollection, out ValidatedUpload upload, out IResult validationError) == false)
            return validationError;

        string temporaryFileLocation = ConfigurationReader.GetValue("AppSettings", "TemporaryFileLocation");
        string temporaryFileName = Guid.NewGuid().ToString("N");
        string fullPath = Path.Combine(temporaryFileLocation, temporaryFileName);
        bool uploadAccepted = false;
        bool temporaryFileCreated = false;

        try
        {
            Directory.CreateDirectory(temporaryFileLocation);

            await using (FileStream stream = new(fullPath, FileMode.CreateNew))
            {
                temporaryFileCreated = true;
                await upload.File.CopyToAsync(stream, cancellationToken);
            }

            if (request.UploadDateTime == DateTime.MinValue)
                request.UploadDateTime = DateTime.Now;

            FileCommands.UploadFileCommand command = new(request.FileId,
                request.EstateId,
                request.MerchantId,
                request.UserId,
                fullPath,
                request.FileProfileId,
                request.UploadDateTime,
                upload.FileName);

            Result<Guid> result = await mediator.Send(command, cancellationToken);
            uploadAccepted = result.IsSuccess;

            Shared.Logger.Logger.LogDebug($"Day is {request.UploadDateTime.Day}");
            Shared.Logger.Logger.LogDebug($"Month is {request.UploadDateTime.Month}");
            Shared.Logger.Logger.LogDebug($"Year is {request.UploadDateTime.Year}");

            if (result.IsSuccess)
                return Results.Accepted(value: request.FileId);

            return ResponseFactory.FromResult(result, r => r);
        }
        finally
        {
            if (uploadAccepted == false && temporaryFileCreated)
            {
                try
                {
                    if (System.IO.File.Exists(fullPath))
                        System.IO.File.Delete(fullPath);
                }
                catch (Exception exception)
                {
                    Shared.Logger.Logger.LogWarning($"Failed to remove temporary upload file [{fullPath}]. Error [{exception.Message}]");
                }
            }
        }
    }

    private static Boolean TryValidateUpload(IFormCollection formCollection,
                                             out ValidatedUpload upload,
                                             out IResult validationError)
    {
        upload = null;
        validationError = null;
        if (formCollection?.Files == null || formCollection.Files.Count != 1)
            return SetValidationError(Results.BadRequest("Exactly one file must be supplied."), out validationError);

        IFormFile file = formCollection.Files[0];
        if (file.Length <= 0)
            return SetValidationError(Results.BadRequest("The uploaded file must not be empty."), out validationError);

        if (file.Length > UploadFileSettings.GetMaximumFileSizeBytes())
            return SetValidationError(Results.StatusCode(StatusCodes.Status413PayloadTooLarge), out validationError);

        String fileName = SanitizeFileName(file.FileName);
        if (String.IsNullOrWhiteSpace(fileName))
            return SetValidationError(Results.BadRequest("The uploaded file must have a valid filename."), out validationError);

        String fileExtension = Path.GetExtension(fileName);
        if (UploadFileSettings.GetAllowedFileExtensions().Contains(fileExtension, StringComparer.OrdinalIgnoreCase) == false)
            return SetValidationError(Results.BadRequest($"Files with extension [{fileExtension}] are not supported."), out validationError);

        upload = new ValidatedUpload(file, fileName);
        return true;
    }

    private static Boolean SetValidationError(IResult error, out IResult validationError)
    {
        validationError = error;
        return false;
    }

    private sealed record ValidatedUpload(IFormFile File, String FileName);

    private static String SanitizeFileName(String fileName)
    {
        if (String.IsNullOrWhiteSpace(fileName))
            return String.Empty;

        String name = Path.GetFileName(fileName.Replace('\\', '/'));
        Char[] invalidCharacters = Path.GetInvalidFileNameChars()
            .Concat(new[] { '<', '>', ':', '"', '|', '?', '*' })
            .ToArray();
        String sanitizedName = new(name.Where(character => Char.IsControl(character) == false && invalidCharacters.Contains(character) == false).ToArray());
        sanitizedName = sanitizedName.Trim().TrimEnd('.');

        if (sanitizedName.Length <= 255)
            return sanitizedName;

        String extension = Path.GetExtension(sanitizedName);
        Int32 nameLength = Math.Max(1, 255 - extension.Length);
        return sanitizedName[..nameLength] + extension;
    }

    public static async Task<IResult> GetFile(IMediator mediator,
                                                   [FromRoute, Description("GUID identifying the uploaded file.")] Guid fileId,
                                                   [FromQuery, Description("GUID identifying the estate that owns the file.")] Guid estateId,
                                                   CancellationToken cancellationToken)
    {
        FileQueries.GetFileQuery query = new(fileId, estateId);
        Result<FileDetails> result = await mediator.Send(query, cancellationToken);

        return ResponseFactory.FromResult(result, ModelFactory.ConvertFrom);
    }

    public static async Task<IResult> ReplayLine(IMediator mediator,
                                                   [FromRoute, Description("GUID identifying the uploaded file.")] Guid fileId,
                                                   [FromQuery, Description("GUID identifying the estate that owns the file.")] Guid estateId,
                                                   [FromRoute, Description("One-based line number to retry.")] Int32 lineNumber,
                                                   CancellationToken cancellationToken)
    {
        FileCommands.ReplayFileLineCommand command = new(fileId, estateId, lineNumber);
        Result result = await mediator.Send(command, cancellationToken);

        return ResponseFactory.FromResult(result);
    }
}
