using FileProcessor.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FileProcessor.Endpoints;

public static class FileEndpoints
{
    public const string BaseRoute = "api/files";

    public static void MapFileEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup(BaseRoute)
            .WithTags("Files")
            .RequireAuthorization();

        group.MapPost("/", FileHandlers.UploadFileAsync)
            .DisableAntiforgery()
            .Accepts<IFormFile>("multipart/form-data")
            .WithName("UploadFile");

        group.MapGet("/{fileId:guid}", FileHandlers.GetFileAsync)
            .WithName("GetFile");
    }
}