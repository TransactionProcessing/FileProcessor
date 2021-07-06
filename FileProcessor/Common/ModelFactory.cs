namespace FileProcessor.Common
{
    using System.Collections.Generic;
    using System.Linq;
    using DataTransferObjects.Responses;
    using FIleProcessor.Models;
    using FileImportLog = FIleProcessor.Models.FileImportLog;
    using FileImportLogResponse = DataTransferObjects.Responses.FileImportLog;

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
        /// <param name="fileImportLogs">The file import logs.</param>
        /// <returns></returns>
        public FileImportLogList ConvertFrom(List<FileImportLog> fileImportLogs)
        {
            FileImportLogList result = new FileImportLogList();
            result.FileImportLogs = new List<FileImportLogResponse>();
            foreach (FileImportLog fileImportLog in fileImportLogs)
            {
                result.FileImportLogs.Add(this.ConvertFrom(fileImportLog));
            }

            return result;
        }

        /// <summary>
        /// Converts from.
        /// </summary>
        /// <param name="fileImportLog">The file import log.</param>
        /// <returns></returns>
        public FileImportLogResponse ConvertFrom(FileImportLog fileImportLog)
        {
            FileImportLogResponse fileImportLogResponse = new FileImportLogResponse
                                                          {
                                                              FileCount = fileImportLog.Files.Count(),
                                                              FileImportLogId = fileImportLog.FileImportLogId,
                                                              ImportLogDate = fileImportLog.FileImportLogDateTime.Date,
                                                              ImportLogDateTime = fileImportLog.FileImportLogDateTime,
                                                              ImportLogTime = fileImportLog.FileImportLogDateTime.TimeOfDay,
                                                              Files = new List<FileImportLogFile>()
                                                          };

            foreach (ImportLogFile importLogFile in fileImportLog.Files)
            {
                FileImportLogFile fileImportLogFile = this.ConvertFrom(importLogFile);
                fileImportLogFile.FileImportLogId = fileImportLog.FileImportLogId;
                fileImportLogResponse.Files.Add(fileImportLogFile);
            }

            return fileImportLogResponse;
        }

        /// <summary>
        /// Converts from.
        /// </summary>
        /// <param name="importLogFile">The import log file.</param>
        /// <returns></returns>
        public FileImportLogFile ConvertFrom(ImportLogFile importLogFile)
        {
            FileImportLogFile fileImportLogFile = new FileImportLogFile
                                                  {
                                                      FileId = importLogFile.FileId,
                                                      FilePath = importLogFile.FilePath,
                                                      FileProfileId = importLogFile.FileProfileId,
                                                      FileUploadedDateTime = importLogFile.UploadedDateTime,
                                                      MerchantId = importLogFile.MerchantId,
                                                      OriginalFileName = importLogFile.OriginalFileName,
                                                      UserId = importLogFile.UserId
                                                  };

            return fileImportLogFile;
        }

        #endregion
    }
}