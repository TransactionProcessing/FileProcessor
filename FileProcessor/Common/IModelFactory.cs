namespace FileProcessor.Common
{
    using System.Collections.Generic;
    using DataTransferObjects.Responses;
    using FileImportLog = FIleProcessor.Models.FileImportLog;
    using FileImportLogResponse = DataTransferObjects.Responses.FileImportLog;

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
        /// <returns></returns>
        FileImportLogList ConvertFrom(List<FileImportLog> importLogs);

        /// <summary>
        /// Converts from.
        /// </summary>
        /// <param name="fileImportLog">The file import log.</param>
        /// <returns></returns>
        FileImportLogResponse ConvertFrom(FileImportLog fileImportLog);

        #endregion
    }
}