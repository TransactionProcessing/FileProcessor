namespace FileProcessor.Common
{
    using System.Collections.Generic;
    using System.Linq;
    using DataTransferObjects.Responses;
    using FileImportLog = FIleProcessor.Models.FileImportLog;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="FileProcessor.Common.IModelFactory" />
    public class ModelFactory : IModelFactory
    {
        #region Methods

        /// <summary>
        /// Converts from.
        /// </summary>
        /// <param name="importLogs">The import logs.</param>
        /// <returns></returns>
        public FileImportLogList ConvertFrom(List<FileImportLog> importLogs)
        {
            FileImportLogList result = new FileImportLogList();
            result.FileImportLogs = new List<DataTransferObjects.Responses.FileImportLog>();
            foreach (FileImportLog fileImportLog in importLogs)
            {
                result.FileImportLogs.Add(new DataTransferObjects.Responses.FileImportLog
                                          {
                                              FileCount = fileImportLog.Files.Count(),
                                              FileImportLogId = fileImportLog.FileImportLogId,
                                              ImportLogDate = fileImportLog.FileImportLogDateTime.Date,
                                              ImportLogDateTime = fileImportLog.FileImportLogDateTime,
                                              ImportLogTime = fileImportLog.FileImportLogDateTime.TimeOfDay
                                          });
            }

            return result;
        }

        #endregion
    }
}