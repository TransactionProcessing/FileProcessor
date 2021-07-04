namespace FileProcessor.BusinessLogic.Common
{
    using System.Collections.Generic;
    using System.Linq;
    using EstateReporting.Database.Entities;
    using FIleProcessor.Models;
    using FileImportLog = FIleProcessor.Models.FileImportLog;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="FileProcessor.BusinessLogic.Common.IModelFactory" />
    public class ModelFactory : IModelFactory
    {
        #region Methods

        /// <summary>
        /// Converts from.
        /// </summary>
        /// <param name="importLogs">The import logs.</param>
        /// <param name="importLogFilesList">The import log files list.</param>
        /// <returns></returns>
        public List<FileImportLog> ConvertFrom(List<EstateReporting.Database.Entities.FileImportLog> importLogs,
                                               List<FileImportLogFile> importLogFilesList)
        {
            List<FileImportLog> models = new List<FileImportLog>();

            foreach (EstateReporting.Database.Entities.FileImportLog fileImportLog in importLogs)
            {
                var model = new FileImportLog();

                model.FileImportLogId = fileImportLog.FileImportLogId;
                model.FileImportLogDateTime = fileImportLog.ImportLogDateTime;
                model.EstateId = fileImportLog.EstateId;
                model.Files = new List<ImportLogFile>();

                var currentImportLogFiles = importLogFilesList.Where(fi => fi.FileImportLogId == fileImportLog.FileImportLogId);

                foreach (var importLogFile in currentImportLogFiles)
                {
                    model.Files.Add(new ImportLogFile
                                    {
                                        MerchantId = importLogFile.MerchantId,
                                        EstateId = importLogFile.EstateId,
                                        FileId = importLogFile.FileId,
                                        FilePath = importLogFile.FilePath,
                                        FileProfileId = importLogFile.FileProfileId,
                                        OriginalFileName = importLogFile.OriginalFileName,
                                        UserId = importLogFile.UserId
                                    });
                }

                models.Add(model);
            }

            return models;
        }

        #endregion
    }
}