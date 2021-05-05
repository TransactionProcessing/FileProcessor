using System;
using Xunit;

namespace FileProcessor.DomainEvents.Tests
{
    using File.DomainEvents;
    using Shouldly;
    using Testing;

    public class DomainEventTests
    {
        [Fact]
        public void FileUploadedEvent_CanBeCreated_IsCreated()
        {
            FileUploadedEvent fileUploadedEvent = new FileUploadedEvent(TestData.FileId,
                                                                        TestData.EstateId,
                                                                        TestData.MerchantId,
                                                                        TestData.UserId,
                                                                        TestData.FileProfileId,
                                                                        TestData.OriginalFileName);

            fileUploadedEvent.FileId.ShouldBe(TestData.FileId);
            fileUploadedEvent.EstateId.ShouldBe(TestData.EstateId);
            fileUploadedEvent.MerchantId.ShouldBe(TestData.MerchantId);
            fileUploadedEvent.UserId.ShouldBe(TestData.UserId);
            fileUploadedEvent.FileProfileId.ShouldBe(TestData.FileProfileId);
            fileUploadedEvent.OriginalFileName.ShouldBe(TestData.OriginalFileName);
        }

        [Fact]
        public void FileLineAddedEvent_CanBeCreated_IsCreated()
        {
            FileLineAddedEvent fileLineAddedEvent = new FileLineAddedEvent(TestData.FileId, TestData.EstateId, TestData.LineNumber, TestData.FileLine);

            fileLineAddedEvent.FileLine.ShouldBe(TestData.FileLine);
            fileLineAddedEvent.EstateId.ShouldBe(TestData.EstateId);
            fileLineAddedEvent.FileId.ShouldBe(TestData.FileId);
            fileLineAddedEvent.LineNumber.ShouldBe(TestData.LineNumber);
        }

        [Fact]
        public void FileLineProcessingSuccessfulEvent_CanBeCreated_IsCreated()
        {
            FileLineProcessingSuccessfulEvent fileLineProcessingSuccessfulEvent =
                new FileLineProcessingSuccessfulEvent(TestData.FileId, TestData.EstateId, TestData.LineNumber, TestData.TransactionId);

            fileLineProcessingSuccessfulEvent.FileId.ShouldBe(TestData.FileId);
            fileLineProcessingSuccessfulEvent.LineNumber.ShouldBe(TestData.LineNumber);
            fileLineProcessingSuccessfulEvent.TransactionId.ShouldBe(TestData.TransactionId);
            fileLineProcessingSuccessfulEvent.EstateId.ShouldBe(TestData.EstateId);
        }

        [Fact]
        public void FileLineProcessingFailedEvent_CanBeCreated_IsCreated()
        {
            FileLineProcessingFailedEvent fileLineProcessingFailedEvent =
                new FileLineProcessingFailedEvent(TestData.FileId, TestData.EstateId, TestData.LineNumber, TestData.TransactionId,
                                                  TestData.ResponseCode, TestData.ResponseMessage);

            fileLineProcessingFailedEvent.FileId.ShouldBe(TestData.FileId);
            fileLineProcessingFailedEvent.LineNumber.ShouldBe(TestData.LineNumber);
            fileLineProcessingFailedEvent.TransactionId.ShouldBe(TestData.TransactionId);
            fileLineProcessingFailedEvent.EstateId.ShouldBe(TestData.EstateId);
            fileLineProcessingFailedEvent.ResponseCode.ShouldBe(TestData.ResponseCode);
            fileLineProcessingFailedEvent.ResponseMessage.ShouldBe(TestData.ResponseMessage);

        }
    }
}
