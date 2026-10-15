using System;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
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
                                                      [FromForm] UploadFileRequest request,
                                                      [FromForm] IFormCollection formCollection,
                                                      CancellationToken cancellationToken)
    {
        IFormFile file = formCollection.Files.First();

        string temporaryFileLocation = ConfigurationReader.GetValue("AppSettings", "TemporaryFileLocation");
        string fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
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
                await file.CopyToAsync(stream, cancellationToken);
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
                fileName);

            Result<Guid> result = await mediator.Send(command, cancellationToken);
            uploadAccepted = result.IsSuccess;

            Shared.Logger.Logger.LogDebug($"Day is {request.UploadDateTime.Day}");
            Shared.Logger.Logger.LogDebug($"Month is {request.UploadDateTime.Month}");
            Shared.Logger.Logger.LogDebug($"Year is {request.UploadDateTime.Year}");

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

    public static async Task<IResult> GetFile(IMediator mediator,
                                                   [FromRoute] Guid fileId,
                                                   [FromQuery] Guid estateId,
                                                   CancellationToken cancellationToken)
    {
        FileQueries.GetFileQuery query = new(fileId, estateId);
        Result<FileDetails> result = await mediator.Send(query, cancellationToken);

        return ResponseFactory.FromResult(result, ModelFactory.ConvertFrom);
    }

    public static async Task<IResult> ReplayLine(IMediator mediator,
                                                   [FromRoute] Guid fileId,
                                                   [FromQuery] Guid estateId,
                                                   [FromRoute] Int32 lineNumber,
                                                   CancellationToken cancellationToken)
    {
        FileCommands.ReplayFileLineCommand command = new(fileId, estateId, lineNumber);
        Result result = await mediator.Send(command, cancellationToken);

        return ResponseFactory.FromResult(result);
    }
}
