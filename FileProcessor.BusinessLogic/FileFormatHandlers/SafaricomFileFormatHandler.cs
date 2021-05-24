namespace FileProcessor.BusinessLogic.FileFormatHandlers
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="IFileFormatHandler" />
    public class SafaricomFileFormatHandler : IFileFormatHandler
    {
        /// <summary>
        /// Parses the file line.
        /// </summary>
        /// <param name="fileLine">The file line.</param>
        /// <returns></returns>
        /// <exception cref="InvalidDataException">
        /// Detail lines must begin with a D, this line begins with {fileLine.StartsWith("D")}
        /// or
        /// Detail lines must contain 3 fields (Identifier, Customer Acccount Number, Amount), this line contains {splitLine.Length} fields
        /// </exception>
        public Dictionary<String, String> ParseFileLine(String fileLine)
        {
            // Format is 
            // Identifier (H,D,T)
            // CustomerAccountNumber
            // Amount
            if (fileLine.StartsWith("D") == false)
            {
                throw new InvalidDataException($"Detail lines must begin with a D, this line begins with {fileLine.StartsWith("D")}");
            }

            String[] splitLine = fileLine.Split(",");

            if (splitLine.Length != 3)
            {
                throw new InvalidDataException($"Detail lines must contain 3 fields (Identifier, Customer Acccount Number, Amount), this line contains {splitLine.Length} fields");
            }

            // Now extract the required data
            String customerAccountNumber = splitLine[1].Trim();
            String topupAmount = splitLine[2].Trim();

            // Create the metadata
            return new Dictionary<String, String>
                   {
                       {"CustomerAccountNumber", customerAccountNumber},
                       {"Amount", topupAmount}
                   };
        }

        /// <summary>
        /// Files the line can be ignored.
        /// </summary>
        /// <param name="fileLine">The file line.</param>
        /// <returns></returns>
        public Boolean FileLineCanBeIgnored(String fileLine)
        {
            // this line is a header or trailer so can be ignored
            return (fileLine.StartsWith("H") || fileLine.StartsWith("T"));
        }
    }
}