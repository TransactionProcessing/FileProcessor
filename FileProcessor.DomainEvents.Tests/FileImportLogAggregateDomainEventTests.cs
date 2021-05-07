namespace FileProcessor.DomainEvents.Tests
{
    using FileImportLog.DomainEvents;
    using Shouldly;
    using Testing;
    using Xunit;

    public class FileImportLogAggregateDomainEventTests
    {
        [Fact]
        public void ImportLogCreatedEvent_CanBeCreated_IsCreated()
        {
            ImportLogCreatedEvent importLogCreatedEvent = new ImportLogCreatedEvent(TestData.FileImportLogId, TestData.EstateId, TestData.ImportLogDateTime);

            importLogCreatedEvent.FileImportLogId.ShouldBe(TestData.FileImportLogId);
            importLogCreatedEvent.EstateId.ShouldBe(TestData.EstateId);
            importLogCreatedEvent.ImportLogDateTime.ShouldBe(TestData.ImportLogDateTime);
        }

        [Fact]
        public void FileAddedToImportLogEvent_CanBeCreated_IsCreated()
        {
            FileAddedToImportLogEvent fileAddedToImportLogEvent = new FileAddedToImportLogEvent(TestData.FileImportLogId, TestData.FileId,
                                                                                                TestData.EstateId,TestData.MerchantId,
                                                                                                TestData.UserId, TestData.FileProfileId,
                                                                                                TestData.OriginalFileName,
                                                                                                TestData.FilePath);

            fileAddedToImportLogEvent.FileImportLogId.ShouldBe(TestData.FileImportLogId);
            fileAddedToImportLogEvent.FileId.ShouldBe(TestData.FileId);
            fileAddedToImportLogEvent.EstateId.ShouldBe(TestData.EstateId);
            fileAddedToImportLogEvent.MerchantId.ShouldBe(TestData.MerchantId);
            fileAddedToImportLogEvent.UserId.ShouldBe(TestData.UserId);
            fileAddedToImportLogEvent.FileProfileId.ShouldBe(TestData.FileProfileId);
            fileAddedToImportLogEvent.OriginalFileName.ShouldBe(TestData.OriginalFileName);
            fileAddedToImportLogEvent.FilePath.ShouldBe(TestData.FilePath);
        }
    }
}