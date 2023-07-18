namespace FileProcessor.BusinessLogic.Common
{
    using System;
    using System.Collections.Generic;
    using EstateManagement.Database.Entities;
    using FileImportLogModel = FIleProcessor.Models.FileImportLog;

    /// <summary>
    /// 
    /// </summary>
    public interface IModelFactory
    {
        #region Methods
        
        List<FileImportLogModel> ConvertFrom(Guid estateId,
                                             Guid merchantId,
                                             List<EstateManagement.Database.Entities.FileImportLog> importLogs,
                                             List<(FileImportLogFile, File)> importLogFilesList);
        
        FileImportLogModel ConvertFrom(Guid estateId,
                                       Guid merchantId,
                                       EstateManagement.Database.Entities.FileImportLog importLog,
                                       List<(FileImportLogFile, File)> importLogFilesList);

        #endregion
    }
}