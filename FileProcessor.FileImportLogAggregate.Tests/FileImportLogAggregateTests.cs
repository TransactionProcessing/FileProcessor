using System;
using SimpleResults;
using Xunit;

namespace FileProcessor.FileImportLogAggregate.Tests
{
    using System.Linq;
    using FileProcessor.Models;
    using Shouldly;
    using Testing;

    public class FileImportLogAggregateTests
    {
        [Fact]
        public void FileImportLogAggregate_CanBeCreated_IsCreated()
        {
            FileImportLogAggregate fileImportLogAggregate = FileImportLogAggregate.Create(TestData.FileImportLogId);

            FileImportLog fileImportLog = fileImportLogAggregate.GetFileImportLog();
            fileImportLog.ShouldNotBeNull();

            fileImportLog.FileImportLogId.ShouldBe(TestData.FileImportLogId);
        }

        [Fact]
        public void FileImportLogAggregate_CreateImportLog_IsCreated()
        {
            FileImportLogAggregate fileImportLogAggregate = FileImportLogAggregate.Create(TestData.FileImportLogId);
            Result result = fileImportLogAggregate.CreateImportLog(TestData.EstateId, TestData.ImportLogDateTime);
            result.IsSuccess.ShouldBeTrue();

            FileImportLog fileImportLog = fileImportLogAggregate.GetFileImportLog();
            fileImportLog.ShouldNotBeNull();

            fileImportLog.FileImportLogId.ShouldBe(TestData.FileImportLogId);
            fileImportLog.EstateId.ShouldBe(TestData.EstateId);
            fileImportLog.FileImportLogDateTime.ShouldBe(TestData.ImportLogDateTime);
        }

        [Fact]
        public void FileImportLogAggregate_CreateImportLog_AlreadyCreated_SilentlyHandled()
        {
            FileImportLogAggregate fileImportLogAggregate = FileImportLogAggregate.Create(TestData.FileImportLogId);
            fileImportLogAggregate.CreateImportLog(TestData.EstateId, TestData.ImportLogDateTime);

            FileImportLog fileImportLog = fileImportLogAggregate.GetFileImportLog();
            fileImportLog.ShouldNotBeNull();

            fileImportLog.FileImportLogId.ShouldBe(TestData.FileImportLogId);
            fileImportLog.EstateId.ShouldBe(TestData.EstateId);
            fileImportLog.FileImportLogDateTime.ShouldBe(TestData.ImportLogDateTime);

            var result = fileImportLogAggregate.CreateImportLog(TestData.EstateId, TestData.ImportLogDateTime);
            result.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public void FileImportLogAggregate_AddImportedFile_FileAdded()
        {
            FileImportLogAggregate fileImportLogAggregate = FileImportLogAggregate.Create(TestData.FileImportLogId);
            fileImportLogAggregate.CreateImportLog(TestData.EstateId, TestData.ImportLogDateTime);
            fileImportLogAggregate.AddImportedFile(TestData.FileId, TestData.MerchantId, TestData.UserId, TestData.FileProfileId, TestData.OriginalFileName, TestData.FilePath, TestData.FileUploadedDateTime);

            FileImportLog fileImportLog = fileImportLogAggregate.GetFileImportLog();
            fileImportLog.ShouldNotBeNull();

            fileImportLog.Files.ShouldNotBeNull();
            fileImportLog.Files.ShouldNotBeEmpty();
            fileImportLog.Files.Any(f => f.FileId == TestData.FileId).ShouldBeTrue();
        }

        [Fact]
        public void FileImportLogAggregate_AddImportedFile_ImportLogNotCreated_ErrorThrown() {
            FileImportLogAggregate fileImportLogAggregate = FileImportLogAggregate.Create(TestData.FileImportLogId);

            var result = fileImportLogAggregate.AddImportedFile(TestData.FileId, TestData.MerchantId, TestData.UserId, TestData.FileProfileId, TestData.OriginalFileName, TestData.FilePath, TestData.FileUploadedDateTime);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }

        [Fact]
        public void FileImportLogAggregate_AddImportedFile_DuplicateFileId_ErrorThrown()
        {
            FileImportLogAggregate fileImportLogAggregate = FileImportLogAggregate.Create(TestData.FileImportLogId);
            fileImportLogAggregate.CreateImportLog(TestData.EstateId, TestData.ImportLogDateTime);
            fileImportLogAggregate.AddImportedFile(TestData.FileId, TestData.MerchantId, TestData.UserId, TestData.FileProfileId, TestData.OriginalFileName, TestData.FilePath, TestData.FileUploadedDateTime);
            Result result = fileImportLogAggregate.AddImportedFile(TestData.FileId, TestData.MerchantId, TestData.UserId, TestData.FileProfileId, TestData.OriginalFileName, TestData.FilePath, TestData.FileUploadedDateTime);
            result.IsFailed.ShouldBeTrue();
            result.Status.ShouldBe(ResultStatus.Invalid);
        }
    }
}
