namespace FileProcessor.Client
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using DataTransferObjects;
    using DataTransferObjects.Responses;

    /// <summary>
    /// 
    /// </summary>
    public interface IFileProcessorClient
    {
        #region Methods

        /// <summary>
        /// Gets the file.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="fileId">The file identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<FileDetails> GetFile(String accessToken,
                                  Guid estateId,
                                  Guid fileId,
                                  CancellationToken cancellationToken);

        /// <summary>
        /// Gets the file import log.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        /// <param name="fileImportLogId">The file import log identifier.</param>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="merchantId">The merchant identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<FileImportLog> GetFileImportLog(String accessToken,
                                             Guid fileImportLogId,
                                             Guid estateId,
                                             Guid? merchantId,
                                             CancellationToken cancellationToken);

        /// <summary>
        /// Gets the file import logs.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="startDateTime">The start date time.</param>
        /// <param name="endDateTime">The end date time.</param>
        /// <param name="merchantId">The merchant identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<FileImportLogList> GetFileImportLogs(String accessToken,
                                                  Guid estateId,
                                                  DateTime startDateTime,
                                                  DateTime endDateTime,
                                                  Guid? merchantId,
                                                  CancellationToken cancellationToken);

        /// <summary>
        /// Uploads the file.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="fileData">The file data.</param>
        /// <param name="uploadFileRequest">The upload file request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<Guid> UploadFile(String accessToken,
                              String fileName,
                              Byte[] fileData,
                              UploadFileRequest uploadFileRequest,
                              CancellationToken cancellationToken);

        #endregion
    }
}