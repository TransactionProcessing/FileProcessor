using System;
using SimpleResults;
using Xunit;

namespace FileProcessor.FileAggregate.Tests
{
    using System.Linq;
    using FileProcessor.Models;
    using Shared.Exceptions;
    using Shouldly;
    using Testing;

    public class FileAggregateTests
    {
        [Fact]
        public void FileAggregate_CanBeCreated_IsCreated()
        {
            FileAggregate fileAggregate = FileAggregate.Create(TestData.FileId);

            fileAggregate.ShouldNotBeNull();
            FileDetails fileDetails = fileAggregate.GetFile();

            fileDetails.ShouldNotBeNull();
            fileDetails.FileId.ShouldBe(TestData.FileId);
            fileDetails.ProcessingCompleted.ShouldBeFalse();
        }

        [Fact]
        public void FileAggregate_CanBeCreated_InvalidFileId_IsCreated()
        {
            Should.Throw<ArgumentException>(() => { FileAggregate fileAggregate = FileAggregate.Create(Guid.Empty); });
        }

        [Fact]
        public void FileAggregate_CreateFile_FileIsCreated()
        {
            FileAggregate fileAggregate = FileAggregate.Create(TestData.FileId);
            var result = fileAggregate.CreateFile(TestData.FileImportLogId, TestData.EstateId, TestData.MerchantId, TestData.UserId,
                                     TestData.FileProfileId, TestData.FileLocation, TestData.FileUploadedDateTime, TestData.SafaricomOperatorId);
            result.IsSuccess.ShouldBeTrue();

            fileAggregate.IsCreated.ShouldBeTrue();
            FileDetails fileDetails = fileAggregate.GetFile();

            fileDetails.ShouldNotBeNull();
            fileDetails.FileId.ShouldBe(TestData.FileId);
            fileDetails.FileImportLogId.ShouldBe(TestData.FileImportLogId);
            fileDetails.EstateId.ShouldBe(TestData.EstateId);
            fileDetails.MerchantId.ShouldBe(TestData.MerchantId);
            fileDetails.UserId.ShouldBe(TestData.UserId);
            fileDetails.FileProfileId.ShouldBe(TestData.FileProfileId);
            fileDetails.FileLocation.ShouldBe(TestData.FileLocation);
            fileDetails.FileLines.ShouldBeEmpty();
            fileDetails.ProcessingCompleted.ShouldBeFalse();
        }

