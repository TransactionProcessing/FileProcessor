using TransactionProcessor.Database.Entities;

namespace FileProcessor.BusinessLogic.Common
{
    using System;
    using System.Collections.Generic;
    using FileImportLogModel = FileProcessor.Models.FileImportLog;

    /// <summary>
    /// 
    /// </summary>
    public interface IModelFactory
    {
        #region Methods
        
        List<FileImportLogModel> ConvertFrom(Guid estateId,
                                             List<TransactionProcessor.Database.Entities.FileImportLog> importLogs,
                                             List<(TransactionProcessor.Database.Entities.FileImportLogFile, TransactionProcessor.Database.Entities.File, 
                                                 TransactionProcessor.Database.Entities.Merchant)> importLogFilesList);
        
        FileImportLogModel ConvertFrom(Guid estateId,
                                       TransactionProcessor.Database.Entities.FileImportLog importLog,
                                       List<(TransactionProcessor.Database.Entities.FileImportLogFile, TransactionProcessor.Database.Entities.File, 
                                           TransactionProcessor.Database.Entities.Merchant)> importLogFilesList);

        #endregion
    }
}