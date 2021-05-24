namespace FileProcessor.BusinessLogic.FileFormatHandlers
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    public class VoucherFileFormatHandler : IFileFormatHandler
    {
        public Dictionary<String, String> ParseFileLine(String fileLine)
        {
            // Format is 
            // Identifier (H,D,T)
            // Voucher Issuer Name
            // EmailAddress/Mobile Number
            // Amount
            if (fileLine.StartsWith("D") == false)
            {
                throw new InvalidDataException($"Detail lines must begin with a D, this line begins with {fileLine.StartsWith("D")}");
            }

            String[] splitLine = fileLine.Split(",");

            if (splitLine.Length != 4)
            {
                throw new InvalidDataException($"Detail lines must contain 4 fields (Identifier, Voucher Issuer Name, EmailAddress/Mobile Number, Amount), this line contains {splitLine.Length} fields");
            }

            // Determine if we have a mobile number or email address
            Dictionary<String, String> metaDataDictionary = new Dictionary<String, String>();

            String customerIdentifier = splitLine[2].Trim();
            if (customerIdentifier.Contains("@"))
            {
                // this is an email address
                metaDataDictionary.Add("RecipientEmail", customerIdentifier);
            }
            else
            {
                // this is a mobile number
                metaDataDictionary.Add("RecipientMobile", customerIdentifier);
            }

            // Now extract other the required data
            String operatorName = splitLine[1].Trim();
            metaDataDictionary.Add("OperatorName", operatorName);

            String amount = splitLine[3].Trim();
            metaDataDictionary.Add("Amount", amount);
            

            // Create the metadata
            return metaDataDictionary;
        }

        public Boolean FileLineCanBeIgnored(String fileLine)
        {
            // this line is a header or trailer so can be ignored
            return (fileLine.StartsWith("H") || fileLine.StartsWith("T"));
        }
    }
}