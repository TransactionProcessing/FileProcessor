using FileProcessor.Models;
using SimpleResults;

namespace FileProcessor.BusinessLogic.Managers
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    
    /// <summary>
    /// 
    /// </summary>
    public interface IFileProcessorManager
    {
        #region Methods

        Task<Result<List<FileProfile>>> GetAllFileProfiles(CancellationToken cancellationToken);

        Task<Result<FileProfile>> GetFileProfile(Guid fileProfileId, CancellationToken cancellationToken);

        Task<Result<List<Models.FileImportLog>>> GetFileImportLogs(Guid estateId, DateTime startDateTime, DateTime endDateTime, Guid? merchantId, CancellationToken cancellationToken);

        Task<Result<Models.FileImportLog>> GetFileImportLog(Guid fileImportLogId, Guid estateId, Guid? merchantId, CancellationToken cancellationToken);

        Task<Result<FileDetails>> GetFile(Guid fileId, Guid estateId, CancellationToken cancellationToken);

        #endregion
    }
}