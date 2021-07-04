namespace FileProcessor.Common
{
    using System.Collections.Generic;
    using DataTransferObjects.Responses;
    using FileImportLog = FIleProcessor.Models.FileImportLog;

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

        #endregion
    }
}