        [Fact]
        public void FileAggregate_CreateFile_FileAlreadyCreated_NoErrorThrown()
        {
            FileAggregate fileAggregate = FileAggregate.Create(TestData.FileId);
            fileAggregate.CreateFile(TestData.FileImportLogId, TestData.EstateId, TestData.MerchantId, TestData.UserId,
                                     TestData.FileProfileId, TestData.FileLocation, TestData.FileUploadedDateTime, TestData.SafaricomOperatorId);

            var result = fileAggregate.CreateFile(TestData.FileImportLogId,
                                                         TestData.EstateId,
                                                         TestData.MerchantId,
                                                         TestData.UserId,
                                                         TestData.FileProfileId,
                                                         TestData.FileLocation,
                                                         TestData.FileUploadedDateTime, TestData.SafaricomOperatorId);
                            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public void FileAggregate_AddFileLine_FileLineAdded()
        {
            FileAggregate fileAggregate = FileAggregate.Create(TestData.FileId);
            fileAggregate.CreateFile(TestData.FileImportLogId, TestData.EstateId, TestData.MerchantId, TestData.UserId,
                                     TestData.FileProfileId, TestData.FileLocation, TestData.FileUploadedDateTime, TestData.SafaricomOperatorId);
            var result = fileAggregate.AddFileLine(TestData.FileLine);
            result.IsSuccess.ShouldBeTrue();

            FileDetails fileDetails = fileAggregate.GetFile();
            fileDetails.FileLines.ShouldNotBeNull();
            fileDetails.FileLines.ShouldNotBeEmpty();
            fileDetails.FileLines.ShouldHaveSingleItem();
            fileDetails.FileLines.Single().LineData.ShouldBe(TestData.FileLine);
            fileDetails.ProcessingCompleted.ShouldBeFalse();
            fileDetails.ProcessingSummary.ShouldNotBeNull();
            fileDetails.ProcessingSummary.TotalLines.ShouldBe(1);
            fileDetails.ProcessingSummary.NotProcessedLines.ShouldBe(1);
            fileDetails.ProcessingSummary.FailedLines.ShouldBe(0);
            fileDetails.ProcessingSummary.SuccessfullyProcessedLines.ShouldBe(0);
            fileDetails.ProcessingSummary.IgnoredLines.ShouldBe(0);
        }

        [Fact]
        public void FileAggregate_AddFileLine_AddDuplicateLine_FileLineIsNotAddedAdded()
        {
            FileAggregate fileAggregate = FileAggregate.Create(TestData.FileId);
            fileAggregate.CreateFile(TestData.FileImportLogId, TestData.EstateId, TestData.MerchantId, TestData.UserId,
                                     TestData.FileProfileId, TestData.FileLocation, TestData.FileUploadedDateTime, TestData.SafaricomOperatorId);
            fileAggregate.AddFileLine(TestData.FileLine);
            var result = fileAggregate.AddFileLine(TestData.FileLine);
            result.IsSuccess.ShouldBeTrue();

            FileDetails fileDetails = fileAggregate.GetFile();
            fileDetails.FileLines.ShouldNotBeNull();
            fileDetails.FileLines.ShouldNotBeEmpty();
            fileDetails.FileLines.ShouldHaveSingleItem();
            fileDetails.FileLines.Count.ShouldBe(1);
        }

        [Fact]
        public void FileAggregate_AddFileLine_FileNotCreated_FileLineAdded() {
            FileAggregate fileAggregate = FileAggregate.Create(TestData.FileId);

            var result = fileAggregate.AddFileLine(TestData.FileLine);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Fact]
        public void FileAggregate_RecordFileLineAsSuccessful_FileLineUpdatedAsSuccessful()
        {
            FileAggregate fileAggregate = FileAggregate.Create(TestData.FileId);
            fileAggregate.CreateFile(TestData.FileImportLogId, TestData.EstateId, TestData.MerchantId, TestData.UserId,
                                     TestData.FileProfileId, TestData.FileLocation, TestData.FileUploadedDateTime, TestData.SafaricomOperatorId);
            fileAggregate.AddFileLine(TestData.FileLine);
            var result = fileAggregate.RecordFileLineAsSuccessful(TestData.LineNumber, TestData.TransactionId);
            result.IsSuccess.ShouldBeTrue();

            FileDetails fileDetails = fileAggregate.GetFile();
            fileDetails.FileLines.ShouldNotBeNull();
            fileDetails.FileLines.ShouldNotBeEmpty();
            fileDetails.FileLines.ShouldHaveSingleItem();
            fileDetails.FileLines.Single().LineNumber.ShouldBe(1);
            fileDetails.FileLines.Single().LineData.ShouldBe(TestData.FileLine);
            fileDetails.FileLines.Single().ProcessingResult.ShouldBe(ProcessingResult.Successful);
            fileDetails.FileLines.Single().TransactionId.ShouldBe(TestData.TransactionId);
            fileDetails.ProcessingCompleted.ShouldBeTrue();
            fileDetails.ProcessingSummary.ShouldNotBeNull();
            fileDetails.ProcessingSummary.TotalLines.ShouldBe(1);
            fileDetails.ProcessingSummary.NotProcessedLines.ShouldBe(0);
            fileDetails.ProcessingSummary.FailedLines.ShouldBe(0);
            fileDetails.ProcessingSummary.SuccessfullyProcessedLines.ShouldBe(1);
            fileDetails.ProcessingSummary.IgnoredLines.ShouldBe(0);
        }

        [Fact]
        public void FileAggregate_RecordFileLineAsSuccessful_FileNotCreated_ErrorThrown()
        {
            FileAggregate fileAggregate = FileAggregate.Create(TestData.FileId);
            
            var result = fileAggregate.RecordFileLineAsSuccessful(TestData.LineNumber, TestData.TransactionId);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Fact]
        public void FileAggregate_RecordFileLineAsSuccessful_FileHasNoLine_ErrorThrown()
        {
            FileAggregate fileAggregate = FileAggregate.Create(TestData.FileId);
            fileAggregate.CreateFile(TestData.FileImportLogId, TestData.EstateId, TestData.MerchantId, TestData.UserId,
                                     TestData.FileProfileId, TestData.FileLocation, TestData.FileUploadedDateTime, TestData.SafaricomOperatorId);

            var result = fileAggregate.RecordFileLineAsSuccessful(TestData.LineNumber, TestData.TransactionId);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Fact]
        public void FileAggregate_RecordFileLineAsSuccessful_LineNotFound_ErrorThrown()
        {
            FileAggregate fileAggregate = FileAggregate.Create(TestData.FileId);
            fileAggregate.CreateFile(TestData.FileImportLogId, TestData.EstateId, TestData.MerchantId, TestData.UserId,
                                     TestData.FileProfileId, TestData.FileLocation, TestData.FileUploadedDateTime, TestData.SafaricomOperatorId);
            fileAggregate.AddFileLine(TestData.FileLine);
            var result = fileAggregate.RecordFileLineAsSuccessful(TestData.NotFoundLineNumber, TestData.TransactionId);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.NotFound);
        }

