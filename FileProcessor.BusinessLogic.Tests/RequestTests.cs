using System;
using Xunit;

namespace FileProcessor.BusinessLogic.Tests
{
    using Requests;
    using Shouldly;
    using Testing;

    public class RequestTests
    {
        [Fact]
        public void UploadFileRequest_CanBeCreated_IsCreated()
        {
            UploadFileRequest uploadFileRequest =
                new UploadFileRequest(TestData.EstateId, TestData.MerchantId, TestData.FileId, TestData.UserId, TestData.FilePath, TestData.FileProfileId);

            uploadFileRequest.EstateId.ShouldBe(TestData.EstateId);
            uploadFileRequest.MerchantId.ShouldBe(TestData.MerchantId);
            uploadFileRequest.FileId.ShouldBe(TestData.FileId);
            uploadFileRequest.UserId.ShouldBe(TestData.UserId);
            uploadFileRequest.FilePath.ShouldBe(TestData.FilePath);
            uploadFileRequest.FileProfileId.ShouldBe(TestData.FileProfileId);
        }

        [Fact]
        public void ProcessUploadedFileRequest_CanBeCreated_IsCreated()
        {
            ProcessUploadedFileRequest processUploadedFileRequest =
                new ProcessUploadedFileRequest(TestData.EstateId, TestData.MerchantId, TestData.FileId, TestData.UserId, TestData.FilePath, TestData.FileProfileId);

            processUploadedFileRequest.EstateId.ShouldBe(TestData.EstateId);
            processUploadedFileRequest.MerchantId.ShouldBe(TestData.MerchantId);
            processUploadedFileRequest.FileId.ShouldBe(TestData.FileId);
            processUploadedFileRequest.UserId.ShouldBe(TestData.UserId);
            processUploadedFileRequest.FilePath.ShouldBe(TestData.FilePath);
            processUploadedFileRequest.FileProfileId.ShouldBe(TestData.FileProfileId);
        }

        [Fact]
        public void SafaricomTopupRequest_CanBeCreated_IsCreated()
        {
            SafaricomTopupRequest safaricomTopupRequest = new SafaricomTopupRequest(TestData.FileId, TestData.FileName, TestData.FileProfileId);

            safaricomTopupRequest.FileId.ShouldBe(TestData.FileId);
            safaricomTopupRequest.FileName.ShouldBe(TestData.FileName);
            safaricomTopupRequest.FileProfileId.ShouldBe(TestData.FileProfileId);
        }

        [Fact]
        public void ProcessTransactionForFileLineRequest_CanBeCreated_IsCreated()
        {
            ProcessTransactionForFileLineRequest processTransactionForFileLineRequest =
                new ProcessTransactionForFileLineRequest(TestData.FileId, TestData.LineNumber, TestData.FileLine);

            processTransactionForFileLineRequest.FileId.ShouldBe(TestData.FileId);
            processTransactionForFileLineRequest.LineNumber.ShouldBe(TestData.LineNumber);
            processTransactionForFileLineRequest.FileLine.ShouldBe(TestData.FileLine);
        }
    }

    
}
