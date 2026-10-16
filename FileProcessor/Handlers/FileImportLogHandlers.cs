using System;
using System.Collections.Generic;
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
                                                         [FromQuery] Guid estateId,
                                                         [FromQuery] DateOnly startDate,
                                                         [FromQuery] DateOnly endDate,
                                                         [FromQuery] Guid? merchantId,
                                                         [FromQuery] Int32 pageNumber,
                                                         [FromQuery] Int32 pageSize,
                                                         [FromQuery] Boolean descending,
                                                         CancellationToken cancellationToken)
    {
        pageNumber = pageNumber == 0 ? 1 : pageNumber;
        pageSize = pageSize == 0 ? 50 : pageSize;
        FileQueries.GetImportLogsQuery query = new(estateId, startDate, endDate, merchantId, pageNumber, pageSize, descending);
        Result<Models.FileImportLogQueryResult> result = await mediator.Send(query, cancellationToken);

        return ResponseFactory.FromResult(result, ModelFactory.ConvertFrom);
    }

    public static async Task<IResult> GetImportLogAsync(IMediator mediator,
                                                        [FromRoute] Guid fileImportLogId,
                                                        [FromQuery] Guid estateId,
                                                        [FromQuery] Guid? merchantId,
                                                        CancellationToken cancellationToken)
    {
        FileQueries.GetImportLogQuery query = new(fileImportLogId, estateId, merchantId);
        Result<Models.FileImportLog> result = await mediator.Send(query, cancellationToken);

        return ResponseFactory.FromResult(result, ModelFactory.ConvertFrom);
    }
}
