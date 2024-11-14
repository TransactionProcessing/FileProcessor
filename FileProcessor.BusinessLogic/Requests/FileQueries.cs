using System;
using System.Collections.Generic;
using FileProcessor.Models;
using MediatR;
using SimpleResults;

namespace FileProcessor.BusinessLogic.Requests;

public record FileQueries {
    public record GetFileQuery(Guid FileId, Guid EstateId) : IRequest<Result<FileDetails>>;

    public record GetImportLogsQuery(Guid EstateId, DateTime StartDateTime, DateTime EndDateTime, Guid? MerchantId) : IRequest<Result<List<Models.FileImportLog>>>;

    public record GetImportLogQuery(Guid FileImportLogId,
                                    Guid EstateId,
                                    Guid? MerchantId) : IRequest<Result<FileProcessor.Models.FileImportLog>>;
}