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
                new UploadFileRequest(TestData.EstateId, TestData.MerchantId, TestData.UserId, TestData.FilePath, TestData.FileProfileId, TestData.FileUploadedDateTime);

            uploadFileRequest.EstateId.ShouldBe(TestData.EstateId);
            uploadFileRequest.MerchantId.ShouldBe(TestData.MerchantId);
            uploadFileRequest.UserId.ShouldBe(TestData.UserId);
            uploadFileRequest.FilePath.ShouldBe(TestData.FilePath);
            uploadFileRequest.FileProfileId.ShouldBe(TestData.FileProfileId);
            uploadFileRequest.FileUploadedDateTime.ShouldBe(TestData.FileUploadedDateTime);
        }

        [Fact]
        public void ProcessUploadedFileRequest_CanBeCreated_IsCreated()
        {
            ProcessUploadedFileRequest processUploadedFileRequest =
                new ProcessUploadedFileRequest(TestData.EstateId, TestData.MerchantId, TestData.FileImportLogId, TestData.FileId, TestData.UserId, TestData.FilePath, TestData.FileProfileId,
                                               TestData.FileUploadedDateTime);

            processUploadedFileRequest.EstateId.ShouldBe(TestData.EstateId);
            processUploadedFileRequest.MerchantId.ShouldBe(TestData.MerchantId);
            processUploadedFileRequest.FileImportLogId.ShouldBe(TestData.FileImportLogId);
            processUploadedFileRequest.FileId.ShouldBe(TestData.FileId);
            processUploadedFileRequest.UserId.ShouldBe(TestData.UserId);
            processUploadedFileRequest.FilePath.ShouldBe(TestData.FilePath);
            processUploadedFileRequest.FileProfileId.ShouldBe(TestData.FileProfileId);
            processUploadedFileRequest.FileUploadedDateTime.ShouldBe(TestData.FileUploadedDateTime);
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
