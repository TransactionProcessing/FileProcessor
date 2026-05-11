using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Shared.Results;

namespace FileProcessor.Client {
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using ClientProxyBase;
    using DataTransferObjects;
    using DataTransferObjects.Responses;
    using SimpleResults;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="ClientProxyBase" />
    /// <seealso cref="FileProcessor.Client.IFileProcessorClient" />
    public class FileProcessorClient : ClientProxyBase, IFileProcessorClient {
        #region Fields

        /// <summary>
        /// The base address resolver
        /// </summary>
        private readonly Func<String, String> BaseAddressResolver;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FileProcessorClient"/> class.
        /// </summary>
        /// <param name="baseAddressResolver">The base address resolver.</param>
        /// <param name="httpClient">The HTTP client.</param>
        public FileProcessorClient(Func<String, String> baseAddressResolver,
                                   HttpClient httpClient,
                                   Func<object, string> serialise,
                                   Func<string, Type, object> deserialise) : base(httpClient, serialise, deserialise)
        {
            this.BaseAddressResolver = baseAddressResolver;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the file.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="fileId">The file identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<Result<FileDetails>> GetFile(String accessToken,
                                               Guid estateId,
                                               Guid fileId,
                                               CancellationToken cancellationToken) {
            String requestUri = this.BuildRequestUrl($"/api/files/{fileId}?estateId={estateId}");

            try {
                Result<FileDetails> result = await this.Get<FileDetails>(requestUri, accessToken, cancellationToken);

                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                return result;
            }
            catch (Exception ex) {
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception($"Error getting file with Id {fileId}.", ex);

                throw exception;
            }
        }

        /// <summary>
        /// Gets the file import log.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        /// <param name="fileImportLogId">The file import log identifier.</param>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="merchantId">The merchant identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<Result<FileImportLog>> GetFileImportLog(String accessToken,
                                                          Guid fileImportLogId,
                                                          Guid estateId,
                                                          Guid? merchantId,
                                                          CancellationToken cancellationToken) {
            String requestUri = this.BuildRequestUrl($"/api/fileImportLogs/{fileImportLogId}?estateId={estateId}");

            if (merchantId.HasValue) {
                requestUri += $"&merchantId={merchantId}";
            }

            try {
                Result<FileImportLog> result = await this.Get<FileImportLog>(requestUri, accessToken, cancellationToken);

                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                return result;
            }
            catch (Exception ex) {
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception("Error getting file import log.", ex);

                throw exception;
            }
        }

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
        public async Task<Result<FileImportLogList>> GetFileImportLogs(String accessToken,
                                                               Guid estateId,
                                                               DateTime startDateTime,
                                                               DateTime endDateTime,
                                                               Guid? merchantId,
                                                               CancellationToken cancellationToken) {
            String requestUri =
                this.BuildRequestUrl(
                    $"/api/fileImportLogs?estateId={estateId}&startDateTime={startDateTime.Date:yyyy-MM-dd}&endDateTime={endDateTime.Date:yyyy-MM-dd}");

            if (merchantId.HasValue) {
                requestUri += $"&merchantId={merchantId}";
            }

            try {
                Result<FileImportLogList> result = await this.Get<FileImportLogList>(requestUri, accessToken, cancellationToken);

                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                return result;
            }
            catch (Exception ex) {
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception("Error getting list of file import logs.", ex);

                throw exception;
            }
        }

        /// <summary>
        /// Uploads the file.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="fileData">The file data.</param>
        /// <param name="uploadFileRequest">The upload file request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<Result<Guid>> UploadFile(String accessToken,
                                                   String fileName,
                                                   Byte[] fileData,
                                                   UploadFileRequest uploadFileRequest,
                                                   CancellationToken cancellationToken) {
            try {
                String requestUri = this.BuildRequestUrl("/api/files");
                
                List<(string fieldName, string data)> formFields = new();
                formFields.Add(("EstateId", uploadFileRequest.EstateId.ToString()));
                formFields.Add(("MerchantId", uploadFileRequest.MerchantId.ToString()));
                formFields.Add(("FileProfileId", uploadFileRequest.FileProfileId.ToString()));
                formFields.Add(("UserId", uploadFileRequest.UserId.ToString()));
                formFields.Add(("UploadDateTime", uploadFileRequest.UploadDateTime.ToString("yyyy-MM-dd HH:mm:ss")));
                
                Guid fileId = CreateGuidFromFileData(fileData);
                formFields.Add(("FileId", fileId.ToString()));

                Result result = await this.Post(requestUri, fileData, fileName, formFields, accessToken, cancellationToken);
                if (result.IsFailed)
                    return ResultHelpers.CreateFailure(result);

                return Result.Success(fileId);
            }
            catch (Exception ex) {
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception($"Error uploading file {fileName}.", ex);

                throw exception;
            }
        }

        private Guid CreateGuidFromFileData(Byte[] fileContents)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                //Generate hash from the key
                Byte[] bytes = sha256Hash.ComputeHash(fileContents);

                Byte[] j = bytes.Skip(Math.Max(0, bytes.Count() - 16)).ToArray(); //Take last 16

                //Create our Guid.
                return new Guid(j);
            }
        }

        /// <summary>
        /// Builds the request URL.
        /// </summary>
        /// <param name="route">The route.</param>
        /// <returns></returns>
        private String BuildRequestUrl(String route) {
            String baseAddress = this.BaseAddressResolver("FileProcessorApi");

            String requestUri = $"{baseAddress}{route}";

            return requestUri;
        }

        #endregion
    }
}