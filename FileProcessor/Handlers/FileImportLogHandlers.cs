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
                                                         [FromQuery] DateTime startDateTime,
                                                         [FromQuery] DateTime endDateTime,
                                                         [FromQuery] Guid? merchantId,
                                                         CancellationToken cancellationToken)
    {
        FileQueries.GetImportLogsQuery query = new(estateId, startDateTime, endDateTime, merchantId);
        Result<List<Models.FileImportLog>> result = await mediator.Send(query, cancellationToken);

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