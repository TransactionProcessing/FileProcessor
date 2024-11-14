using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileProcessor.Tests
{
    using Common;
    using DataTransferObjects.Responses;
    using FileProcessor.Models;
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using Shouldly;
    using Testing;
    using Xunit;
    using FileDetails = FileProcessor.Models.FileDetails;
    using FileImportLog = FileProcessor.Models.FileImportLog;
    using FileLine = FileProcessor.Models.FileLine;

    public class ModelFactoryTests
    {
        [Fact]
        public void ModelFactory_ConvertFrom_FileImportLogList_IsConverted()
        {
            List<FileProcessor.Models.FileImportLog> importLogs = TestData.FileImportLogModels;

            ModelFactory modelFactory = new ModelFactory();

            FileImportLogList result = modelFactory.ConvertFrom(importLogs);

            this.VerifyFileImportLogList(importLogs, result);
        }

        [Fact]
        public void ModelFactory_ConvertFrom_FileImportLogList_WithNoFiles_IsConverted()
        {
            List<FileProcessor.Models.FileImportLog> importLogs = TestData.FileImportLogModels;

            foreach (FileImportLog fileImportLog in importLogs)
            {
                fileImportLog.Files = new List<ImportLogFile>();
            }

            ModelFactory modelFactory = new ModelFactory();

            FileImportLogList result = modelFactory.ConvertFrom(importLogs);

            this.VerifyFileImportLogList(importLogs, result);
        }

        [Fact]
        public void ModelFactory_ConvertFrom_FileImportLog_IsConverted()
        {
            FileProcessor.Models.FileImportLog importLog = TestData.FileImportLogModel1;

            ModelFactory modelFactory = new ModelFactory();

            var result = modelFactory.ConvertFrom(importLog);

            this.VerifyFileImportLog(importLog, result);
        }

        [Fact]
        public void ModelFactory_ConvertFrom_FileImportLog_WithNoFiles_IsConverted()
        {
            FileProcessor.Models.FileImportLog importLog = TestData.FileImportLogModel1;
            importLog.Files = new List<ImportLogFile>();

            ModelFactory modelFactory = new ModelFactory();

            var result = modelFactory.ConvertFrom(importLog);

            this.VerifyFileImportLog(importLog, result);
        }

        [Fact]
        public void ModelFactory_ConvertFrom_FileDetails_IsConverted()
        {
            ModelFactory modelFactory = new ModelFactory();
            FileDetails fileDetails = TestData.FileDetailsModel;

            var result = modelFactory.ConvertFrom(fileDetails);

            this.VerifyFileDetails(fileDetails, result);
        }

        private void VerifyFileDetails(FileDetails source,
                                       DataTransferObjects.Responses.FileDetails fileDetails)
        {
            fileDetails.FileImportLogId.ShouldBe(source.FileImportLogId);
            fileDetails.EstateId.ShouldBe(source.EstateId);
            fileDetails.FileId.ShouldBe(source.FileId);
            fileDetails.FileLocation.ShouldBe(source.FileLocation);
            fileDetails.FileProfileId.ShouldBe(source.FileProfileId);
            fileDetails.MerchantId.ShouldBe(source.MerchantId);
            fileDetails.UserId.ShouldBe(source.UserId);
            fileDetails.ProcessingCompleted.ShouldBe(source.ProcessingCompleted);

            if (source.ProcessingSummary != null)
            {
                fileDetails.ProcessingSummary.ShouldNotBeNull();
                fileDetails.ProcessingSummary.FailedLines.ShouldBe(source.ProcessingSummary.FailedLines);
                fileDetails.ProcessingSummary.TotalLines.ShouldBe(source.ProcessingSummary.TotalLines);
                fileDetails.ProcessingSummary.IgnoredLines.ShouldBe(source.ProcessingSummary.IgnoredLines);
                fileDetails.ProcessingSummary.NotProcessedLines.ShouldBe(source.ProcessingSummary.NotProcessedLines);
                fileDetails.ProcessingSummary.SuccessfullyProcessedLines.ShouldBe(source.ProcessingSummary.SuccessfullyProcessedLines);
            }

            foreach (FileLine sourceFileLine in source.FileLines)
            {
                DataTransferObjects.Responses.FileLine? fileLineToVerify = fileDetails.FileLines.SingleOrDefault(l => l.LineNumber == sourceFileLine.LineNumber);

                fileLineToVerify.ShouldNotBeNull();
                fileLineToVerify.LineData.ShouldBe(sourceFileLine.LineData);
                fileLineToVerify.TransactionId.ShouldBe(sourceFileLine.TransactionId);

                if (sourceFileLine.ProcessingResult == ProcessingResult.Failed)
                    fileLineToVerify.ProcessingResult.ShouldBe(FileLineProcessingResult.Failed);
                if (sourceFileLine.ProcessingResult == ProcessingResult.Successful)
                    fileLineToVerify.ProcessingResult.ShouldBe(FileLineProcessingResult.Successful);
                if (sourceFileLine.ProcessingResult == ProcessingResult.Ignored)
                    fileLineToVerify.ProcessingResult.ShouldBe(FileLineProcessingResult.Ignored);
                if (sourceFileLine.ProcessingResult == ProcessingResult.NotProcessed)
                    fileLineToVerify.ProcessingResult.ShouldBe(FileLineProcessingResult.NotProcessed);
                if (sourceFileLine.ProcessingResult == ProcessingResult.Rejected)
                {
                    fileLineToVerify.ProcessingResult.ShouldBe(FileLineProcessingResult.Rejected);
                    fileLineToVerify.RejectionReason.ShouldBe(sourceFileLine.RejectedReason);
                }
            }
        }

        private void VerifyFileImportLogList(List<FileProcessor.Models.FileImportLog> source, FileImportLogList fileImportLogList)
        {
            fileImportLogList.ShouldNotBeNull();
            fileImportLogList.FileImportLogs.ShouldNotBeNull();
            fileImportLogList.FileImportLogs.Count().ShouldBe(source.Count);

            foreach (FileImportLog fileImportLog in source)
            {
                DataTransferObjects.Responses.FileImportLog? foundFileImportLog = fileImportLogList.FileImportLogs.SingleOrDefault(i => i.FileImportLogId == fileImportLog.FileImportLogId);
                this.VerifyFileImportLog(fileImportLog, foundFileImportLog);
            }
        }

        private void VerifyFileImportLog(FileProcessor.Models.FileImportLog source,
                                     DataTransferObjects.Responses.FileImportLog fileImportLog)
        {
            fileImportLog.ShouldNotBeNull();
            fileImportLog.Files.Count.ShouldBe(source.Files.Count);
            fileImportLog.ImportLogDateTime.ShouldBe(source.FileImportLogDateTime);

            foreach (ImportLogFile importLogFile in source.Files)
            {
                var foundFile = fileImportLog.Files.SingleOrDefault(f => f.FileId == importLogFile.FileId);
                foundFile.ShouldNotBeNull();
                foundFile.FileId.ShouldBe(importLogFile.FileId);
                foundFile.FileImportLogId.ShouldBe(fileImportLog.FileImportLogId);
                foundFile.MerchantId.ShouldBe(importLogFile.MerchantId);
                foundFile.OriginalFileName.ShouldBe(importLogFile.OriginalFileName);
                foundFile.FilePath.ShouldBe(importLogFile.FilePath);
                foundFile.FileProfileId.ShouldBe(importLogFile.FileProfileId);
                foundFile.FileUploadedDateTime.ShouldBe(importLogFile.UploadedDateTime);
                foundFile.UserId.ShouldBe(importLogFile.UserId);
            }
        }
    }
}
