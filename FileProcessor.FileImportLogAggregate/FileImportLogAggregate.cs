using System;
using FileProcessor.Models;
using SimpleResults;

namespace FileProcessor.FileImportLogAggregate
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using FileImportLog.DomainEvents;
    using FileProcessor.Models;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EventStore.Aggregate;
    using Shared.General;

    public static class FileImportLogAggregateExtensions{
        public static Result CreateImportLog(this FileImportLogAggregate aggregate, Guid estateId, DateTime importLogDateTime)
        {
            // Silently handle a duplicate create
            if (aggregate.IsCreated)
                return Result.Success();

            ImportLogCreatedEvent importLogCreatedEvent = new ImportLogCreatedEvent(aggregate.AggregateId, estateId, importLogDateTime);

            aggregate.ApplyAndAppend(importLogCreatedEvent);
            return Result.Success();
        }
        
        public static Result AddImportedFile(this FileImportLogAggregate aggregate,
                                             Guid fileId,
                                             Guid merchantId,
                                             Guid userId,
                                             Guid fileProfileId,
                                             String originalFileName,
                                             String filePath,
                                             DateTime fileUploadedDateTime)
        {
            return aggregate.AddImportedFile(new AddImportedFileRequest(fileId,
                                                                         merchantId,
                                                                         userId,
                                                                         fileProfileId,
                                                                         originalFileName,
                                                                         filePath,
                                                                         fileUploadedDateTime));
        }

        public static Result AddImportedFile(this FileImportLogAggregate aggregate, AddImportedFileRequest request)
        {
            if (aggregate.IsCreated == false)
            {
                return Result.Invalid("Import log has not been created");
            }

            ImportLogFile existingFile = aggregate.Files.SingleOrDefault(f => f.FileId == request.FileId);
            if (existingFile != null)
            {
                if (existingFile.EstateId == aggregate.EstateId &&
                    existingFile.MerchantId == request.MerchantId &&
                    existingFile.UserId == request.UserId &&
                    existingFile.FileProfileId == request.FileProfileId)
                {
                    return Result.Success();
                }

                return Result.Invalid($"File Id [{request.FileId}] is already registered with different upload details");
            }

            FileAddedToImportLogEvent fileAddedToImportLogEvent =
                new FileAddedToImportLogEvent(aggregate.AggregateId,
                                              request.FileId,
                                              aggregate.EstateId,
                                              request.MerchantId,
                                              request.UserId,
                                              request.FileProfileId,
                                              request.OriginalFileName,
                                              request.FilePath,
                                              request.FileUploadedDateTime,
                                              request.FileProfileVersionId);

            aggregate.ApplyAndAppend(fileAddedToImportLogEvent);

            return Result.Success();
        }

        public static Boolean ContainsFile(this FileImportLogAggregate aggregate, Guid fileId)
        {
            return aggregate.Files.Any(file => file.FileId == fileId);
        }

        public static void PlayEvent(this FileImportLogAggregate aggregate, ImportLogCreatedEvent domainEvent)
        {
            aggregate.IsCreated = true;
            aggregate.EstateId = domainEvent.EstateId;
            aggregate.ImportLogDateTime = domainEvent.ImportLogDateTime;
        }

        public static void PlayEvent(this FileImportLogAggregate aggregate, FileAddedToImportLogEvent domainEvent)
        {
            aggregate.Files.Add(new ImportLogFile
                                {
                                    EstateId = domainEvent.EstateId,
                                    FileId = domainEvent.FileId,
                                    FileProfileId = domainEvent.FileProfileId,
                                    FileProfileVersionId = domainEvent.FileProfileVersionId == Guid.Empty ? domainEvent.FileProfileId : domainEvent.FileProfileVersionId,
                                    MerchantId = domainEvent.MerchantId,
                                    FilePath = domainEvent.FilePath,
                                    OriginalFileName = domainEvent.OriginalFileName,
                                    UserId = domainEvent.UserId,
                                    UploadedDateTime = domainEvent.FileUploadedDateTime
                                });
        }

        public static Models.FileImportLog GetFileImportLog(this FileImportLogAggregate aggregate)
        {
            return new Models.FileImportLog
                   {
                       EstateId = aggregate.EstateId,
                       FileImportLogId = aggregate.AggregateId,
                       Files = aggregate.Files,
                       FileImportLogDateTime = aggregate.ImportLogDateTime
                   };
        }
    }

    public record AddImportedFileRequest(Guid FileId,
                                         Guid MerchantId,
                                         Guid UserId,
                                         Guid FileProfileId,
                                         String OriginalFileName,
                                         String FilePath,
                                         DateTime FileUploadedDateTime,
                                         Guid FileProfileVersionId = default);


    public record FileImportLogAggregate : Aggregate
    {
        [ExcludeFromCodeCoverage]
        protected override Object GetMetadata()
        {
            return null;
        }

        public override void PlayEvent(IDomainEvent domainEvent) => FileImportLogAggregateExtensions.PlayEvent(this, (dynamic)domainEvent);

        [ExcludeFromCodeCoverage]
        public FileImportLogAggregate()
        {
            this.Files = new List<ImportLogFile>();
        }

        private FileImportLogAggregate(Guid aggregateId)
        {
            if (aggregateId == Guid.Empty)
                throw new ArgumentException("Aggregate Id cannot be an Empty Guid");

            this.AggregateId = aggregateId;
            this.Files = new List<ImportLogFile>();
        }

        public static FileImportLogAggregate Create(Guid aggregateId)
        {
            return new FileImportLogAggregate(aggregateId);
        }
        
        public Boolean IsCreated { get; internal set; }
        
        public Guid EstateId { get; internal set; }
        
        public DateTime ImportLogDateTime { get; internal set; }
        
        internal List<ImportLogFile> Files;

        
    }
}
