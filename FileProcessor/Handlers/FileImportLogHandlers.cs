using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using FileProcessor.BusinessLogic.Requests;
using FileProcessor.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TransactionProcessing.UnionResults;

namespace FileProcessor.Handlers;

public static class FileImportLogHandlers
{
    public static async Task<Result<object?>> GetImportLogsAsync(IMediator mediator,
                                                                [AsParameters] ImportLogsQueryParameters parameters,
                                                                CancellationToken cancellationToken)
    {
        int pageNumber = parameters.PageNumber == 0 ? 1 : parameters.PageNumber;
        int pageSize = parameters.PageSize == 0 ? 50 : parameters.PageSize;
        FileQueries.GetImportLogsQuery query = new(parameters.EstateId, parameters.StartDate, parameters.EndDate,
                                                  parameters.MerchantId, pageNumber, pageSize, parameters.Descending);
        Result<Models.FileImportLogQueryResult> result = await mediator.Send(query, cancellationToken);

        return ResponseFactory.FromResult(result, ModelFactory.ConvertFrom);
    }

    public static async Task<Result<object?>> GetImportLogAsync(IMediator mediator,
                                                        [FromRoute, Description("GUID identifying the file import log.")] Guid fileImportLogId,
                                                        [FromQuery, Description("Required estate GUID to scope the logs or log lookup.")] Guid estateId,
                                                        [FromQuery, Description("Optional merchant GUID used to filter logs or files.")] Guid? merchantId,
                                                        CancellationToken cancellationToken)
    {
        FileQueries.GetImportLogQuery query = new(fileImportLogId, estateId, merchantId);
        Result<Models.FileImportLog> result = await mediator.Send(query, cancellationToken);

        return ResponseFactory.FromResult(result, ModelFactory.ConvertFrom);
    }

    public sealed class ImportLogsQueryParameters
    {
        [FromQuery(Name = "estateId")]
        [Description("Required estate GUID to scope the logs or log lookup.")]
        public Guid EstateId { get; init; }

        [FromQuery(Name = "startDate")]
        [Description("Required inclusive start date in ISO 8601 date format (yyyy-MM-dd). Must be on or before endDate.")]
        public DateOnly StartDate { get; init; }

        [FromQuery(Name = "endDate")]
        [Description("Required inclusive end date in ISO 8601 date format (yyyy-MM-dd). Must be on or after startDate.")]
        public DateOnly EndDate { get; init; }

        [FromQuery(Name = "merchantId")]
        [Description("Optional merchant GUID used to filter logs or files.")]
        public Guid? MerchantId { get; init; }

        [FromQuery(Name = "pageNumber")]
        [Description("One-based page number. Defaults to 1 when omitted or zero.")]
        public int PageNumber { get; init; }

        [FromQuery(Name = "pageSize")]
        [Description("Number of logs per page (1–200). Defaults to 50 when omitted or zero.")]
        public int PageSize { get; init; }

        [FromQuery(Name = "descending")]
        [Description("Sort newest first when true; defaults to false.")]
        public bool Descending { get; init; }
    }
}
