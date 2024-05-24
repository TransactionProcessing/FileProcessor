namespace FileProcessor.IntegrationTesting.Helpers.FileProcessor.IntegrationTests.Steps;

using System.Text;
using DataTransferObjects;
using DataTransferObjects.Responses;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Reqnroll;
using Shared.IntegrationTesting;
using Shared.Logger;
using Shouldly;

public static class ReqnrollExtensions
{
    public static List<(DateTime, Int32)> ToExpectedImportLogData(this DataTableRows tableRows)
    {
        List<(DateTime, Int32)> results = new List<(DateTime, Int32)>();
        foreach (DataTableRow tableRow in tableRows)
        {
            DateTime importLogDateTime = ReqnrollTableHelper.GetDateForDateString(ReqnrollTableHelper.GetStringRowValue(tableRow, "ImportLogDate"), DateTime.Now);
            Int32 fileCount = ReqnrollTableHelper.GetIntValue(tableRow, "FileCount");
            results.Add((importLogDateTime, fileCount));
        }

        return results;
    }

    public static String ToFileData(this DataTableRows tableRows)
    {
        StringBuilder fileBuilder = new StringBuilder();

        Int32 currentRow = 1;
        foreach (DataTableRow row in tableRows)
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

    public static (EstateDetails1, UploadFileRequest) ToUploadFileRequest(this DataTableRows tableRows, List<EstateDetails1> estateDetailsList, Byte[] fileData)
    {
        tableRows.Count.ShouldBe(1); // We can only handle 1 row here
        var row = tableRows.First();
        string estateName = ReqnrollTableHelper.GetStringRowValue(row, "EstateName");
        string merchantName = ReqnrollTableHelper.GetStringRowValue(row, "MerchantName");
        string fileProfileId = ReqnrollTableHelper.GetStringRowValue(row, "FileProfileId");
        
        string userId = ReqnrollTableHelper.GetStringRowValue(row, "UserId");
        string uploadDateTime = ReqnrollTableHelper.GetStringRowValue(row, "UploadDateTime");

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
            uploadFileRequest.UploadDateTime = ReqnrollTableHelper.GetDateForDateString(uploadDateTime, DateTime.Now);
        }

        return (estate, uploadFileRequest);
    }

    public static List<(String, Guid)> ToFileDetails(this DataTableRows tableRows, String estateName, List<EstateDetails1> estateDetailsList)
    {
        List<(String, Guid)> results = new List<(String, Guid)>();
        foreach (DataTableRow tableRow in tableRows)
        {
            //| MerchantName    | OriginalFileName | NumberOfLines |
            String merchantName = ReqnrollTableHelper.GetStringRowValue(tableRow, "MerchantName");
            String originalFileName = ReqnrollTableHelper.GetStringRowValue(tableRow, "OriginalFileName");

            EstateDetails1 estate = estateDetailsList.SingleOrDefault(e => e.EstateDetails.EstateName == estateName);
            Guid merchantId = estate.EstateDetails.GetMerchantId(merchantName);
            results.Add((originalFileName, merchantId));
        }
        return results;
    }

    public static FileProcessingSummary ToFileProcessingSummary(this DataTableRows tableRows)
    {
        tableRows.Count.ShouldBe(1);
        DataTableRow tableRow = tableRows.First();
        FileProcessingSummary summary = new FileProcessingSummary{
                                                                     ProcessingCompleted = ReqnrollTableHelper.GetBooleanValue(tableRow, "ProcessingCompleted"),
                                                                     NumberOfLines = ReqnrollTableHelper.GetIntValue(tableRow, "NumberOfLines"),
                                                                     TotaLines = ReqnrollTableHelper.GetIntValue(tableRow, "TotaLines"),
                                                                     SuccessfulLines = ReqnrollTableHelper.GetIntValue(tableRow, "SuccessfulLines"),
                                                                     IgnoredLines = ReqnrollTableHelper.GetIntValue(tableRow, "IgnoredLines"),
                                                                     FailedLines = ReqnrollTableHelper.GetIntValue(tableRow, "FailedLines"),
                                                                     NotProcessedLines = ReqnrollTableHelper.GetIntValue(tableRow, "NotProcessedLines")
                                                                 };
        return summary;
    }

    public static List<FileLineDetails> ToFileLineDetails(this DataTableRows tableRows){
        List< FileLineDetails > results = new List< FileLineDetails >();
        foreach (DataTableRow tableRow in tableRows){
            FileLineDetails fileLineDetails = new FileLineDetails();
            fileLineDetails.LineNumber = ReqnrollTableHelper.GetIntValue(tableRow, "LineNumber");
            fileLineDetails.LineData = ReqnrollTableHelper.GetStringRowValue(tableRow, "Data");
            String processingResultString = ReqnrollTableHelper.GetStringRowValue(tableRow, "Result");
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