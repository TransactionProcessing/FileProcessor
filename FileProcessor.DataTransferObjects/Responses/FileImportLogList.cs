using System.ComponentModel;
namespace FileProcessor.DataTransferObjects.Responses
{
    using System.Collections.Generic;

    /// <summary>A paginated collection of file import logs.</summary>
    [Description("A paginated collection of file import logs.")]
    public class FileImportLogList
    {
        /// <summary>Gets or sets the import logs on the current page.</summary>
        [Description("The import logs on the current page.")]
        public List<FileImportLog> FileImportLogs { get; set; }

        /// <summary>Gets or sets the one-based current page number.</summary>
        [Description("The one-based current page number.")]
        public int PageNumber { get; set; }

        /// <summary>Gets or sets the maximum number of results requested per page.</summary>
        [Description("The maximum number of results requested per page.")]
        public int PageSize { get; set; }

        /// <summary>Gets or sets the total number of matching logs across all pages.</summary>
        [Description("The total number of matching logs across all pages.")]
        public int TotalCount { get; set; }

        /// <summary>Gets or sets whether additional matching results are available on a later page.</summary>
        [Description("Whether additional matching results are available on a later page.")]
        public bool HasMore { get; set; }
    }
}
