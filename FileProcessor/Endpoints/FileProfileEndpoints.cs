using System;
using FileProcessor.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FileProcessor.Endpoints;

public static class FileProfileEndpoints
{
    public static readonly String BaseRoute = "api/file-profiles";

    public static void MapFileProfileEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup(BaseRoute)
            .WithTags("File Profiles")
            .RequireAuthorization();

        group.MapGet("/", FileProfileHandlers.GetFileProfiles)
            .WithName("GetFileProfiles");

        group.MapGet("/{fileProfileId:guid}", FileProfileHandlers.GetFileProfile)
            .WithName("GetFileProfile");

        group.MapPost("/", FileProfileHandlers.CreateFileProfile)
            .WithName("CreateFileProfile");

        group.MapPatch("/{fileProfileId:guid}", FileProfileHandlers.UpdateFileProfile)
            .WithName("UpdateFileProfile");

        group.MapDelete("/{fileProfileId:guid}", FileProfileHandlers.ArchiveFileProfile)
            .WithName("ArchiveFileProfile");
    }
}
