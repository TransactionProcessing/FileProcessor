namespace FileProcessor.IntegrationTesting.Helpers.FileProcessor.IntegrationTests.Steps;

using System.Text;
using DataTransferObjects;
using DataTransferObjects.Responses;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Shared.IntegrationTesting;
using Shouldly;
using TechTalk.SpecFlow;

public static class SpecflowExtensions
{
    public static List<(DateTime, Int32)> ToExpectedImportLogData(this TableRows tableRows)
    {
        List<(DateTime, Int32)> results = new List<(DateTime, Int32)>();
        foreach (TableRow tableRow in tableRows)
        {
            DateTime importLogDateTime = SpecflowTableHelper.GetDateForDateString(SpecflowTableHelper.GetStringRowValue(tableRow, "ImportLogDate"), DateTime.Now);
            Int32 fileCount = SpecflowTableHelper.GetIntValue(tableRow, "FileCount");
            results.Add((importLogDateTime, fileCount));
        }

        return results;
    }

    public static String ToFileData(this TableRows tableRows)
    {
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

    public static (EstateDetails1, UploadFileRequest) ToUploadFileRequest(this TableRows tableRows, List<EstateDetails1> estateDetailsList, Byte[] fileData)
    {
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

    public static List<(String, Guid)> ToFileDetails(this TableRows tableRows, String estateName, List<EstateDetails1> estateDetailsList)
    {
        List<(String, Guid)> results = new List<(String, Guid)>();
        foreach (TableRow tableRow in tableRows)
        {
            //| MerchantName    | OriginalFileName | NumberOfLines |
            String merchantName = SpecflowTableHelper.GetStringRowValue(tableRow, "MerchantName");
            String originalFileName = SpecflowTableHelper.GetStringRowValue(tableRow, "OriginalFileName");

            EstateDetails1 estate = estateDetailsList.SingleOrDefault(e => e.EstateDetails.EstateName == estateName);
            Guid merchantId = estate.EstateDetails.GetMerchantId(merchantName);
            results.Add((originalFileName, merchantId));
        }
        return results;
    }

    public static FileProcessingSummary ToFileProcessingSummary(this TableRows tableRows)
    {
        tableRows.Count.ShouldBe(1);
        TableRow tableRow = tableRows.First();
        FileProcessingSummary summary = new FileProcessingSummary{
                                                                     ProcessingCompleted = SpecflowTableHelper.GetBooleanValue(tableRow, "ProcessingCompleted"),
                                                                     NumberOfLines = SpecflowTableHelper.GetIntValue(tableRow, "NumberOfLines"),
                                                                     TotaLines = SpecflowTableHelper.GetIntValue(tableRow, "TotaLines"),
                                                                     SuccessfulLines = SpecflowTableHelper.GetIntValue(tableRow, "SuccessfulLines"),
                                                                     IgnoredLines = SpecflowTableHelper.GetIntValue(tableRow, "IgnoredLines"),
                                                                     FailedLines = SpecflowTableHelper.GetIntValue(tableRow, "FailedLines"),
                                                                     NotProcessedLines = SpecflowTableHelper.GetIntValue(tableRow, "NotProcessedLines")
                                                                 };
        return summary;
    }

    public static List<FileLineDetails> ToFileLineDetails(this TableRows tableRows){
        List< FileLineDetails > results = new List< FileLineDetails >();
        foreach (TableRow tableRow in tableRows){
            FileLineDetails fileLineDetails = new FileLineDetails();
            fileLineDetails.LineNumber = SpecflowTableHelper.GetIntValue(tableRow, "LineNumber");
            fileLineDetails.LineData = SpecflowTableHelper.GetStringRowValue(tableRow, "Data");
            String processingResultString = SpecflowTableHelper.GetStringRowValue(tableRow, "Result");
            fileLineDetails.ProcessingResult = Enum.Parse<FileLineProcessingResult>(processingResultString, true);
            results.Add(fileLineDetails);
        }

        return results;
    }

    public class FileLineDetails{
        public Int32 LineNumber{ get; set; }
        public String LineData{ get; set; }
        public FileLineProcessingResult ProcessingResult{ get; set; }
    }

    public class FileProcessingSummary{
        public Boolean ProcessingCompleted {get; set; }
        public Int32 NumberOfLines { get; set; }
        public Int32 TotaLines { get; set; }
        public Int32 SuccessfulLines { get; set; }
        public Int32 IgnoredLines { get; set; }
        public Int32 FailedLines { get; set; }
        public Int32 NotProcessedLines { get; set; }
    }
}