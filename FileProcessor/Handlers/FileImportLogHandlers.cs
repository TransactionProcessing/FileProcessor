using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using FileProcessor.BusinessLogic.Requests;
using FileProcessor.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Results.Web;
using SimpleResults;

namespace FileProcessor.Handlers;

public static class FileImportLogHandlers
{
    public static async Task<IResult> GetImportLogsAsync(IMediator mediator,
                                                         [FromQuery, Description("Required estate GUID to scope the logs or log lookup.")] Guid estateId,
                                                         [FromQuery, Description("Required inclusive start date in ISO 8601 date format (yyyy-MM-dd). Must be on or before endDate.")] DateOnly startDate,
                                                         [FromQuery, Description("Required inclusive end date in ISO 8601 date format (yyyy-MM-dd). Must be on or after startDate.")] DateOnly endDate,
                                                         [FromQuery, Description("Optional merchant GUID used to filter logs or files.")] Guid? merchantId,
                                                         [FromQuery, Description("One-based page number. Defaults to 1 when omitted or zero.")] Int32 pageNumber,
                                                         [FromQuery, Description("Number of logs per page (1–200). Defaults to 50 when omitted or zero.")] Int32 pageSize,
                                                         [FromQuery, Description("Sort newest first when true; defaults to false.")] Boolean descending,
                                                         CancellationToken cancellationToken)
    {
        pageNumber = pageNumber == 0 ? 1 : pageNumber;
        pageSize = pageSize == 0 ? 50 : pageSize;
        FileQueries.GetImportLogsQuery query = new(estateId, startDate, endDate, merchantId, pageNumber, pageSize, descending);
        Result<Models.FileImportLogQueryResult> result = await mediator.Send(query, cancellationToken);

        return ResponseFactory.FromResult(result, ModelFactory.ConvertFrom);
    }

    public static async Task<IResult> GetImportLogAsync(IMediator mediator,
                                                        [FromRoute, Description("GUID identifying the file import log.")] Guid fileImportLogId,
                                                        [FromQuery, Description("Required estate GUID to scope the logs or log lookup.")] Guid estateId,
                                                        [FromQuery, Description("Optional merchant GUID used to filter logs or files.")] Guid? merchantId,
                                                        CancellationToken cancellationToken)
    {
        FileQueries.GetImportLogQuery query = new(fileImportLogId, estateId, merchantId);
        Result<Models.FileImportLog> result = await mediator.Send(query, cancellationToken);

        return ResponseFactory.FromResult(result, ModelFactory.ConvertFrom);
    }
}
