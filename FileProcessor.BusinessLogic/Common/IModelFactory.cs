namespace FileProcessor.BusinessLogic.Common
{
    using System;
    using System.Collections.Generic;
    using EstateManagement.Database.Entities;
    using FileImportLogModel = FileProcessor.Models.FileImportLog;

    /// <summary>
    /// 
    /// </summary>
    public interface IModelFactory
    {
        #region Methods
        
        List<FileImportLogModel> ConvertFrom(Guid estateId,
                                             List<EstateManagement.Database.Entities.FileImportLog> importLogs,
                                             List<(FileImportLogFile, File, Merchant)> importLogFilesList);
        
        FileImportLogModel ConvertFrom(Guid estateId,
                                       EstateManagement.Database.Entities.FileImportLog importLog,
                                       List<(FileImportLogFile, File, Merchant)> importLogFilesList);

        #endregion
    }
}