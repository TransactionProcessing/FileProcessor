namespace FileProcessor.BusinessLogic.FileFormatHandlers
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// 
    /// </summary>
    public interface IFileFormatHandler
    {
        /// <summary>
        /// Parses the file line.
        /// </summary>
        /// <param name="fileLine">The file line.</param>
        /// <returns></returns>
        Dictionary<String, String> ParseFileLine(String fileLine);

        /// <summary>
        /// Files the line can be ignored.
        /// </summary>
        /// <param name="fileLine">The file line.</param>
        /// <returns></returns>
        Boolean FileLineCanBeIgnored(String fileLine);
    }
}