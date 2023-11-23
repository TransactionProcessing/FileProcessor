using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileProcessor.IntegrationTests.Steps
{
    using Ductus.FluentDocker.Builders;
    using Gherkin.Ast;
    using Microsoft.EntityFrameworkCore.Metadata.Internal;
    using System.IO;
    using System.Threading;
    using Client;
    using Common;
    using DataTransferObjects;
    using DataTransferObjects.Responses;
    using EstateManagement.IntegrationTesting.Helpers;
    using Shared.IntegrationTesting;
    using Shouldly;
    using TechTalk.SpecFlow;
    using SpecflowTableHelper = Shared.IntegrationTesting.SpecflowTableHelper;
    using Gherkin.CucumberMessages.Types;
    using Table = Microsoft.EntityFrameworkCore.Metadata.Internal.Table;
    using TableRow = TechTalk.SpecFlow.TableRow;
    using Azure.Core;
    /*
    public static class SpecflowExtensions
    {
        public static List<(DateTime, Int32)> ToExpectedImportLogData(this TableRows tableRows){
            List<(DateTime, Int32)> results = new List<(DateTime, Int32)>();
            foreach (TableRow tableRow in tableRows){
                DateTime importLogDateTime = SpecflowTableHelper.GetDateForDateString(SpecflowTableHelper.GetStringRowValue(tableRow, "ImportLogDate"), DateTime.Now);
                Int32 fileCount = SpecflowTableHelper.GetIntValue(tableRow, "FileCount");
                results.Add((importLogDateTime, fileCount));
            }

            return results;
        }

        public static String ToFileData(this TableRows tableRows){
            StringBuilder fileBuilder = new StringBuilder();

            Int32 currentRow = 1;
            foreach (var row in tableRows)
            {
                StringBuilder rowBuilder = new StringBuilder();
                foreach (String rowValue in row.Values)
                {
                    rowBuilder.Append($"{rowValue},");
                }
                // remove the trailing comma
                rowBuilder.Remove(rowBuilder.Length - 1, 1);
                if (currentRow < tableRows.Count)
                {
                    rowBuilder.Append("\n");
                }

                fileBuilder.Append(rowBuilder.ToString());
                currentRow++;
            }
            return fileBuilder.ToString();
        }

        public static (EstateDetails1, UploadFileRequest) ToUploadFileRequest(this TableRows tableRows, List<EstateDetails1> estateDetailsList, Byte[] fileData){
            tableRows.Count.ShouldBe(1); // We can only handle 1 row here
            var row = tableRows.First();
            string estateName = SpecflowTableHelper.GetStringRowValue(row, "EstateName");
            string merchantName = SpecflowTableHelper.GetStringRowValue(row, "MerchantName");
            string fileProfileId = SpecflowTableHelper.GetStringRowValue(row, "FileProfileId");
            string userId = SpecflowTableHelper.GetStringRowValue(row, "UserId");
            string uploadDateTime = SpecflowTableHelper.GetStringRowValue(row, "UploadDateTime");

            EstateDetails1 estate = estateDetailsList.SingleOrDefault(e => e.EstateDetails.EstateName == estateName);
            estate.ShouldNotBeNull();
            Guid estateId = estate.EstateDetails.EstateId;
            var merchantId = estate.EstateDetails.GetMerchantId(merchantName);

            UploadFileRequest uploadFileRequest = new UploadFileRequest
                                                  {
                                                      EstateId = estateId,
                                                      FileProfileId = Guid.Parse(fileProfileId),
                                                      MerchantId = merchantId,
                                                      UserId = Guid.Parse(userId),
                                                  };

            if (string.IsNullOrEmpty(uploadDateTime) == false)
            {
                uploadFileRequest.UploadDateTime = SpecflowTableHelper.GetDateForDateString(uploadDateTime, DateTime.Now);
            }

            return (estate, uploadFileRequest);
        }

        public static List<(String,Guid)> ToFileDetails(this TableRows tableRows, String estateName, List<EstateDetails1> estateDetailsList)
        {
            List < (String, Guid) > results = new List<(String, Guid) >();
            foreach (TableRow tableRow in tableRows){
                //| MerchantName    | OriginalFileName | NumberOfLines |
                String merchantName = SpecflowTableHelper.GetStringRowValue(tableRow, "MerchantName");
                String originalFileName = SpecflowTableHelper.GetStringRowValue(tableRow, "OriginalFileName");
                
                EstateDetails1 estate = estateDetailsList.SingleOrDefault(e => e.EstateDetails.EstateName == estateName);
                Guid merchantId = estate.EstateDetails.GetMerchantId(merchantName);
                results.Add((originalFileName, merchantId));
            }
            return results;
        }
    }

    public class FileProcessorSteps{
        private readonly IFileProcessorClient FileProcessorClient;

        public FileProcessorSteps(IFileProcessorClient fileProcessorClient){
            this.FileProcessorClient = fileProcessorClient;
        }

        public String WriteDileToDisk(String fileName, String fileData){
            Directory.CreateDirectory("/home/runner/specflow");
            String filepath = $"/home/runner/specflow/{fileName}";
            // Should have the whole file here
            using (StreamWriter sw = new StreamWriter(filepath))
            {
                sw.WriteAsync(fileData);
            }

            return filepath;
        }

        public async Task<Guid> GivenIUploadThisFileForProcessing(String accessToken, String filePath, Byte[] fileData, EstateDetails1 estateDetails, UploadFileRequest uploadFileRequest){
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
                            }, TimeSpan.FromMinutes(3));

            return fileId;
        }

        public async Task GivenIUploadThisFileForProcessingAnErrorShouldBeReturnedIndicatingTheFileIsADuplicate(String accessToken, String filePath, Byte[] fileData, UploadFileRequest uploadFileRequest){
            Should.Throw<Exception>(async () => {
                                        await this.FileProcessorClient.UploadFile(accessToken,
                                                                                  Path.GetFileName(filePath),
                                                                                  fileData,
                                                                                  uploadFileRequest,
                                                                                  CancellationToken.None);
                                    });
        }

        public async Task WhenIGetTheImportLogsBetweenAndTheFollowingDataIsReturned(String accessToken, String startDateString, String endDateString, String estateName, List<EstateDetails1> estateDetailsList, List<(DateTime, Int32)> expectedImportLogs){
            DateTime queryStartDate = SpecflowTableHelper.GetDateForDateString(startDateString, DateTime.Now);
            DateTime queryEndDate = SpecflowTableHelper.GetDateForDateString(endDateString, DateTime.Now);
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

                                foreach ((DateTime, Int32) expectedImportLog in expectedImportLogs){
                                    // Find the import log now
                                    FileImportLog? importLog = importLogList.FileImportLogs.SingleOrDefault(fil => fil.ImportLogDate == expectedImportLog.Item1.Date && fil.FileCount == expectedImportLog.Item2);
                                    importLog.ShouldNotBeNull();
                                }
                            });
        }

        public async Task WhenIGetTheImportLogForTheFollowingFileInformationIsReturned(String accessToken, String estateName, String startDate, List<EstateDetails1> estateDetailsList, List<(String, Guid)> expectedFiles){
            EstateDetails1 estate = estateDetailsList.SingleOrDefault(e => e.EstateDetails.EstateName == estateName);
            estate.ShouldNotBeNull();

            DateTime queryStartDate = SpecflowTableHelper.GetDateForDateString(startDate, DateTime.Now);
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

            foreach ((String, Guid) expectedFile in expectedFiles){
                FileImportLogFile file = fileImportLog.Files.SingleOrDefault(f => f.OriginalFileName == expectedFile.Item1 && f.MerchantId == expectedFile.Item2);

                file.ShouldNotBeNull();
                // TODO: cache the list
                //estate.AddFileImportLogFile(file);
            }
        }

        public async Task WhenIGetTheImportLogForEstateTheDateOnTheImportLogIs(String accessToken, String estateName, String expectedDateString, List<EstateDetails1> estateDetailsList){

            EstateDetails1 estate = estateDetailsList.SingleOrDefault(e => e.EstateDetails.EstateName == estateName);
            estate.ShouldNotBeNull();

            DateTime expectedDate = SpecflowTableHelper.GetDateForDateString(expectedDateString, DateTime.Now);

            var importLogs = await this.FileProcessorClient.GetFileImportLogs(accessToken,
                                                                                   estate.EstateDetails.EstateId,
                                                                                   expectedDate.AddDays(-1),
                                                                                   DateTime.Now,
                                                                                   null,
                                                                                   CancellationToken.None);

            FileImportLog i = importLogs.FileImportLogs.SingleOrDefault(x => x.ImportLogDate == expectedDate.Date);
            i.ShouldNotBeNull();
        }
    }*/
}
