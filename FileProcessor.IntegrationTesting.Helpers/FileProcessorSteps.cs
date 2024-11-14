namespace FileProcessor.IntegrationTesting.Helpers.FileProcessor.IntegrationTests.Steps;

using Client;
using DataTransferObjects;
using DataTransferObjects.Responses;
using Shared.IntegrationTesting;
using Shouldly;

public class FileProcessorSteps
{
    private readonly IFileProcessorClient FileProcessorClient;

    public FileProcessorSteps(IFileProcessorClient fileProcessorClient)
    {
        this.FileProcessorClient = fileProcessorClient;
    }

    public String WriteDileToDisk(String fileName, String fileData)
    {
        Directory.CreateDirectory("/home/runner/specflow");
        String filepath = $"/home/runner/specflow/{fileName}";
        // Should have the whole file here
        using (StreamWriter sw = new StreamWriter(filepath))
        {
            sw.WriteAsync(fileData);
        }

        return filepath;
    }

    public async Task<Guid> GivenIUploadThisFileForProcessing(String accessToken, String filePath, Byte[] fileData, EstateDetails1 estateDetails, UploadFileRequest uploadFileRequest)
    {
        Guid fileId = await this.FileProcessorClient.UploadFile(accessToken,
                                                                Path.GetFileName(filePath),
                                                                fileData,
                                                                uploadFileRequest,
                                                                CancellationToken.None);

        fileId.ShouldNotBe(Guid.Empty);

        await Retry.For(async () =>
                        {
                            FileDetails fileDetails =
                                await this.FileProcessorClient.GetFile(accessToken, estateDetails.EstateDetails.EstateId, fileId, CancellationToken.None);

                            fileDetails.ShouldNotBeNull();
                            fileDetails.ProcessingCompleted.ShouldBeTrue();
                        }, TimeSpan.FromMinutes(5),
                        TimeSpan.FromSeconds(30));

        return fileId;
    }

