namespace FileProcessor.BusinessLogic.Common
{
    using System.Collections.Generic;
    using System.Linq;
    using EstateReporting.Database.Entities;
    using FIleProcessor.Models;
    using FileImportLog = FIleProcessor.Models.FileImportLog;
    using FileLine = EstateReporting.Database.Entities.FileLine;

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
                FileImportLog model = new FileImportLog();

                model.FileImportLogId = fileImportLog.FileImportLogId;
                model.FileImportLogDateTime = fileImportLog.ImportLogDateTime;
                model.EstateId = fileImportLog.EstateId;
                model.Files = new List<ImportLogFile>();

                IEnumerable<FileImportLogFile> currentImportLogFiles = importLogFilesList.Where(fi => fi.FileImportLogId == fileImportLog.FileImportLogId);

                foreach (FileImportLogFile importLogFile in currentImportLogFiles)
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

        /// <summary>
        /// Converts from.
        /// </summary>
        /// <param name="importLog">The import log.</param>
        /// <param name="importLogFilesList">The import log files list.</param>
        /// <returns></returns>
        public FileImportLog ConvertFrom(EstateReporting.Database.Entities.FileImportLog importLog,
                                         List<FileImportLogFile> importLogFilesList)
        {
            FileImportLog model = new FileImportLog();

            model.FileImportLogId = importLog.FileImportLogId;
            model.FileImportLogDateTime = importLog.ImportLogDateTime;
            model.EstateId = importLog.EstateId;
            model.Files = new List<ImportLogFile>();

            IEnumerable<FileImportLogFile> currentImportLogFiles = importLogFilesList.Where(fi => fi.FileImportLogId == importLog.FileImportLogId);

            foreach (FileImportLogFile importLogFile in currentImportLogFiles)
            {
                model.Files.Add(new ImportLogFile
                                {
                                    MerchantId = importLogFile.MerchantId,
                                    EstateId = importLogFile.EstateId,
                                    FileId = importLogFile.FileId,
                                    FilePath = importLogFile.FilePath,
                                    FileProfileId = importLogFile.FileProfileId,
                                    OriginalFileName = importLogFile.OriginalFileName,
                                    UserId = importLogFile.UserId,
                                    UploadedDateTime = importLogFile.FileUploadedDateTime
                                });
            }

            return model;
        }

        #endregion
    }
}