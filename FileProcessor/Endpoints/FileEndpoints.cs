using System;
using FileProcessor.Handlers;
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
            .Accepts<IFormFile>("multipart/form-data")
            .WithName("UploadFile");

        group.MapGet("/{fileId:guid}", FileHandlers.GetFile)
            .WithName("GetFile");

        group.MapPost("/{fileId:guid}/lines/{lineNumber:int}/retry", FileHandlers.ReplayLine)
            .WithName("ReplayLine");
    }
}