        [Fact]
        public void FileAggregate_RecordFileLineAsFailed_FileLineUpdatedAsFailed()
        {
            FileAggregate fileAggregate = FileAggregate.Create(TestData.FileId);
            fileAggregate.CreateFile(TestData.FileImportLogId, TestData.EstateId, TestData.MerchantId, TestData.UserId,
                                     TestData.FileProfileId, TestData.FileLocation, TestData.FileUploadedDateTime, TestData.SafaricomOperatorId);
            fileAggregate.AddFileLine(TestData.FileLine);
            var result = fileAggregate.RecordFileLineAsFailed(TestData.LineNumber, TestData.TransactionId,TestData.ResponseCodeFailed, TestData.ResponseMessageFailed);
            result.IsSuccess.ShouldBeTrue();

            FileDetails fileDetails = fileAggregate.GetFile();
            fileDetails.FileLines.ShouldNotBeNull();
            fileDetails.FileLines.ShouldNotBeEmpty();
            fileDetails.FileLines.ShouldHaveSingleItem();
            fileDetails.FileLines.Single().LineNumber.ShouldBe(1);
            fileDetails.FileLines.Single().LineData.ShouldBe(TestData.FileLine);
            fileDetails.FileLines.Single().ProcessingResult.ShouldBe(ProcessingResult.Failed);
            fileDetails.FileLines.Single().TransactionId.ShouldBe(TestData.TransactionId);
            fileDetails.ProcessingCompleted.ShouldBeTrue();
            fileDetails.ProcessingSummary.ShouldNotBeNull();
            fileDetails.ProcessingSummary.TotalLines.ShouldBe(1);
            fileDetails.ProcessingSummary.NotProcessedLines.ShouldBe(0);
            fileDetails.ProcessingSummary.FailedLines.ShouldBe(1);
            fileDetails.ProcessingSummary.SuccessfullyProcessedLines.ShouldBe(0);
            fileDetails.ProcessingSummary.IgnoredLines.ShouldBe(0);
        }

