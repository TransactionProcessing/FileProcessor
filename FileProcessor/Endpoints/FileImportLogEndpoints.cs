using System;
using FileProcessor.Handlers;
using FileProcessor.DataTransferObjects.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace FileProcessor.Endpoints;

public static class FileImportLogEndpoints
{
    public static readonly String BaseRoute = "api/fileImportLogs";

    public static void MapFileImportLogEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup(BaseRoute)
            .WithTags("File Import Logs")
            .RequireAuthorization();

        group.MapGet("/", FileImportLogHandlers.GetImportLogsAsync)
            .WithName("GetImportLogs")
            .WithSummary("List file import logs")
            .WithDescription("Requires authorization. Query parameters: `estateId` (required), `startDate` and `endDate` (required ISO dates; end date must be on or after start date), `merchantId` (optional), `pageNumber` (defaults to 1), `pageSize` (defaults to 50; allowed range 1–200), and `descending` (defaults to false). The date range includes both calendar dates. Returns a page of import logs and pagination metadata.")
            .Produces<FileImportLogList>(StatusCodes.Status200OK)
            .WithEndpointExamples();

        group.MapGet("/{fileImportLogId:guid}", FileImportLogHandlers.GetImportLogAsync)
            .WithName("GetImportLog")
            .WithSummary("Get a file import log")
            .WithDescription("Requires authorization. Required GUID path parameter `fileImportLogId` identifies the log; required GUID query parameter `estateId` scopes the lookup, and optional GUID query parameter `merchantId` filters its files. Returns the import log and its files.")
            .Produces<global::FileProcessor.DataTransferObjects.Responses.FileImportLog>(StatusCodes.Status200OK)
            .WithEndpointExamples();
    }
}
