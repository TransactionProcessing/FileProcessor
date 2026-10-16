using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using FileProcessor.Models;
using MediatR;
using SimpleResults;

namespace FileProcessor.BusinessLogic.Requests;

[ExcludeFromCodeCoverage]
public record FileQueries {
    public record GetFileQuery(Guid FileId, Guid EstateId) : IRequest<Result<FileDetails>>;

    public record GetImportLogsQuery(Guid EstateId, DateOnly StartDate, DateOnly EndDate, Guid? MerchantId, Int32 PageNumber = 1, Int32 PageSize = 50, Boolean Descending = false) : IRequest<Result<Models.FileImportLogQueryResult>>;

    public record GetImportLogQuery(Guid FileImportLogId,
                                    Guid EstateId,
                                    Guid? MerchantId) : IRequest<Result<FileProcessor.Models.FileImportLog>>;
}
