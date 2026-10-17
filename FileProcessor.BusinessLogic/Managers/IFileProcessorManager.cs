using FileProcessor.Models;
using SimpleResults;

namespace FileProcessor.BusinessLogic.Managers
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using FileProfileModel = global::FileProcessor.Models.FileProfile;

    /// <summary>
    /// 
    /// </summary>
    public interface IFileProcessorManager
    {
        #region Methods

        Task<Result<List<FileProfileModel>>> GetAllFileProfiles(CancellationToken cancellationToken);

        Task<Result<List<FileProfileModel>>> GetAllFileProfileVersions(CancellationToken cancellationToken);

        Task<Result<FileProfileModel>> GetFileProfile(Guid fileProfileId, CancellationToken cancellationToken);

        Task<Result<FileProfileModel>> GetFileProfileVersion(Guid fileProfileId,
                                                             Guid fileProfileVersionId,
                                                             CancellationToken cancellationToken);

        Task<Result<Models.FileImportLogQueryResult>> GetFileImportLogs(Guid estateId, DateOnly startDate, DateOnly endDate, Guid? merchantId, Int32 pageNumber, Int32 pageSize, Boolean descending, CancellationToken cancellationToken);

        Task<Result<Models.FileImportLog>> GetFileImportLog(Guid fileImportLogId, Guid estateId, Guid? merchantId, CancellationToken cancellationToken);

        Task<Result<Models.FileImportLog>> GetFileImportLogForFile(Guid fileId, Guid estateId, CancellationToken cancellationToken);

        Task<Result<FileDetails>> GetFile(Guid fileId, Guid estateId, CancellationToken cancellationToken);

        Task<Result> ValidateUploadReferences(Guid estateId, Guid merchantId, Guid userId, CancellationToken cancellationToken);

        #endregion
    }
}
