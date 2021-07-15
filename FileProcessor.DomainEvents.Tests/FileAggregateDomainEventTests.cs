using System;
using Xunit;

namespace FileProcessor.DomainEvents.Tests
{
    using File.DomainEvents;
    using Shouldly;
    using Testing;

    public class FileAggregateDomainEventTests
    {
        [Fact]
        public void FileCreatedEvent_CanBeCreated_IsCreated()
        {
            FileCreatedEvent fileCreatedEvent = new FileCreatedEvent(TestData.FileId,
                                                                     TestData.FileImportLogId,
                                                                     TestData.EstateId,
                                                                     TestData.MerchantId,
                                                                     TestData.UserId,
                                                                     TestData.FileProfileId,
                                                                     TestData.FileLocation,
                                                                     TestData.FileUploadedDateTime);

            fileCreatedEvent.FileId.ShouldBe(TestData.FileId);
            fileCreatedEvent.FileImportLogId.ShouldBe(TestData.FileImportLogId);
            fileCreatedEvent.EstateId.ShouldBe(TestData.EstateId);
            fileCreatedEvent.MerchantId.ShouldBe(TestData.MerchantId);
            fileCreatedEvent.UserId.ShouldBe(TestData.UserId);
            fileCreatedEvent.FileProfileId.ShouldBe(TestData.FileProfileId);
            fileCreatedEvent.FileLocation.ShouldBe(TestData.FileLocation);
            fileCreatedEvent.FileReceivedDateTime.ShouldBe(TestData.FileUploadedDateTime);
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

        [Fact]
        public void FileLineProcessingIgnoredEvent_CanBeCreated_IsCreated()
        {
            FileLineProcessingIgnoredEvent fileLineProcessingIgnoredEvent =
                new FileLineProcessingIgnoredEvent(TestData.FileId, TestData.EstateId, TestData.LineNumber);

            fileLineProcessingIgnoredEvent.FileId.ShouldBe(TestData.FileId);
            fileLineProcessingIgnoredEvent.LineNumber.ShouldBe(TestData.LineNumber);
            fileLineProcessingIgnoredEvent.EstateId.ShouldBe(TestData.EstateId);
        }

        [Fact]
        public void FileLineProcessingRejectedEvent_CanBeCreated_IsCreated()
        {
            FileLineProcessingRejectedEvent fileLineProcessingRejectedEvent =
                new FileLineProcessingRejectedEvent(TestData.FileId, TestData.EstateId, TestData.LineNumber, TestData.RejectionReason);

            fileLineProcessingRejectedEvent.FileId.ShouldBe(TestData.FileId);
            fileLineProcessingRejectedEvent.LineNumber.ShouldBe(TestData.LineNumber);
            fileLineProcessingRejectedEvent.EstateId.ShouldBe(TestData.EstateId);
            fileLineProcessingRejectedEvent.Reason.ShouldBe(TestData.RejectionReason);
        }

        [Fact]
        public void FileProcessingCompletedEvent_CanBeCreated_IsCreated()
        {
            FileProcessingCompletedEvent fileProcessingCompletedEvent =
                new FileProcessingCompletedEvent(TestData.FileId, TestData.EstateId, TestData.ProcessingCompletedDateTime);

            fileProcessingCompletedEvent.FileId.ShouldBe(TestData.FileId);
            fileProcessingCompletedEvent.EstateId.ShouldBe(TestData.EstateId);
            fileProcessingCompletedEvent.ProcessingCompletedDateTime.ShouldBe(TestData.ProcessingCompletedDateTime);
        }
    }
}
