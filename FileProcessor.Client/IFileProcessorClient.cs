using SimpleResults;

namespace FileProcessor.Client
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using DataTransferObjects;
    using ApiFileDetails = DataTransferObjects.Responses.FileDetails;
    using ApiFileImportLog = DataTransferObjects.Responses.FileImportLog;
    using DataTransferObjects.Responses;
    using FileProcessor.Models;

    /// <summary>
    /// 
    /// </summary>
    public interface IFileProcessorClient
    {
        #region Methods

        Task<Result<List<FileProfile>>> GetFileProfiles(String accessToken,
                                                        CancellationToken cancellationToken);

        Task<Result<FileProfile>> GetFileProfile(String accessToken,
                                                 Guid fileProfileId,
                                                 CancellationToken cancellationToken);

        Task<Result<List<FileProfile>>> GetFileProfileVersions(String accessToken,
                                                               Guid fileProfileId,
                                                               CancellationToken cancellationToken);

        Task<Result<FileProfile>> GetFileProfileVersion(String accessToken,
                                                        Guid fileProfileId,
                                                        Guid fileProfileVersionId,
                                                        CancellationToken cancellationToken);

        Task<Result<FileProfile>> CreateFileProfile(String accessToken,
                                                    DataTransferObjects.Requests.CreateFileProfileRequest request,
                                                    CancellationToken cancellationToken);

        Task<Result<FileProfile>> UpdateFileProfile(String accessToken,
                                                    Guid fileProfileId,
                                                    DataTransferObjects.Requests.UpdateFileProfileRequest request,
                                                    CancellationToken cancellationToken);

        Task<Result> ArchiveFileProfile(String accessToken,
                                        Guid fileProfileId,
                                        CancellationToken cancellationToken);

        /// <summary>
        /// Gets the file.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="fileId">The file identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<Result<ApiFileDetails>> GetFile(String accessToken,
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
        Task<Result<ApiFileImportLog>> GetFileImportLog(String accessToken,
                                                        Guid fileImportLogId,
                                                        Guid estateId,
                                                        Guid? merchantId,
                                                        CancellationToken cancellationToken);

        /// <summary>
        /// Gets the file import logs.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="startDate">The first calendar date.</param>
        /// <param name="endDate">The last calendar date.</param>
        /// <param name="merchantId">The merchant identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<Result<FileImportLogList>> GetFileImportLogs(String accessToken,
                                                  Guid estateId,
                                                  DateOnly startDate,
                                                  DateOnly endDate,
                                                  Guid? merchantId,
                                                  Int32 pageNumber,
                                                  Int32 pageSize,
                                                  Boolean descending,
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
        Task<Result<Guid>> UploadFile(String accessToken,
                              String fileName,
                              Byte[] fileData,
                              UploadFileRequest uploadFileRequest,
                              CancellationToken cancellationToken);

        #endregion
    }
}
