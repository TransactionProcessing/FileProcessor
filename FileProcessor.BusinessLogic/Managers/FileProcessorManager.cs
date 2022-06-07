namespace FileProcessor.BusinessLogic.Managers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using System.Threading.Tasks;
    using Common;
    using EstateReporting.Database;
    using EstateReporting.Database.Entities;
    using EstateReporting.Database.ViewEntities;
    using FileAggregate;
    using FIleProcessor.Models;
    using Microsoft.EntityFrameworkCore;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EventStore.Aggregate;
    using Shared.Exceptions;
    using FileImportLog = FIleProcessor.Models.FileImportLog;
    using FileLine = EstateReporting.Database.Entities.FileLine;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="FileProcessor.BusinessLogic.Managers.IFileProcessorManager" />
    public class FileProcessorManager : IFileProcessorManager
    {
        #region Fields

        /// <summary>
        /// The file profiles
        /// </summary>
        private readonly List<FileProfile> FileProfiles;

        private readonly Shared.EntityFramework.IDbContextFactory<EstateReportingGenericContext> DbContextFactory;

        private readonly IModelFactory ModelFactory;

        private readonly IAggregateRepository<FileAggregate, DomainEvent> FileAggregateRepository;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FileProcessorManager" /> class.
        /// </summary>
        /// <param name="fileProfiles">The file profiles.</param>
        /// <param name="dbContextFactory">The database context factory.</param>
        /// <param name="modelFactory">The model factory.</param>
        public FileProcessorManager(List<FileProfile> fileProfiles,
                                    Shared.EntityFramework.IDbContextFactory<EstateReportingGenericContext> dbContextFactory,
                                    IModelFactory modelFactory,
                                    IAggregateRepository<FileAggregate, DomainEvent> fileAggregateRepository)
        {
            this.FileProfiles = fileProfiles;
            this.DbContextFactory = dbContextFactory;
            this.ModelFactory = modelFactory;
            this.FileAggregateRepository = fileAggregateRepository;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets all file profiles.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<List<FileProfile>> GetAllFileProfiles(CancellationToken cancellationToken)
        {
            return this.FileProfiles;
        }

        /// <summary>
        /// Gets the file profile.
        /// </summary>
        /// <param name="fileProfileId">The file profile identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<FileProfile> GetFileProfile(Guid fileProfileId,
                                                      CancellationToken cancellationToken)
        {
            return this.FileProfiles.SingleOrDefault(f => f.FileProfileId == fileProfileId);
        }

        /// <summary>
        /// Gets the file import logs.
        /// </summary>
        /// <param name="estateId"></param>
        /// <param name="startDateTime">The start date time.</param>
        /// <param name="endDateTime">The end date time.</param>
        /// <param name="merchantId">The merchant identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<List<FileImportLog>> GetFileImportLogs(Guid estateId,
                                                                 DateTime startDateTime,
                                                                 DateTime endDateTime,
                                                                 Guid? merchantId,
                                                                 CancellationToken cancellationToken)
        {
            EstateReportingGenericContext context = await this.DbContextFactory.GetContext(estateId, cancellationToken);

            List<EstateReporting.Database.Entities.FileImportLog> importLogQuery =
                await context.FileImportLogs.AsAsyncEnumerable().Where(f => f.ImportLogDateTime >= startDateTime).ToListAsync(cancellationToken);

            List<FileImportLogFile> importLogFileQuery = await context.FileImportLogFiles.AsAsyncEnumerable()
                                                                      .Where(fi => importLogQuery.Select(f => f.FileImportLogId).Contains(fi.FileImportLogId))
                                                                      .ToListAsync(cancellationToken);

            if (merchantId.HasValue)
            {
                importLogFileQuery = importLogFileQuery.Where(i => i.MerchantId == merchantId.Value).ToList();
            }
            
            return this.ModelFactory.ConvertFrom(importLogQuery, importLogFileQuery);
        }

        /// <summary>
        /// Gets the file import log files.
        /// </summary>
        /// <param name="fileImportLogId">The file import log identifier.</param>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="merchantId">The merchant identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<FileImportLog> GetFileImportLog(Guid fileImportLogId, 
                                                                     Guid estateId, 
                                                                     Guid? merchantId,
                                                                     CancellationToken cancellationToken)
        {
            EstateReportingGenericContext context = await this.DbContextFactory.GetContext(estateId, cancellationToken);

            EstateReporting.Database.Entities.FileImportLog importLogQuery =
                await context.FileImportLogs.AsAsyncEnumerable().SingleOrDefaultAsync(f => f.FileImportLogId == fileImportLogId, cancellationToken);

            List<FileImportLogFile> importLogFileQuery = await context.FileImportLogFiles.AsAsyncEnumerable()
                                                                      .Where(fi => fi.FileImportLogId == fileImportLogId)
                                                                      .ToListAsync(cancellationToken);

            if (merchantId.HasValue)
            {
                importLogFileQuery = importLogFileQuery.Where(i => i.MerchantId == merchantId.Value).ToList();
            }

            return this.ModelFactory.ConvertFrom(importLogQuery, importLogFileQuery);
        }

        /// <summary>
        /// Gets the file.
        /// </summary>
        /// <param name="fileId">The file identifier.</param>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<FileDetails> GetFile(Guid fileId,
            Guid estateId,
            CancellationToken cancellationToken)
        {
            FileAggregate fileAggregate =
                await this.FileAggregateRepository.GetLatestVersion(fileId, cancellationToken);

            if (fileAggregate.IsCreated == false)
            {
                throw new NotFoundException($"File with Id [{fileId}] not found");
            }

            FileDetails fileDetails = fileAggregate.GetFile();

            EstateReportingGenericContext context = await this.DbContextFactory.GetContext(estateId, cancellationToken);

            Merchant merchant = await context.Merchants.AsAsyncEnumerable()
                .SingleOrDefaultAsync(m => m.MerchantId == fileDetails.MerchantId, cancellationToken);

            if (merchant != null)
            {
                fileDetails.MerchantName = merchant.Name;
            }

            EstateSecurityUser userDetails = await context.EstateSecurityUsers.AsAsyncEnumerable()
                .SingleOrDefaultAsync(u => u.SecurityUserId == fileDetails.UserId);
            if (userDetails != null)
            {
                fileDetails.UserEmailAddress = userDetails.EmailAddress;
            }

            FileProfile fileProfile = await this.GetFileProfile(fileDetails.FileProfileId, cancellationToken);
            if (fileProfile != null)
            {
                fileDetails.FileProfileName = fileProfile.Name;
            }

            return fileDetails;
        }

        #endregion
    }
}