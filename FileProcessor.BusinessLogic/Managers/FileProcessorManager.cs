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
    using FIleProcessor.Models;
    using Microsoft.EntityFrameworkCore;
    using FileImportLog = FIleProcessor.Models.FileImportLog;

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

        private readonly Shared.EntityFramework.IDbContextFactory<EstateReportingContext> DbContextFactory;

        private readonly IModelFactory ModelFactory;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FileProcessorManager" /> class.
        /// </summary>
        /// <param name="fileProfiles">The file profiles.</param>
        /// <param name="dbContextFactory">The database context factory.</param>
        /// <param name="modelFactory">The model factory.</param>
        public FileProcessorManager(List<FileProfile> fileProfiles,
                                    Shared.EntityFramework.IDbContextFactory<EstateReportingContext> dbContextFactory,
                                    IModelFactory modelFactory)
        {
            this.FileProfiles = fileProfiles;
            this.DbContextFactory = dbContextFactory;
            this.ModelFactory = modelFactory;
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
            EstateReportingContext context = await this.DbContextFactory.GetContext(estateId, cancellationToken);

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

        #endregion
    }
}