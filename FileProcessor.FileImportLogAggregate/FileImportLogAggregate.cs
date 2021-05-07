using System;

namespace FileProcessor.FileImportLogAggregate
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using FileImportLog.DomainEvents;
    using FIleProcessor.Models;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EventStore.Aggregate;
    using Shared.General;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Shared.EventStore.Aggregate.Aggregate" />
    public class FileImportLogAggregate : Aggregate
    {
        /// <summary>
        /// Gets the metadata.
        /// </summary>
        /// <returns></returns>
        [ExcludeFromCodeCoverage]
        protected override Object GetMetadata()
        {
            return null;
        }

        /// <summary>
        /// Plays the event.
        /// </summary>
        /// <param name="domainEvent">The domain event.</param>
        public override void PlayEvent(IDomainEvent domainEvent)
        {
            this.PlayEvent((dynamic)domainEvent);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileImportLogAggregate" /> class.
        /// </summary>
        [ExcludeFromCodeCoverage]
        public FileImportLogAggregate()
        {
            this.Files = new List<ImportLogFile>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileImportLogAggregate" /> class.
        /// </summary>
        /// <param name="aggregateId">The aggregate identifier.</param>
        private FileImportLogAggregate(Guid aggregateId)
        {
            Guard.ThrowIfInvalidGuid(aggregateId, "Aggregate Id cannot be an Empty Guid");

            this.AggregateId = aggregateId;
            this.Files = new List<ImportLogFile>();
        }

        /// <summary>
        /// Creates the specified aggregate identifier.
        /// </summary>
        /// <param name="aggregateId">The aggregate identifier.</param>
        /// <returns></returns>
        public static FileImportLogAggregate Create(Guid aggregateId)
        {
            return new FileImportLogAggregate(aggregateId);
        }

        /// <summary>
        /// Creates the import log.
        /// </summary>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="importLogDateTime">The import log date time.</param>
        public void CreateImportLog(Guid estateId, DateTime importLogDateTime)
        {
            // Silently handle a duplicate create
            if (this.IsCreated)
                return;

            ImportLogCreatedEvent importLogCreatedEvent = new ImportLogCreatedEvent(this.AggregateId, estateId, importLogDateTime);

            this.ApplyAndAppend(importLogCreatedEvent);
        }

        /// <summary>
        /// Adds the imported file.
        /// </summary>
        /// <param name="fileId">The file identifier.</param>
        /// <param name="merchantId">The merchant identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="fileProfileId">The file profile identifier.</param>
        /// <param name="originalFileName">Name of the original file.</param>
        /// <param name="filePath">The file path.</param>
        public void AddImportedFile(Guid fileId, Guid merchantId,Guid userId, Guid fileProfileId, String originalFileName, String filePath)
        {
            if (this.IsCreated == false)
            {
                throw new InvalidOperationException("Import log has not been created");
            }

            if (this.Files.Any(f => f.FileId == fileId))
            {
                throw new InvalidOperationException($"Duplicate file {originalFileName} detected File Id [{fileId}]");
            }

            FileAddedToImportLogEvent fileAddedToImportLogEvent =
                new FileAddedToImportLogEvent(this.AggregateId, fileId, this.EstateId, merchantId, userId, fileProfileId, originalFileName, filePath);

            this.ApplyAndAppend(fileAddedToImportLogEvent);
        }

        /// <summary>
        /// Gets a value indicating whether this instance is created.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is created; otherwise, <c>false</c>.
        /// </value>
        public Boolean IsCreated { get; private set; }
        /// <summary>
        /// Gets the estate identifier.
        /// </summary>
        /// <value>
        /// The estate identifier.
        /// </value>
        public Guid EstateId { get; private set; }
        /// <summary>
        /// Gets the import log date time.
        /// </summary>
        /// <value>
        /// The import log date time.
        /// </value>
        public DateTime ImportLogDateTime { get; private set; }
        /// <summary>
        /// The files
        /// </summary>
        private List<ImportLogFile> Files;

        /// <summary>
        /// Plays the event.
        /// </summary>
        /// <param name="domainEvent">The domain event.</param>
        private void PlayEvent(ImportLogCreatedEvent domainEvent)
        {
            this.IsCreated = true;
            this.EstateId = domainEvent.EstateId;
            this.ImportLogDateTime = domainEvent.ImportLogDateTime;
        }

        /// <summary>
        /// Plays the event.
        /// </summary>
        /// <param name="domainEvent">The domain event.</param>
        private void PlayEvent(FileAddedToImportLogEvent domainEvent)
        {
            this.Files.Add(new ImportLogFile
                           {
                               EstateId = domainEvent.EstateId,
                               FileId = domainEvent.FileId,
                               FileProfileId = domainEvent.FileProfileId,
                               MerchantId = domainEvent.MerchantId,
                               FilePath = domainEvent.FilePath,
                               OriginalFileName = domainEvent.OriginalFileName,
                               UserId = domainEvent.UserId
                           });
        }

        /// <summary>
        /// Gets the file import log.
        /// </summary>
        /// <returns></returns>
        public FileImportLog GetFileImportLog()
        {
            return new FileImportLog
                   {
                       EstateId = this.EstateId,
                       FileImportLogId = this.AggregateId,
                       Files = this.Files,
                       FileImportLogDateTime = this.ImportLogDateTime
                   };
        }
    }
}
