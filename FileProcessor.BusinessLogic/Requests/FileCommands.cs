using System;
using System.Diagnostics.CodeAnalysis;
using MediatR;
using SimpleResults;

namespace FileProcessor.BusinessLogic.Requests;

[ExcludeFromCodeCoverage]
public record FileCommands {
    public record ProcessTransactionForFileLineCommand(Guid FileId, Int32 LineNumber, String FileLine)
        : IRequest<Result>;

    public record ProcessUploadedFileCommand(Guid EstateId,
                                             Guid MerchantId,
                                             Guid FileImportLogId,
                                             Guid FileId,
                                             Guid UserId,
                                             String FilePath,
                                             Guid FileProfileId,
                                             DateTime FileUploadedDateTime) : IRequest<Result>;

    public record UploadFileCommand(Guid FileId,
                                    Guid EstateId,
                                    Guid MerchantId,
                                    Guid UserId,
                                    String FilePath,
                                    Guid FileProfileId,
                                    DateTime FileUploadedDateTime) : IRequest<Result>;
}