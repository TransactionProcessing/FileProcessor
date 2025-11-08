using FileProcessor.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FileProcessor.Endpoints;

public static class FileImportLogEndpoints
{
    public const string BaseRoute = "api/fileImportLogs";

    public static void MapFileImportLogEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup(BaseRoute)
            .WithTags("File Import Logs")
            .RequireAuthorization();

        group.MapGet("/", FileImportLogHandlers.GetImportLogsAsync)
            .WithName("GetImportLogs");

        group.MapGet("/{fileImportLogId:guid}", FileImportLogHandlers.GetImportLogAsync)
            .WithName("GetImportLog");
    }
}