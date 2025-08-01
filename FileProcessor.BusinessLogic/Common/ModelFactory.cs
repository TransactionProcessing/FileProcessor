using TransactionProcessor.Database.Entities;

namespace FileProcessor.BusinessLogic.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FileProcessor.Models;
    using FileImportLog = FileProcessor.Models.FileImportLog;

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
        public List<FileImportLog> ConvertFrom(Guid estateId,
                                               List<TransactionProcessor.Database.Entities.FileImportLog> importLogs,
                                               List<(TransactionProcessor.Database.Entities.FileImportLogFile, TransactionProcessor.Database.Entities.File, Merchant)> importLogFilesList)
        {
            List<FileImportLog> models = new List<FileImportLog>();

            foreach (TransactionProcessor.Database.Entities.FileImportLog fileImportLog in importLogs)
            {
                FileImportLog model = new FileImportLog();

                model.FileImportLogId = fileImportLog.FileImportLogId;
                model.FileImportLogDateTime = fileImportLog.ImportLogDateTime;
                model.EstateId = estateId;
                model.Files = new List<ImportLogFile>();

                IEnumerable<(FileImportLogFile, TransactionProcessor.Database.Entities.File,Merchant)> currentImportLogFiles = importLogFilesList.Where(fi => fi.Item1.FileImportLogId == fileImportLog.FileImportLogId);

                foreach ((FileImportLogFile, TransactionProcessor.Database.Entities.File, Merchant) importLogFile in currentImportLogFiles)
                {
                    model.Files.Add(new ImportLogFile
                                    {
                                        MerchantId = importLogFile.Item3.MerchantId,
                                        EstateId = estateId,
                                        FileId = importLogFile.Item2.FileId,
                                        FilePath = importLogFile.Item1.FilePath,
                                        FileProfileId = importLogFile.Item1.FileProfileId,
                                        OriginalFileName = importLogFile.Item1.OriginalFileName,
                                        UserId = importLogFile.Item1.UserId
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
        public FileImportLog ConvertFrom(Guid estateId,
                                         TransactionProcessor.Database.Entities.FileImportLog importLog,
                                         List<(TransactionProcessor.Database.Entities.FileImportLogFile, TransactionProcessor.Database.Entities.File, Merchant)> importLogFilesList) {
            FileImportLog model = new FileImportLog();

            model.FileImportLogId = importLog.FileImportLogId;
            model.FileImportLogDateTime = importLog.ImportLogDateTime;
            model.EstateId = estateId;
            model.Files = new List<ImportLogFile>();

            IEnumerable<(FileImportLogFile, TransactionProcessor.Database.Entities.File,Merchant)> currentImportLogFiles = importLogFilesList.Where(fi => fi.Item1.FileImportLogId == importLog.FileImportLogId);

            foreach ((FileImportLogFile, TransactionProcessor.Database.Entities.File,Merchant) importLogFile in currentImportLogFiles)
            {
                model.Files.Add(new ImportLogFile
                                {
                                    MerchantId = importLogFile.Item3.MerchantId,
                                    EstateId = estateId,
                                    FileId = importLogFile.Item2.FileId,
                                    FilePath = importLogFile.Item1.FilePath,
                                    FileProfileId = importLogFile.Item1.FileProfileId,
                                    OriginalFileName = importLogFile.Item1.OriginalFileName,
                                    UserId = importLogFile.Item1.UserId,
                                    UploadedDateTime = importLogFile.Item1.FileUploadedDateTime
                                });
            }

            return model;
        }

        #endregion
    }
}