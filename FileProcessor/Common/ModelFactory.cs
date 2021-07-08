namespace FileProcessor.Common
{
    using System.Collections.Generic;
    using System.Linq;
    using DataTransferObjects.Responses;
    using FIleProcessor.Models;
    using FileDetails = FIleProcessor.Models.FileDetails;
    using FileImportLog = FIleProcessor.Models.FileImportLog;
    using FileLine = FIleProcessor.Models.FileLine;
    using FileImportLogResponse = DataTransferObjects.Responses.FileImportLog;
    using FileDetailsResponse = DataTransferObjects.Responses.FileDetails;
    using FileLineResponse = DataTransferObjects.Responses.FileLine;

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

        public FileDetailsResponse ConvertFrom(FileDetails fileDetails)
        {
            FileDetailsResponse fileDetailsResponse = new FileDetailsResponse
                                                      {
                EstateId = fileDetails.EstateId,
                FileId = fileDetails.FileId,
                FileImportLogId = fileDetails.FileImportLogId,
                UserId = fileDetails.UserId,
                MerchantId = fileDetails.MerchantId,
                FileProfileId = fileDetails.FileProfileId,
                FileLocation = fileDetails.FileLocation,
                ProcessingCompleted = fileDetails.ProcessingCompleted,
                FileLines = new List<FileLineResponse>()
            };

            foreach (FileLine fileDetailsFileLine in fileDetails.FileLines)
            {
                fileDetailsResponse.FileLines.Add(new FileLineResponse
                                                  {
                                                      LineData = fileDetailsFileLine.LineData,
                                                      LineNumber = fileDetailsFileLine.LineNumber,
                                                      ProcessingResult = this.TranslateProcessingResult(fileDetailsFileLine.ProcessingResult),
                                                      TransactionId = fileDetailsFileLine.TransactionId
                                                  });
            }

            fileDetailsResponse.ProcessingSummary = new FileProcessingSummary
                                                    {
                                                        FailedLines = fileDetails.ProcessingSummary.FailedLines,
                                                        IgnoredLines = fileDetails.ProcessingSummary.IgnoredLines,
                                                        NotProcessedLines = fileDetails.ProcessingSummary.NotProcessedLines,
                                                        SuccessfullyProcessedLines = fileDetails.ProcessingSummary.SuccessfullyProcessedLines,
                                                        TotalLines = fileDetails.ProcessingSummary.TotalLines
                                                    };

            return fileDetailsResponse;
        }


        /// <summary>
        /// Translates the processing result.
        /// </summary>
        /// <param name="processingResult">The processing result.</param>
        /// <returns></returns>
        private FileLineProcessingResult TranslateProcessingResult(ProcessingResult processingResult)
        {
            switch(processingResult)
            {
                case ProcessingResult.Failed:
                    return FileLineProcessingResult.Failed;
                case ProcessingResult.Ignored:
                    return FileLineProcessingResult.Ignored;
                case ProcessingResult.NotProcessed:
                    return FileLineProcessingResult.NotProcessed;
                case ProcessingResult.Successful:
                    return FileLineProcessingResult.Successful;
                default:
                    return FileLineProcessingResult.Unknown;
            }
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