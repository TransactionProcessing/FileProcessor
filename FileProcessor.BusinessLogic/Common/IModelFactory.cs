namespace FileProcessor.BusinessLogic.Common
{
    using System.Collections.Generic;
    using EstateReporting.Database.Entities;
    using FileImportLogModel = FIleProcessor.Models.FileImportLog;

    /// <summary>
    /// 
    /// </summary>
    public interface IModelFactory
    {
        #region Methods

        /// <summary>
        /// Converts from.
        /// </summary>
        /// <param name="importLogs">The import logs.</param>
        /// <param name="importLogFilesList">The import log files list.</param>
        /// <returns></returns>
        List<FileImportLogModel> ConvertFrom(List<FileImportLog> importLogs,
                                             List<FileImportLogFile> importLogFilesList);

        #endregion
    }
}