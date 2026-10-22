using System;
using FileProcessor.Handlers;
using FileProcessor.DataTransferObjects.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FileProcessor.Endpoints;

public static class FileEndpoints
{
    public static readonly String BaseRoute = "api/files";

    public static void MapFileEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup(BaseRoute)
            .WithTags("Files")
            .RequireAuthorization();

        group.MapPost("/", FileHandlers.UploadFile)
            .DisableAntiforgery()
            .Accepts<UploadFileForm>("multipart/form-data")
            .WithName("UploadFile")
            .WithSummary("Upload a transaction file")
            .WithDescription("Requires authorization. Submit the upload metadata fields from `UploadFileRequest` and exactly one non-empty file as multipart form data. The file name must have a configured allowed extension and the file must be within the configured size limit. The upload time defaults to the current local time when omitted or `DateTime.MinValue`. Returns 202 Accepted with the file ID when the upload is accepted.")
            .Produces<Guid>(StatusCodes.Status202Accepted)
            .Produces<string>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status413PayloadTooLarge)
            .WithEndpointExamples();

        group.MapGet("/{fileId:guid}", FileHandlers.GetFile)
            .WithName("GetFile")
            .WithSummary("Get a file and its processing details")
            .WithDescription("Requires authorization. Required GUID path parameter `fileId` identifies the file; required GUID query parameter `estateId` identifies the estate that owns it. Returns the file metadata, processing state, line results, and processing summary.")
            .Produces<FileDetails>(StatusCodes.Status200OK)
            .WithEndpointExamples();

        group.MapPost("/{fileId:guid}/lines/{lineNumber:int}/retry", FileHandlers.ReplayLine)
            .WithName("ReplayLine")
            .WithSummary("Retry a file line")
            .WithDescription("Requires authorization. Required GUID path parameter `fileId` and integer path parameter `lineNumber` identify the file and line to replay; required GUID query parameter `estateId` identifies the owning estate. The request has no body. Returns the operation result.")
            .Produces(StatusCodes.Status200OK)
            .WithEndpointExamples();
    }
}
