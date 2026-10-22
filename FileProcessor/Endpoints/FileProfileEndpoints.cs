using System;
using FileProcessor.Handlers;
using System.Collections.Generic;
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
            .WithName("GetFileProfiles")
            .WithSummary("List file profiles")
            .WithDescription("Requires authorization. The request has no path parameters, query parameters, or body. Returns all available file profile versions.")
            .Produces<List<global::FileProcessor.DataTransferObjects.Responses.FileProfileResponse>>(StatusCodes.Status200OK)
            .WithEndpointExamples();

        group.MapGet("/{fileProfileId:guid}", FileProfileHandlers.GetFileProfile)
            .WithName("GetFileProfile")
            .WithSummary("Get the current file profile")
            .WithDescription("Requires authorization. Path parameter `fileProfileId` identifies the file profile. The request has no query parameters or body. Returns the current profile details.")
            .Produces<global::FileProcessor.DataTransferObjects.Responses.FileProfileResponse>(StatusCodes.Status200OK)
            .WithEndpointExamples();

        group.MapGet("/{fileProfileId:guid}/versions", FileProfileHandlers.GetFileProfileVersions)
            .WithName("GetFileProfileVersions")
            .WithSummary("List file profile versions")
            .WithDescription("Requires authorization. Path parameter `fileProfileId` identifies the file profile. The request has no query parameters or body. Returns the profile's versions.")
            .Produces<List<global::FileProcessor.DataTransferObjects.Responses.FileProfileResponse>>(StatusCodes.Status200OK)
            .WithEndpointExamples();

        group.MapGet("/{fileProfileId:guid}/versions/{fileProfileVersionId:guid}", FileProfileHandlers.GetFileProfileVersion)
            .WithName("GetFileProfileVersion")
            .WithSummary("Get a file profile version")
            .WithDescription("Requires authorization. Path parameters `fileProfileId` and `fileProfileVersionId` identify the profile and version. The request has no query parameters or body. Returns the selected version details.")
            .Produces<global::FileProcessor.DataTransferObjects.Responses.FileProfileResponse>(StatusCodes.Status200OK)
            .WithEndpointExamples();

        group.MapPost("/", FileProfileHandlers.CreateFileProfile)
            .WithName("CreateFileProfile")
            .WithSummary("Create a file profile")
            .WithDescription("Requires authorization. Supply a `CreateFileProfileRequest` JSON body containing the new profile identifier and configuration. The request has no path or query parameters. Returns the created profile.")
            .Produces<global::FileProcessor.DataTransferObjects.Responses.FileProfileResponse>(StatusCodes.Status200OK)
            .WithEndpointExamples();

        group.MapPatch("/{fileProfileId:guid}", FileProfileHandlers.UpdateFileProfile)
            .WithName("UpdateFileProfile")
            .WithSummary("Update a file profile")
            .WithDescription("Requires authorization. Path parameter `fileProfileId` identifies the profile. Supply an `UpdateFileProfileRequest` JSON body with the profile configuration to apply. The request has no query parameters. Returns the updated profile.")
            .Produces<global::FileProcessor.DataTransferObjects.Responses.FileProfileResponse>(StatusCodes.Status200OK)
            .WithEndpointExamples();

        group.MapDelete("/{fileProfileId:guid}", FileProfileHandlers.ArchiveFileProfile)
            .WithName("ArchiveFileProfile")
            .WithSummary("Archive a file profile")
            .WithDescription("Requires authorization. Path parameter `fileProfileId` identifies the profile. The request has no query parameters or body. Returns 204 No Content when the profile is archived successfully.")
            .Produces(StatusCodes.Status204NoContent)
            .WithEndpointExamples();
    }
}
