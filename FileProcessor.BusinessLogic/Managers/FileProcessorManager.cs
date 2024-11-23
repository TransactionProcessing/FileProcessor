using FileProcessor.Models;
using Shared.Results;
using SimpleResults;

namespace FileProcessor.BusinessLogic.Managers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using System.Threading.Tasks;
    using Common;
    using EstateManagement.Database.Contexts;
    using EstateManagement.Database.Entities;
    using FileAggregate;
    using FileProcessor.Models;
    using Microsoft.EntityFrameworkCore;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EventStore.Aggregate;
    using Shared.Exceptions;
    using FileImportLog = FileProcessor.Models.FileImportLog;

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

        private readonly Shared.EntityFramework.IDbContextFactory<EstateManagementGenericContext> DbContextFactory;

        private readonly IModelFactory ModelFactory;

        private readonly IAggregateRepository<FileAggregate, DomainEvent> FileAggregateRepository;

        private const String ConnectionStringIdentifier = "EstateReportingReadModel";

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FileProcessorManager" /> class.
        /// </summary>
        /// <param name="fileProfiles">The file profiles.</param>
        /// <param name="dbContextFactory">The database context factory.</param>
        /// <param name="modelFactory">The model factory.</param>
        public FileProcessorManager(List<FileProfile> fileProfiles,
                                    Shared.EntityFramework.IDbContextFactory<EstateManagementGenericContext> dbContextFactory,
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

        public async Task<Result<List<FileProfile>>> GetAllFileProfiles(CancellationToken cancellationToken)
        {
            return Result.Success(this.FileProfiles);
        }

        public async Task<Result<FileProfile>> GetFileProfile(Guid fileProfileId,
                                                              CancellationToken cancellationToken)
        {
            FileProfile fileProfile = this.FileProfiles.SingleOrDefault(f => f.FileProfileId == fileProfileId);
            if (fileProfile == null)
                return Result.NotFound($"No file profile found for File Profile Id {fileProfileId}");

            return Result.Success(fileProfile);

        }

        public async Task<Result<List<FileImportLog>>> GetFileImportLogs(Guid estateId,
                                                                 DateTime startDateTime,
                                                                 DateTime endDateTime,
                                                                 Guid? merchantId,
                                                                 CancellationToken cancellationToken)
        {
            EstateManagementGenericContext context = await this.DbContextFactory.GetContext(estateId, ConnectionStringIdentifier, cancellationToken);

            List<EstateManagement.Database.Entities.FileImportLog> importLogQuery =
                await context.FileImportLogs.AsAsyncEnumerable().Where(f => f.ImportLogDateTime >= startDateTime).ToListAsync(cancellationToken);

            var importLogFileQuery = await context.FileImportLogFiles
                                                  .Join(context.Files,
                                                        fileImportLogFile => fileImportLogFile.FileId,
                                                        file => file.FileId,
                                                        (fileImportLogFile, file) => new {
                                                                                             fileImportLogFile,
                                                                                             file
                                                                                         })
                                                  .Join(context.Merchants,
                                                        file => file.file.MerchantId,
                                                        merchant => merchant.MerchantId,
                                                        (file, merchant) => new {
                                                                                    file,
                                                                                    merchant
                                                                                })
                                                  .AsAsyncEnumerable()
                                                  .Where(fi => importLogQuery.Select(f => f.FileImportLogId).Contains(fi.file.fileImportLogFile.FileImportLogId))
                                                  .ToListAsync(cancellationToken);

            if (merchantId.HasValue){
                Merchant merchant = await context.Merchants.SingleOrDefaultAsync(m => m.MerchantId == merchantId.Value, cancellationToken:cancellationToken);
                importLogFileQuery = importLogFileQuery.Where(i => i.file.fileImportLogFile.MerchantId == merchant.MerchantId).ToList();
            }

            List<(FileImportLogFile, File,Merchant)> entityData = new List<(FileImportLogFile, File, Merchant)>();
            foreach (var file in importLogFileQuery){
                entityData.Add((file.file.fileImportLogFile, file.file.file, file.merchant));
            }

            List<FileImportLog> model = this.ModelFactory.ConvertFrom(estateId, importLogQuery, entityData);
            return Result.Success(model);
        }

        public async Task<Result<FileImportLog>> GetFileImportLog(Guid fileImportLogId, 
                                                                     Guid estateId, 
                                                                     Guid? merchantId,
                                                                     CancellationToken cancellationToken)
        {
            EstateManagementGenericContext context = await this.DbContextFactory.GetContext(estateId, ConnectionStringIdentifier, cancellationToken);

            EstateManagement.Database.Entities.FileImportLog importLogQuery =
                await context.FileImportLogs.AsAsyncEnumerable().SingleOrDefaultAsync(f => f.FileImportLogId == fileImportLogId, cancellationToken);

            var importLogFileQuery = await context.FileImportLogFiles
                                                                      .Join(context.Files,
                                                                            fileImportLogFile => fileImportLogFile.FileId,
                                                                            file => file.FileId,
                                                                            (fileImportLogFile, file) => new{
                                                                                                                fileImportLogFile,
                                                                                                                file
                                                                                                            })
                                                                      .Join(context.Merchants,
                                                                            file => file.file.MerchantId,
                                                                            merchant => merchant.MerchantId,
                                                                            (file, merchant) => new{
                                                                                                                                    file,
                                                                                                                                    merchant
                                                                                                                                })
                                                                      .AsAsyncEnumerable()
                                                                      .Where(fi => fi.file.fileImportLogFile.FileImportLogId == importLogQuery.FileImportLogId)
                                                                      .ToListAsync(cancellationToken);
            
            if (merchantId.HasValue)
            {
                Merchant merchant = await context.Merchants.SingleOrDefaultAsync(m => m.MerchantId == merchantId.Value, cancellationToken: cancellationToken);
                importLogFileQuery = importLogFileQuery.Where(i => i.file.fileImportLogFile.MerchantId == merchant.MerchantId).ToList();
            }

            List<(FileImportLogFile, File,Merchant)> entityData = new List<(FileImportLogFile, File, Merchant)>();
            foreach (var file in importLogFileQuery)
            {
                entityData.Add((file.file.fileImportLogFile, file.file.file, file.merchant));
            }

            return this.ModelFactory.ConvertFrom(estateId, importLogQuery, entityData);
        }

        public async Task<Result<FileDetails>> GetFile(Guid fileId,
            Guid estateId,
            CancellationToken cancellationToken)
        {
            Result<FileAggregate> fileAggregateResult=
                await this.FileAggregateRepository.GetLatestVersion(fileId, cancellationToken);

            if (fileAggregateResult.IsFailed)
                return ResultHelpers.CreateFailure(fileAggregateResult);

            FileAggregate fileAggregate = fileAggregateResult.Data;
            if (fileAggregate.IsCreated == false)
            {
                return Result.NotFound($"File with Id [{fileId}] not found");
            }

            FileDetails fileDetails = fileAggregate.GetFile();

            EstateManagementGenericContext context = await this.DbContextFactory.GetContext(estateId, ConnectionStringIdentifier, cancellationToken);

            Merchant merchant = await context.Merchants.AsAsyncEnumerable()
                .SingleOrDefaultAsync(m => m.MerchantId == fileDetails.MerchantId, cancellationToken);

            if (merchant != null)
            {
                fileDetails.MerchantName = merchant.Name;
            }

            EstateSecurityUser userDetails = await context.EstateSecurityUsers.AsAsyncEnumerable()
                .SingleOrDefaultAsync(u => u.SecurityUserId == fileDetails.UserId, cancellationToken: cancellationToken);
            if (userDetails != null)
            {
                fileDetails.UserEmailAddress = userDetails.EmailAddress;
            }

            FileProfile fileProfile = await this.GetFileProfile(fileDetails.FileProfileId, cancellationToken);
            if (fileProfile != null)
            {
                fileDetails.FileProfileName = fileProfile.Name;
            }

            return Result.Success(fileDetails);
        }

        #endregion
    }
}