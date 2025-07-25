﻿using FileProcessor.Models;
using Shared.Results;
using SimpleResults;
using TransactionProcessor.Database.Contexts;
using TransactionProcessor.Database.Entities;

namespace FileProcessor.BusinessLogic.Managers
{
    using Common;
    using FileAggregate;
    using FileProcessor.Models;
    using Microsoft.EntityFrameworkCore;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EntityFramework;
    using Shared.EventStore.Aggregate;
    using Shared.Exceptions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading;
    using System.Threading.Tasks;
    using FileImportLog = FileProcessor.Models.FileImportLog;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="FileProcessor.BusinessLogic.Managers.IFileProcessorManager" />
    public class FileProcessorManager : IFileProcessorManager
    {
        #region Fields
        private readonly List<FileProfile> FileProfiles;

        private readonly IDbContextResolver<EstateManagementContext> Resolver;
        private static readonly String EstateManagementDatabaseName = "TransactionProcessorReadModel";

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
                                    IDbContextResolver<EstateManagementContext> resolver,
                                    IModelFactory modelFactory,
                                    IAggregateRepository<FileAggregate, DomainEvent> fileAggregateRepository)
        {
            this.FileProfiles = fileProfiles;
            this.Resolver = resolver;
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

        private async Task<EstateManagementContext> GetContext(Guid estateId)
        {
            ResolvedDbContext<EstateManagementContext>? resolvedContext = this.Resolver.Resolve(EstateManagementDatabaseName, estateId.ToString());
            return resolvedContext.Context;
        }


        public async Task<Result<List<FileImportLog>>> GetFileImportLogs(Guid estateId,
                                                                 DateTime startDateTime,
                                                                 DateTime endDateTime,
                                                                 Guid? merchantId,
                                                                 CancellationToken cancellationToken)
        {
            EstateManagementContext context = await this.GetContext(estateId);

            List<TransactionProcessor.Database.Entities.FileImportLog> importLogQuery =
                await context.FileImportLogs.Where(f => f.ImportLogDateTime >= startDateTime).ToListAsync(cancellationToken);

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
                                                  .Where(fi => importLogQuery.Select(f => f.FileImportLogId).Contains(fi.file.fileImportLogFile.FileImportLogId))
                                                  .ToListAsync(cancellationToken);

            if (merchantId.HasValue){
                importLogFileQuery = importLogFileQuery.Where(i => i.file.fileImportLogFile.MerchantId == merchantId.Value).ToList();
            }

            List<(FileImportLogFile, TransactionProcessor.Database.Entities.File,Merchant)> entityData = new List<(FileImportLogFile, TransactionProcessor.Database.Entities.File, Merchant)>();
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
            EstateManagementContext context = await this.GetContext(estateId);

            // Fetch the import log entry
            TransactionProcessor.Database.Entities.FileImportLog importLogQuery = await context.FileImportLogs
                .SingleOrDefaultAsync(f => f.FileImportLogId == fileImportLogId, cancellationToken);

            // Ensure the query starts as IQueryable
            var query = from fileImportLogFile in context.FileImportLogFiles
                join file in context.Files on fileImportLogFile.FileId equals file.FileId
                join merchant in context.Merchants on file.MerchantId equals merchant.MerchantId
                select new { fileImportLogFile, file, merchant };


            // Conditionally apply a filter
            if (merchantId.HasValue)
            {
                query = query.Where(i => i.fileImportLogFile.MerchantId == merchantId.Value);
            }

            // Enumerate at the end
            var importLogFileQuery = await query.ToListAsync(cancellationToken);

            List<(FileImportLogFile, TransactionProcessor.Database.Entities.File,Merchant)> entityData = new List<(FileImportLogFile, TransactionProcessor.Database.Entities.File, Merchant)>();
            foreach (var file in importLogFileQuery)
            {
                entityData.Add((file.fileImportLogFile, file.file, file.merchant));
            }

            var x = this.ModelFactory.ConvertFrom(estateId, importLogQuery, entityData);
            return Result.Success(x);
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

            EstateManagementContext context = await this.GetContext(estateId);

            Merchant merchant = await context.Merchants
                .SingleOrDefaultAsync(m => m.MerchantId == fileDetails.MerchantId, cancellationToken);

            if (merchant != null)
            {
                fileDetails.MerchantName = merchant.Name;
            }

            EstateSecurityUser userDetails = await context.EstateSecurityUsers
                .SingleOrDefaultAsync(u => u.SecurityUserId == fileDetails.UserId, cancellationToken: cancellationToken);
            if (userDetails != null)
            {
                fileDetails.UserEmailAddress = userDetails.EmailAddress;
            }

            Result<FileProfile> getFileProfile = await this.GetFileProfile(fileDetails.FileProfileId, cancellationToken);
            if (getFileProfile.IsFailed)
            {
                return ResultHelpers.CreateFailure(getFileProfile);
            }
            FileProfile fileProfile = getFileProfile.Data;
            if (fileProfile != null)
            {
                fileDetails.FileProfileName = fileProfile.Name;
            }

            return Result.Success(fileDetails);
        }

        #endregion
    }
}