using System;
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
            Should.Throw<ArgumentNullException>(() => { FileAggregate fileAggregate = FileAggregate.Create(Guid.Empty); });
        }

        [Fact]
        public void FileAggregate_CreateFile_FileIsCreated()
        {
            FileAggregate fileAggregate = FileAggregate.Create(TestData.FileId);
            fileAggregate.CreateFile(TestData.FileImportLogId, TestData.EstateId, TestData.MerchantId, TestData.UserId,
                                     TestData.FileProfileId, TestData.FileLocation, TestData.FileUploadedDateTime, TestData.SafaricomOperatorId);

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

            Should.NotThrow(() =>
                            {

                                fileAggregate.CreateFile(TestData.FileImportLogId,
                                                         TestData.EstateId,
                                                         TestData.MerchantId,
                                                         TestData.UserId,
                                                         TestData.FileProfileId,
                                                         TestData.FileLocation,
                                                         TestData.FileUploadedDateTime, TestData.SafaricomOperatorId);
                            });
        }

        [Fact]
        public void FileAggregate_AddFileLine_FileLineAdded()
        {
            FileAggregate fileAggregate = FileAggregate.Create(TestData.FileId);
            fileAggregate.CreateFile(TestData.FileImportLogId, TestData.EstateId, TestData.MerchantId, TestData.UserId,
                                     TestData.FileProfileId, TestData.FileLocation, TestData.FileUploadedDateTime, TestData.SafaricomOperatorId);
            fileAggregate.AddFileLine(TestData.FileLine);

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
            fileAggregate.AddFileLine(TestData.FileLine);

            FileDetails fileDetails = fileAggregate.GetFile();
            fileDetails.FileLines.ShouldNotBeNull();
            fileDetails.FileLines.ShouldNotBeEmpty();
            fileDetails.FileLines.ShouldHaveSingleItem();
            fileDetails.FileLines.Count.ShouldBe(1);
        }

        [Fact]
        public void FileAggregate_AddFileLine_FileNotCreated_FileLineAdded()
        {
            FileAggregate fileAggregate = FileAggregate.Create(TestData.FileId);

            Should.Throw<InvalidOperationException>(() =>
                                                    {
                                                        fileAggregate.AddFileLine(TestData.FileLine);
                                                    });
        }

        [Fact]
        public void FileAggregate_RecordFileLineAsSuccessful_FileLineUpdatedAsSuccessful()
        {
            FileAggregate fileAggregate = FileAggregate.Create(TestData.FileId);
            fileAggregate.CreateFile(TestData.FileImportLogId, TestData.EstateId, TestData.MerchantId, TestData.UserId,
                                     TestData.FileProfileId, TestData.FileLocation, TestData.FileUploadedDateTime, TestData.SafaricomOperatorId);
            fileAggregate.AddFileLine(TestData.FileLine);
            fileAggregate.RecordFileLineAsSuccessful(TestData.LineNumber, TestData.TransactionId);

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
            
            Should.Throw<InvalidOperationException>(() =>
                                            {
                                                fileAggregate.RecordFileLineAsSuccessful(TestData.LineNumber, TestData.TransactionId);
                                            });
        }

        [Fact]
        public void FileAggregate_RecordFileLineAsSuccessful_FileHasNoLine_ErrorThrown()
        {
            FileAggregate fileAggregate = FileAggregate.Create(TestData.FileId);
            fileAggregate.CreateFile(TestData.FileImportLogId, TestData.EstateId, TestData.MerchantId, TestData.UserId,
                                     TestData.FileProfileId, TestData.FileLocation, TestData.FileUploadedDateTime, TestData.SafaricomOperatorId);

            Should.Throw<InvalidOperationException>(() =>
                                                    {
                                                        fileAggregate.RecordFileLineAsSuccessful(TestData.LineNumber, TestData.TransactionId);
                                                    });
        }

        [Fact]
        public void FileAggregate_RecordFileLineAsSuccessful_LineNotFound_ErrorThrown()
        {
            FileAggregate fileAggregate = FileAggregate.Create(TestData.FileId);
            fileAggregate.CreateFile(TestData.FileImportLogId, TestData.EstateId, TestData.MerchantId, TestData.UserId,
                                     TestData.FileProfileId, TestData.FileLocation, TestData.FileUploadedDateTime, TestData.SafaricomOperatorId);
            fileAggregate.AddFileLine(TestData.FileLine);
            Should.Throw<NotFoundException>(() =>
                                            {
                                                fileAggregate.RecordFileLineAsSuccessful(TestData.NotFoundLineNumber, TestData.TransactionId);
                                            });
        }

        [Fact]
        public void FileAggregate_RecordFileLineAsFailed_FileLineUpdatedAsFailed()
        {
            FileAggregate fileAggregate = FileAggregate.Create(TestData.FileId);
            fileAggregate.CreateFile(TestData.FileImportLogId, TestData.EstateId, TestData.MerchantId, TestData.UserId,
                                     TestData.FileProfileId, TestData.FileLocation, TestData.FileUploadedDateTime, TestData.SafaricomOperatorId);
            fileAggregate.AddFileLine(TestData.FileLine);
            fileAggregate.RecordFileLineAsFailed(TestData.LineNumber, TestData.TransactionId,TestData.ResponseCodeFailed, TestData.ResponseMessageFailed);

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

            Should.Throw<InvalidOperationException>(() =>
                                                    {
                                                        fileAggregate.RecordFileLineAsFailed(TestData.LineNumber, TestData.TransactionId, TestData.ResponseCodeFailed, TestData.ResponseMessageFailed);
                                                    });
        }

        [Fact]
        public void FileAggregate_RecordFileLineAsFailed_FileHasNoLines_ErrorThrown()
        {
            FileAggregate fileAggregate = FileAggregate.Create(TestData.FileId);
            fileAggregate.CreateFile(TestData.FileImportLogId, TestData.EstateId, TestData.MerchantId, TestData.UserId,
                                     TestData.FileProfileId, TestData.FileLocation, TestData.FileUploadedDateTime, TestData.SafaricomOperatorId);

            Should.Throw<InvalidOperationException>(() =>
                                                    {
                                                        fileAggregate.RecordFileLineAsFailed(TestData.LineNumber, TestData.TransactionId, TestData.ResponseCodeFailed, TestData.ResponseMessageFailed);
                                                    });
        }

        [Fact]
        public void FileAggregate_RecordFileLineAsFailed_LineNotFound_ErrorThrown()
        {
            FileAggregate fileAggregate = FileAggregate.Create(TestData.FileId);
            fileAggregate.CreateFile(TestData.FileImportLogId, TestData.EstateId, TestData.MerchantId, TestData.UserId,
                                     TestData.FileProfileId, TestData.FileLocation, TestData.FileUploadedDateTime, TestData.SafaricomOperatorId);
            fileAggregate.AddFileLine(TestData.FileLine);
            Should.Throw<NotFoundException>(() =>
                                            {
                                                fileAggregate.RecordFileLineAsFailed(TestData.NotFoundLineNumber, TestData.TransactionId, TestData.ResponseCodeFailed, TestData.ResponseMessageFailed);
                                            });
        }

        [Fact]
        public void FileAggregate_RecordFileLineAsIgnored_FileLineUpdatedAsIgnored()
        {
            FileAggregate fileAggregate = FileAggregate.Create(TestData.FileId);
            fileAggregate.CreateFile(TestData.FileImportLogId, TestData.EstateId, TestData.MerchantId, TestData.UserId,
                                     TestData.FileProfileId, TestData.FileLocation, TestData.FileUploadedDateTime, TestData.SafaricomOperatorId);
            fileAggregate.AddFileLine(TestData.FileLine);
            fileAggregate.RecordFileLineAsIgnored(TestData.LineNumber);

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

            Should.Throw<InvalidOperationException>(() =>
            {
                fileAggregate.RecordFileLineAsIgnored(TestData.LineNumber);
            });
        }

        [Fact]
        public void FileAggregate_FileAggregate_RecordFileLineAsIgnored_FileHasNoLine_ErrorThrown()
        {
            FileAggregate fileAggregate = FileAggregate.Create(TestData.FileId);
            fileAggregate.CreateFile(TestData.FileImportLogId, TestData.EstateId, TestData.MerchantId, TestData.UserId,
                                     TestData.FileProfileId, TestData.FileLocation, TestData.FileUploadedDateTime, TestData.SafaricomOperatorId);

            Should.Throw<InvalidOperationException>(() =>
            {
                fileAggregate.RecordFileLineAsIgnored(TestData.LineNumber);
            });
        }

        [Fact]
        public void FileAggregate_RecordFileLineAsIgnored_LineNotFound_ErrorThrown()
        {
            FileAggregate fileAggregate = FileAggregate.Create(TestData.FileId);
            fileAggregate.CreateFile(TestData.FileImportLogId, TestData.EstateId, TestData.MerchantId, TestData.UserId,
                                     TestData.FileProfileId, TestData.FileLocation, TestData.FileUploadedDateTime, TestData.SafaricomOperatorId);
            fileAggregate.AddFileLine(TestData.FileLine);
            Should.Throw<NotFoundException>(() =>
            {
                fileAggregate.RecordFileLineAsIgnored(TestData.NotFoundLineNumber);
            });
        }



        [Fact]
        public void FileAggregate_RecordFileLineAsRejected_FileLineUpdatedAsRejected()
        {
            FileAggregate fileAggregate = FileAggregate.Create(TestData.FileId);
            fileAggregate.CreateFile(TestData.FileImportLogId, TestData.EstateId, TestData.MerchantId, TestData.UserId,
                                     TestData.FileProfileId, TestData.FileLocation, TestData.FileUploadedDateTime, TestData.SafaricomOperatorId);
            fileAggregate.AddFileLine(TestData.FileLine);
            fileAggregate.RecordFileLineAsRejected(TestData.LineNumber,TestData.RejectionReason);

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

            Should.Throw<InvalidOperationException>(() =>
            {
                fileAggregate.RecordFileLineAsRejected(TestData.LineNumber,TestData.RejectionReason);
            });
        }

        [Fact]
        public void FileAggregate_RecordFileLineAsRejected_FileHasNoLine_ErrorThrown()
        {
            FileAggregate fileAggregate = FileAggregate.Create(TestData.FileId);
            fileAggregate.CreateFile(TestData.FileImportLogId, TestData.EstateId, TestData.MerchantId, TestData.UserId,
                                     TestData.FileProfileId, TestData.FileLocation, TestData.FileUploadedDateTime, TestData.SafaricomOperatorId);

            Should.Throw<InvalidOperationException>(() =>
            {
                fileAggregate.RecordFileLineAsRejected(TestData.LineNumber,TestData.RejectionReason);
            });
        }

        [Fact]
        public void FileAggregate_RecordFileLineAsRejected_LineNotFound_ErrorThrown()
        {
            FileAggregate fileAggregate = FileAggregate.Create(TestData.FileId);
            fileAggregate.CreateFile(TestData.FileImportLogId, TestData.EstateId, TestData.MerchantId, TestData.UserId,
                                     TestData.FileProfileId, TestData.FileLocation, TestData.FileUploadedDateTime, TestData.SafaricomOperatorId);
            fileAggregate.AddFileLine(TestData.FileLine);
            Should.Throw<NotFoundException>(() =>
            {
                fileAggregate.RecordFileLineAsRejected(TestData.NotFoundLineNumber, TestData.RejectionReason);
            });
        }
    }
}
