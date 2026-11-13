using System;
using System.Diagnostics.CodeAnalysis;
using MediatR;
using TransactionProcessing.UnionResults;

namespace FileProcessor.BusinessLogic.Requests;

[ExcludeFromCodeCoverage]
public record FileCommands {
    public record ProcessTransactionForFileLineCommand(Guid FileId, Guid EventId, Int32 LineNumber, String FileLine)
        : IRequest<Result>;

    public record ProcessUploadedFileCommand(Guid EstateId,
                                             Guid MerchantId,
                                             Guid FileImportLogId,
                                             Guid FileId,
                                             Guid UserId,
                                             String FilePath,
                                             Guid FileProfileId,
                                             DateTimeOffset FileUploadedDateTime,
                                             Guid FileProfileVersionId = default) : IRequest<Result>;

    public record UploadFileCommand(Guid FileId,
                                    Guid EstateId,
                                    Guid MerchantId,
                                    Guid UserId,
                                    String FilePath,
                                    Guid FileProfileId,
                                    DateTimeOffset FileUploadedDateTime,
                                    String OriginalFileName = null) : IRequest<Result>;

    public record ReplayFileLineCommand(Guid FileId,
                                    Guid EstateId,
                                    Int32 LineNumber) : IRequest<Result>;
}
