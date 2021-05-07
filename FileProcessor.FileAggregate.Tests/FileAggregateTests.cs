using System;
using Xunit;

namespace FileProcessor.FileAggregate.Tests
{
    using System.Linq;
    using FIleProcessor.Models;
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
                                     TestData.FileProfileId, TestData.FileLocation);

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
        }

        [Fact]
        public void FileAggregate_CretaeFile_FileAlreadyCreated_ErrorThrown()
        {
            FileAggregate fileAggregate = FileAggregate.Create(TestData.FileId);
            fileAggregate.CreateFile(TestData.FileImportLogId, TestData.EstateId, TestData.MerchantId, TestData.UserId,
                                     TestData.FileProfileId, TestData.FileLocation);

            Should.Throw<InvalidOperationException>(() =>
                                                    {

                                                        fileAggregate.CreateFile(TestData.FileImportLogId, TestData.EstateId, TestData.MerchantId, TestData.UserId,
                                                                                 TestData.FileProfileId, TestData.FileLocation);
                                                    });
        }

        [Fact]
        public void FileAggregate_AddFileLine_FileLineAdded()
        {
            FileAggregate fileAggregate = FileAggregate.Create(TestData.FileId);
            fileAggregate.CreateFile(TestData.FileImportLogId, TestData.EstateId, TestData.MerchantId, TestData.UserId,
                                     TestData.FileProfileId, TestData.FileLocation);
            fileAggregate.AddFileLine(TestData.FileLine);

            FileDetails fileDetails = fileAggregate.GetFile();
            fileDetails.FileLines.ShouldNotBeNull();
            fileDetails.FileLines.ShouldNotBeEmpty();
            fileDetails.FileLines.ShouldHaveSingleItem();
            fileDetails.FileLines.Single().LineData.ShouldBe(TestData.FileLine);
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
                                     TestData.FileProfileId, TestData.FileLocation);
            fileAggregate.AddFileLine(TestData.FileLine);
            fileAggregate.RecordFileLineAsSuccessful(TestData.LineNumber, TestData.TransactionId);

            FileDetails fileDetails = fileAggregate.GetFile();
            fileDetails.FileLines.ShouldNotBeNull();
            fileDetails.FileLines.ShouldNotBeEmpty();
            fileDetails.FileLines.ShouldHaveSingleItem();
            fileDetails.FileLines.Single().LineNumber.ShouldBe(1);
            fileDetails.FileLines.Single().LineData.ShouldBe(TestData.FileLine);
            fileDetails.FileLines.Single().SuccessfullyProcessed.ShouldBeTrue();
            fileDetails.FileLines.Single().TransactionId.ShouldBe(TestData.TransactionId);
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
                                     TestData.FileProfileId, TestData.FileLocation);

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
                                     TestData.FileProfileId, TestData.FileLocation);
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
                                     TestData.FileProfileId, TestData.FileLocation);
            fileAggregate.AddFileLine(TestData.FileLine);
            fileAggregate.RecordFileLineAsFailed(TestData.LineNumber, TestData.TransactionId,TestData.ResponseCodeFailed, TestData.ResponseMessageFailed);

            FileDetails fileDetails = fileAggregate.GetFile();
            fileDetails.FileLines.ShouldNotBeNull();
            fileDetails.FileLines.ShouldNotBeEmpty();
            fileDetails.FileLines.ShouldHaveSingleItem();
            fileDetails.FileLines.Single().LineNumber.ShouldBe(1);
            fileDetails.FileLines.Single().LineData.ShouldBe(TestData.FileLine);
            fileDetails.FileLines.Single().SuccessfullyProcessed.ShouldBeFalse();
            fileDetails.FileLines.Single().TransactionId.ShouldBe(TestData.TransactionId);
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
                                     TestData.FileProfileId, TestData.FileLocation);

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
                                     TestData.FileProfileId, TestData.FileLocation);
            fileAggregate.AddFileLine(TestData.FileLine);
            Should.Throw<NotFoundException>(() =>
                                            {
                                                fileAggregate.RecordFileLineAsFailed(TestData.NotFoundLineNumber, TestData.TransactionId, TestData.ResponseCodeFailed, TestData.ResponseMessageFailed);
                                            });
        }
    }
}
