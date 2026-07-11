using System;
using Shared.DomainDrivenDesign.EventSourcing;

namespace FileProcessor.FileProfile.DomainEvents;

public record FileProfileCreatedEvent(Guid FileProfileCollectionId,
                                     Guid FileProfileId,
                                     String Name,
                                     String ListeningDirectory,
                                     String RequestType,
                                     String OperatorName,
                                     String LineTerminator,
                                     String FileFormatHandler) : DomainEvent(FileProfileCollectionId, Guid.NewGuid());

public record FileProfileNameUpdatedEvent(Guid FileProfileCollectionId, Guid FileProfileId, String Name) : DomainEvent(FileProfileCollectionId, Guid.NewGuid());

public record FileProfileListeningDirectoryUpdatedEvent(Guid FileProfileCollectionId, Guid FileProfileId, String ListeningDirectory) : DomainEvent(FileProfileCollectionId, Guid.NewGuid());

public record FileProfileRequestTypeUpdatedEvent(Guid FileProfileCollectionId, Guid FileProfileId, String RequestType) : DomainEvent(FileProfileCollectionId, Guid.NewGuid());

public record FileProfileOperatorNameUpdatedEvent(Guid FileProfileCollectionId, Guid FileProfileId, String OperatorName) : DomainEvent(FileProfileCollectionId, Guid.NewGuid());

public record FileProfileLineTerminatorUpdatedEvent(Guid FileProfileCollectionId, Guid FileProfileId, String LineTerminator) : DomainEvent(FileProfileCollectionId, Guid.NewGuid());

public record FileProfileFileFormatHandlerUpdatedEvent(Guid FileProfileCollectionId, Guid FileProfileId, String FileFormatHandler) : DomainEvent(FileProfileCollectionId, Guid.NewGuid());

public record FileProfileArchivedEvent(Guid FileProfileCollectionId, Guid FileProfileId) : DomainEvent(FileProfileCollectionId, Guid.NewGuid());