    public async Task GivenIUploadThisFileForProcessingAnErrorShouldBeReturnedIndicatingTheFileIsADuplicate(String accessToken,
                                                                                                            String filePath,
                                                                                                            Byte[] fileData,
                                                                                                            UploadFileRequest uploadFileRequest) {
        var result = await this.FileProcessorClient.UploadFile(accessToken, Path.GetFileName(filePath), fileData, uploadFileRequest, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    public async Task WhenIGetTheImportLogsBetweenAndTheFollowingDataIsReturned(String accessToken, String startDateString, String endDateString, String estateName, List<EstateDetails1> estateDetailsList, List<(DateTime, Int32)> expectedImportLogs)
    {
        DateTime queryStartDate = ReqnrollTableHelper.GetDateForDateString(startDateString, DateTime.Now);
        DateTime queryEndDate = ReqnrollTableHelper.GetDateForDateString(endDateString, DateTime.Now);
        EstateDetails1 estate = estateDetailsList.SingleOrDefault(e => e.EstateDetails.EstateName == estateName);
        estate.ShouldNotBeNull();

        await Retry.For(async () => {
                            FileImportLogList importLogList = await this.FileProcessorClient.GetFileImportLogs(accessToken,
                                                                                                               estate.EstateDetails.EstateId,
                                                                                                               queryStartDate,
                                                                                                               queryEndDate,
                                                                                                               null,
                                                                                                               CancellationToken.None);
                            importLogList.ShouldNotBeNull();
                            importLogList.FileImportLogs.ShouldNotBeNull();
                            importLogList.FileImportLogs.ShouldNotBeEmpty();

                            foreach ((DateTime, Int32) expectedImportLog in expectedImportLogs)
                            {
                                // Find the import log now
                                FileImportLog? importLog = importLogList.FileImportLogs.SingleOrDefault(fil => fil.ImportLogDate == expectedImportLog.Item1.Date && fil.FileCount == expectedImportLog.Item2);
                                importLog.ShouldNotBeNull();
                            }
                        });
    }

    public async Task WhenIGetTheImportLogForTheFollowingFileInformationIsReturned(String accessToken, String estateName, String startDate, List<EstateDetails1> estateDetailsList, List<(String, Guid)> expectedFiles)
    {
        EstateDetails1 estate = estateDetailsList.SingleOrDefault(e => e.EstateDetails.EstateName == estateName);
        estate.ShouldNotBeNull();

        DateTime queryStartDate = ReqnrollTableHelper.GetDateForDateString(startDate, DateTime.Now);
        FileImportLogList importLogList = await this.FileProcessorClient.GetFileImportLogs(accessToken,
                                                                                           estate.EstateDetails.EstateId,
                                                                                           queryStartDate,
                                                                                           queryStartDate,
                                                                                           null,
                                                                                           CancellationToken.None);
        importLogList.ShouldNotBeNull();
        importLogList.FileImportLogs.ShouldHaveSingleItem();

        Guid fileImportLogId = importLogList.FileImportLogs.Single().FileImportLogId;
        FileImportLog fileImportLog = await this.FileProcessorClient.GetFileImportLog(accessToken,
                                                                                      fileImportLogId,
                                                                                      estate.EstateDetails.EstateId,
                                                                                      null,
                                                                                      CancellationToken.None);
        fileImportLog.ShouldNotBeNull();
        fileImportLog.Files.ShouldNotBeNull();
        fileImportLog.Files.ShouldNotBeEmpty();

        foreach ((String, Guid) expectedFile in expectedFiles)
        {
            FileImportLogFile file = fileImportLog.Files.SingleOrDefault(f => f.OriginalFileName == expectedFile.Item1 && f.MerchantId == expectedFile.Item2);

            file.ShouldNotBeNull();
            estate.AddFileImportLogFile(file);
        }
    }

    public async Task WhenIGetTheImportLogForEstateTheDateOnTheImportLogIs(String accessToken, String estateName, String expectedDateString, List<EstateDetails1> estateDetailsList)
    {

        EstateDetails1 estate = estateDetailsList.SingleOrDefault(e => e.EstateDetails.EstateName == estateName);
        estate.ShouldNotBeNull();

        DateTime expectedDate = ReqnrollTableHelper.GetDateForDateString(expectedDateString, DateTime.Now);

        FileImportLogList? importLogs = await this.FileProcessorClient.GetFileImportLogs(accessToken,
                                                                                         estate.EstateDetails.EstateId,
                                                                                         expectedDate.AddDays(-1),
                                                                                         DateTime.Now,
                                                                                         null,
                                                                                         CancellationToken.None);

        FileImportLog i = importLogs.FileImportLogs.SingleOrDefault(x => x.ImportLogDate == expectedDate.Date);
        i.ShouldNotBeNull();
    }

    public async Task WhenIGetTheFileForEstateTheFollowingFileInformationIsReturned(String accessToken, String fileName, String estateName, List<EstateDetails1> estateDetailsList, ReqnrollExtensions.FileProcessingSummary fileSummary){
        EstateDetails1 estate = estateDetailsList.SingleOrDefault(e => e.EstateDetails.EstateName == estateName);
        estate.ShouldNotBeNull();

        Guid fileId = estate.GetFileId(fileName);

        await Retry.For(async () =>
                        {
                            FileDetails fileDetails = await this.FileProcessorClient.GetFile(accessToken, estate.EstateDetails.EstateId, fileId, CancellationToken.None);
                            fileDetails.ShouldNotBeNull();

                            fileDetails.ProcessingCompleted.ShouldBe(fileSummary.ProcessingCompleted);
                            fileDetails.FileLines.Count.ShouldBe(fileSummary.NumberOfLines);
                            fileDetails.ProcessingSummary.TotalLines.ShouldBe(fileSummary.TotaLines);
                            fileDetails.ProcessingSummary.SuccessfullyProcessedLines.ShouldBe(fileSummary.SuccessfulLines);
                            fileDetails.ProcessingSummary.IgnoredLines.ShouldBe(fileSummary.IgnoredLines);
                            fileDetails.ProcessingSummary.FailedLines.ShouldBe(fileSummary.FailedLines);
                            fileDetails.ProcessingSummary.NotProcessedLines.ShouldBe(fileSummary.NotProcessedLines);
                        }, TimeSpan.FromMinutes(4), TimeSpan.FromSeconds(30));
    }

    public async Task WhenIGetTheFileForEstateTheFollowingFileLinesAreReturned(String accessToken, String fileName, String estateName, List<EstateDetails1> estateDetailsList, List<ReqnrollExtensions.FileLineDetails> fileLineDetails){
        EstateDetails1 estate = estateDetailsList.SingleOrDefault(e => e.EstateDetails.EstateName == estateName);
        estate.ShouldNotBeNull();

        Guid fileId = estate.GetFileId(fileName);

        await Retry.For(async () => {
                            FileDetails fileDetails = await this.FileProcessorClient.GetFile(accessToken, estate.EstateDetails.EstateId, fileId, CancellationToken.None);
                            fileDetails.ShouldNotBeNull();

                            foreach (ReqnrollExtensions.FileLineDetails fileLineDetail in fileLineDetails){

                                FileLine? lineToVerify = fileDetails.FileLines.SingleOrDefault(fl => fl.LineNumber == fileLineDetail.LineNumber);
                                lineToVerify.ShouldNotBeNull();
                                lineToVerify.LineData.ShouldBe(fileLineDetail.LineData);
                                lineToVerify.ProcessingResult.ShouldBe(fileLineDetail.ProcessingResult);
                            }
                        },
                        TimeSpan.FromMinutes(4),
                        TimeSpan.FromSeconds(30));
    }
}