        [Fact]
        public void FileAggregate_RecordFileLineAsFailed_FileNotCreated_ErrorThrown()
        {
            FileAggregate fileAggregate = FileAggregate.Create(TestData.FileId);

            var result = fileAggregate.RecordFileLineAsFailed(TestData.LineNumber, TestData.TransactionId, TestData.ResponseCodeFailed, TestData.ResponseMessageFailed);
                                                    
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Fact]
        public void FileAggregate_RecordFileLineAsFailed_FileHasNoLines_ErrorThrown()
        {
            FileAggregate fileAggregate = FileAggregate.Create(TestData.FileId);
            fileAggregate.CreateFile(TestData.FileImportLogId, TestData.EstateId, TestData.MerchantId, TestData.UserId,
                                     TestData.FileProfileId, TestData.FileLocation, TestData.FileUploadedDateTime, TestData.SafaricomOperatorId);

            var result = fileAggregate.RecordFileLineAsFailed(TestData.LineNumber, TestData.TransactionId, TestData.ResponseCodeFailed, TestData.ResponseMessageFailed);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Fact]
        public void FileAggregate_RecordFileLineAsFailed_LineNotFound_ErrorThrown()
        {
            FileAggregate fileAggregate = FileAggregate.Create(TestData.FileId);
            fileAggregate.CreateFile(TestData.FileImportLogId, TestData.EstateId, TestData.MerchantId, TestData.UserId,
                                     TestData.FileProfileId, TestData.FileLocation, TestData.FileUploadedDateTime, TestData.SafaricomOperatorId);
            fileAggregate.AddFileLine(TestData.FileLine);
            var result = fileAggregate.RecordFileLineAsFailed(TestData.NotFoundLineNumber, TestData.TransactionId, TestData.ResponseCodeFailed, TestData.ResponseMessageFailed);
                                            
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.NotFound);
        }

        [Fact]
        public void FileAggregate_RecordFileLineAsIgnored_FileLineUpdatedAsIgnored()
        {
            FileAggregate fileAggregate = FileAggregate.Create(TestData.FileId);
            fileAggregate.CreateFile(TestData.FileImportLogId, TestData.EstateId, TestData.MerchantId, TestData.UserId,
                                     TestData.FileProfileId, TestData.FileLocation, TestData.FileUploadedDateTime, TestData.SafaricomOperatorId);
            fileAggregate.AddFileLine(TestData.FileLine);
            var result = fileAggregate.RecordFileLineAsIgnored(TestData.LineNumber);
            result.IsSuccess.ShouldBeTrue();

            FileDetails fileDetails = fileAggregate.GetFile();
            fileDetails.FileLines.ShouldNotBeNull();
            fileDetails.FileLines.ShouldNotBeEmpty();
            fileDetails.FileLines.ShouldHaveSingleItem();
            fileDetails.FileLines.Single().LineNumber.ShouldBe(1);
            fileDetails.FileLines.Single().LineData.ShouldBe(TestData.FileLine);
            fileDetails.FileLines.Single().ProcessingResult.ShouldBe(ProcessingResult.Ignored);
            fileDetails.ProcessingCompleted.ShouldBeTrue();
            fileDetails.ProcessingSummary.ShouldNotBeNull();
            fileDetails.ProcessingSummary.TotalLines.ShouldBe(1);
            fileDetails.ProcessingSummary.NotProcessedLines.ShouldBe(0);
            fileDetails.ProcessingSummary.FailedLines.ShouldBe(0);
            fileDetails.ProcessingSummary.SuccessfullyProcessedLines.ShouldBe(0);
            fileDetails.ProcessingSummary.IgnoredLines.ShouldBe(1);
        }

