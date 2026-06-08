using System;

namespace FileProcessor.FileImportLog.DomainEvents
{
    using Shared.DomainDrivenDesign.EventSourcing;

    public record ImportLogCreatedEvent(Guid FileImportLogId, Guid EstateId, DateTime ImportLogDateTime) : DomainEvent(FileImportLogId, Guid.NewGuid());
    public record FileAddedToImportLogEvent(Guid FileImportLogId, Guid FileId, Guid EstateId, Guid MerchantId, Guid UserId, Guid FileProfileId, String OriginalFileName, String FilePath, DateTime FileUploadedDateTime) : DomainEvent(FileImportLogId, Guid.NewGuid());
}
