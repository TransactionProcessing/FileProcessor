namespace FileProcessor.BusinessLogic.Managers
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using FIleProcessor.Models;

    /// <summary>
    /// 
    /// </summary>
    public interface IFileProcessorManager
    {
        #region Methods

        /// <summary>
        /// Gets all file profiles.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<List<FileProfile>> GetAllFileProfiles(CancellationToken cancellationToken);

        /// <summary>
        /// Gets the file profile.
        /// </summary>
        /// <param name="fileProfileId">The file profile identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<FileProfile> GetFileProfile(Guid fileProfileId,
                                         CancellationToken cancellationToken);

        /// <summary>
        /// Gets the file import logs.
        /// </summary>
        /// <param name="estateId"></param>
        /// <param name="startDateTime">The start date time.</param>
        /// <param name="endDateTime">The end date time.</param>
        /// <param name="merchantId">The merchant identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<List<FileImportLog>> GetFileImportLogs(Guid estateId, DateTime startDateTime, DateTime endDateTime, Guid? merchantId, CancellationToken cancellationToken);

        #endregion
    }
}