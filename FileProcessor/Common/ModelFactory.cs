using FileProcessor.Models;

namespace FileProcessor.Common
{
    using System.Collections.Generic;
    using System.Linq;
    using DataTransferObjects.Responses;
    using FileImportLogResponse = DataTransferObjects.Responses.FileImportLog;
    using FileDetailsResponse = DataTransferObjects.Responses.FileDetails;
    using FileLineResponse = DataTransferObjects.Responses.FileLine;

    public static class ModelFactory
    {
        #region Methods

        /// <summary>
        /// Converts from.
        /// </summary>
        /// <param name="fileImportLogs">The file import logs.</param>
        /// <returns></returns>
        public static FileImportLogList ConvertFrom(List<Models.FileImportLog> fileImportLogs)
        {
            FileImportLogList result = new FileImportLogList();
            result.FileImportLogs = new List<FileImportLogResponse>();
            foreach (Models.FileImportLog fileImportLog in fileImportLogs)
            {
                result.FileImportLogs.Add(ConvertFrom(fileImportLog));
            }

            return result;
        }

        /// <summary>
        /// Converts from.
        /// </summary>
        /// <param name="fileImportLog">The file import log.</param>
        /// <returns></returns>
        public static FileImportLogResponse ConvertFrom(Models.FileImportLog fileImportLog)
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
                FileImportLogFile fileImportLogFile = ConvertFrom(importLogFile);
                fileImportLogFile.FileImportLogId = fileImportLog.FileImportLogId;
                fileImportLogResponse.Files.Add(fileImportLogFile);
            }

            return fileImportLogResponse;
        }

        public static FileDetailsResponse ConvertFrom(Models.FileDetails fileDetails)
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
                FileProfileName = fileDetails.FileProfileName,
                MerchantName = fileDetails.MerchantName,
                UserEmailAddress = fileDetails.UserEmailAddress,
                FileLines = new List<FileLineResponse>()
            };

            foreach (Models.FileLine fileDetailsFileLine in fileDetails.FileLines)
            {
                fileDetailsResponse.FileLines.Add(new FileLineResponse
                                                  {
                                                      LineData = fileDetailsFileLine.LineData,
                                                      LineNumber = fileDetailsFileLine.LineNumber,
                                                      ProcessingResult = TranslateProcessingResult(fileDetailsFileLine.ProcessingResult),
                                                      TransactionId = fileDetailsFileLine.TransactionId,
                                                      RejectionReason = fileDetailsFileLine.RejectedReason
                                                  });
            }

            fileDetailsResponse.ProcessingSummary = new FileProcessingSummary
                                                    {
                                                        FailedLines = fileDetails.ProcessingSummary.FailedLines,
                                                        IgnoredLines = fileDetails.ProcessingSummary.IgnoredLines,
                                                        NotProcessedLines = fileDetails.ProcessingSummary.NotProcessedLines,
                                                        SuccessfullyProcessedLines = fileDetails.ProcessingSummary.SuccessfullyProcessedLines,
                                                        TotalLines = fileDetails.ProcessingSummary.TotalLines,
                                                        RejectedLines = fileDetails.ProcessingSummary.RejectedLines
                                                    };

            return fileDetailsResponse;
        }


        /// <summary>
        /// Translates the processing result.
        /// </summary>
        /// <param name="processingResult">The processing result.</param>
        /// <returns></returns>
        private static  FileLineProcessingResult TranslateProcessingResult(ProcessingResult processingResult)
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
                case ProcessingResult.Rejected:
                    return FileLineProcessingResult.Rejected;
                default:
                    return FileLineProcessingResult.Unknown;
            }
        }

        /// <summary>
        /// Converts from.
        /// </summary>
        /// <param name="importLogFile">The import log file.</param>
        /// <returns></returns>
        public static FileImportLogFile ConvertFrom(ImportLogFile importLogFile)
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