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
using SimpleResults;

namespace FileProcessor.Handlers;

public static class FileHandlers
{
    public static async Task<IResult> UploadFileAsync(IMediator mediator,
                                                      [FromForm] UploadFileRequest request,
                                                      [FromForm] IFormCollection formCollection,
                                                      CancellationToken cancellationToken)
    {
        IFormFile file = formCollection.Files.First();

        string temporaryFileLocation = ConfigurationReader.GetValue("AppSettings", "TemporaryFileLocation");
        string fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
        string fullPath = Path.Combine(temporaryFileLocation, fileName);

        await using (FileStream stream = new(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        if (request.UploadDateTime == DateTime.MinValue)
            request.UploadDateTime = DateTime.Now;

        FileCommands.UploadFileCommand command = new(request.EstateId,
            request.MerchantId,
            request.UserId,
            fullPath,
            request.FileProfileId,
            request.UploadDateTime);

        Result<Guid> result = await mediator.Send(command, cancellationToken);

        Shared.Logger.Logger.LogDebug($"Day is {request.UploadDateTime.Day}");
        Shared.Logger.Logger.LogDebug($"Month is {request.UploadDateTime.Month}");
        Shared.Logger.Logger.LogDebug($"Year is {request.UploadDateTime.Year}");

        return ResponseFactory.FromResult(result, r => r);
    }

    public static async Task<IResult> GetFileAsync(IMediator mediator,
                                                   [FromRoute] Guid fileId,
                                                   [FromQuery] Guid estateId,
                                                   CancellationToken cancellationToken)
    {
        FileQueries.GetFileQuery query = new(fileId, estateId);
        Result<FileDetails> result = await mediator.Send(query, cancellationToken);

        return ResponseFactory.FromResult(result, ModelFactory.ConvertFrom);
    }
}