        [Fact]
        public void FileAggregate_RecordFileLineAsIgnored_FileNotCreated_ErrorThrown()
        {
            FileAggregate fileAggregate = FileAggregate.Create(TestData.FileId);

            var result = fileAggregate.RecordFileLineAsIgnored(TestData.LineNumber);
            
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Fact]
        public void FileAggregate_FileAggregate_RecordFileLineAsIgnored_FileHasNoLine_ErrorThrown()
        {
            FileAggregate fileAggregate = FileAggregate.Create(TestData.FileId);
            fileAggregate.CreateFile(TestData.FileImportLogId, TestData.EstateId, TestData.MerchantId, TestData.UserId,
                                     TestData.FileProfileId, TestData.FileLocation, TestData.FileUploadedDateTime, TestData.SafaricomOperatorId);

            var result = fileAggregate.RecordFileLineAsIgnored(TestData.LineNumber);
            
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Fact]
        public void FileAggregate_RecordFileLineAsIgnored_LineNotFound_ErrorThrown()
        {
            FileAggregate fileAggregate = FileAggregate.Create(TestData.FileId);
            fileAggregate.CreateFile(TestData.FileImportLogId, TestData.EstateId, TestData.MerchantId, TestData.UserId,
                                     TestData.FileProfileId, TestData.FileLocation, TestData.FileUploadedDateTime, TestData.SafaricomOperatorId);
            fileAggregate.AddFileLine(TestData.FileLine);
            var result = fileAggregate.RecordFileLineAsIgnored(TestData.NotFoundLineNumber);
            
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.NotFound);
        }



        [Fact]
        public void FileAggregate_RecordFileLineAsRejected_FileLineUpdatedAsRejected()
        {
            FileAggregate fileAggregate = FileAggregate.Create(TestData.FileId);
            fileAggregate.CreateFile(TestData.FileImportLogId, TestData.EstateId, TestData.MerchantId, TestData.UserId,
                                     TestData.FileProfileId, TestData.FileLocation, TestData.FileUploadedDateTime, TestData.SafaricomOperatorId);
            fileAggregate.AddFileLine(TestData.FileLine);
            var result = fileAggregate.RecordFileLineAsRejected(TestData.LineNumber, TestData.RejectionReason);
            result.IsSuccess.ShouldBeTrue();

            FileDetails fileDetails = fileAggregate.GetFile();
            fileDetails.FileLines.ShouldNotBeNull();
            fileDetails.FileLines.ShouldNotBeEmpty();
            fileDetails.FileLines.ShouldHaveSingleItem();
            fileDetails.FileLines.Single().LineNumber.ShouldBe(1);
            fileDetails.FileLines.Single().LineData.ShouldBe(TestData.FileLine);
            fileDetails.FileLines.Single().ProcessingResult.ShouldBe(ProcessingResult.Rejected);
            fileDetails.FileLines.Single().RejectedReason.ShouldBe(TestData.RejectionReason);
            fileDetails.ProcessingCompleted.ShouldBeTrue();
            fileDetails.ProcessingSummary.ShouldNotBeNull();
            fileDetails.ProcessingSummary.TotalLines.ShouldBe(1);
            fileDetails.ProcessingSummary.NotProcessedLines.ShouldBe(0);
            fileDetails.ProcessingSummary.FailedLines.ShouldBe(0);
            fileDetails.ProcessingSummary.SuccessfullyProcessedLines.ShouldBe(0);
            fileDetails.ProcessingSummary.IgnoredLines.ShouldBe(0);
            fileDetails.ProcessingSummary.RejectedLines.ShouldBe(1);
        }

        [Fact]
        public void FileAggregate_RecordFileLineAsRejected_FileNotCreated_ErrorThrown()
        {
            FileAggregate fileAggregate = FileAggregate.Create(TestData.FileId);

            var result = fileAggregate.RecordFileLineAsRejected(TestData.LineNumber,TestData.RejectionReason);
            
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Fact]
        public void FileAggregate_RecordFileLineAsRejected_FileHasNoLine_ErrorThrown()
        {
            FileAggregate fileAggregate = FileAggregate.Create(TestData.FileId);
            fileAggregate.CreateFile(TestData.FileImportLogId, TestData.EstateId, TestData.MerchantId, TestData.UserId,
                                     TestData.FileProfileId, TestData.FileLocation, TestData.FileUploadedDateTime, TestData.SafaricomOperatorId);

            var result = fileAggregate.RecordFileLineAsRejected(TestData.LineNumber,TestData.RejectionReason);
            
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Fact]
        public void FileAggregate_RecordFileLineAsRejected_LineNotFound_ErrorThrown()
        {
            FileAggregate fileAggregate = FileAggregate.Create(TestData.FileId);
            fileAggregate.CreateFile(TestData.FileImportLogId, TestData.EstateId, TestData.MerchantId, TestData.UserId,
                                     TestData.FileProfileId, TestData.FileLocation, TestData.FileUploadedDateTime, TestData.SafaricomOperatorId);
            fileAggregate.AddFileLine(TestData.FileLine);
            var result = fileAggregate.RecordFileLineAsRejected(TestData.NotFoundLineNumber, TestData.RejectionReason);
            
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.NotFound);
        }
    }
}
