namespace FileProcessor.File.DomainEvents
{
    using System;using System.Xml;
    using Shared.DomainDrivenDesign.EventSourcing;

    public record FileLineAddedEvent(Guid FileId, Guid EstateId, Guid MerchantId, Int32 LineNumber, String FileLine) : DomainEvent(FileId, Guid.NewGuid());
    
    public record FileProcessingCompletedEvent(Guid FileId, Guid EstateId, Guid MerchantId, DateTime ProcessingCompletedDateTime) : DomainEvent(FileId, Guid.NewGuid());

    public record FileLineProcessingFailedEvent(Guid FileId, Guid EstateId, Guid MerchantId, Int32 LineNumber, Guid TransactionId, String ResponseCode, String ResponseMessage) : DomainEvent(FileId, Guid.NewGuid());

    public record FileLineProcessingRejectedEvent(Guid FileId, Guid EstateId, Guid MerchantId, Int32 LineNumber, String Reason) : DomainEvent(FileId, Guid.NewGuid());

    public record FileLineProcessingIgnoredEvent(Guid FileId, Guid EstateId, Guid MerchantId, Int32 LineNumber) : DomainEvent(FileId, Guid.NewGuid());

    public record FileLineProcessingSuccessfulEvent(Guid FileId, Guid EstateId, Guid MerchantId, Int32 LineNumber, Guid TransactionId) : DomainEvent(FileId, Guid.NewGuid());

    public record FileCreatedEvent(Guid FileId,
                                   Guid FileImportLogId,
                                   Guid EstateId,
                                   Guid MerchantId,
                                   Guid UserId,
                                   Guid FileProfileId,
                                   String FileLocation,
                                   DateTime FileReceivedDateTime) : DomainEvent(FileId, Guid.NewGuid());
}