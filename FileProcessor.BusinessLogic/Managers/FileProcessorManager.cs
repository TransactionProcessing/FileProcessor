namespace FileProcessor.BusinessLogic.Managers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using FIleProcessor.Models;

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

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FileProcessorManager"/> class.
        /// </summary>
        /// <param name="fileProfiles">The file profiles.</param>
        public FileProcessorManager(List<FileProfile> fileProfiles)
        {
            this.FileProfiles = fileProfiles;
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

        #endregion
    }
}