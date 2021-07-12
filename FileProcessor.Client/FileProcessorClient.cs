namespace FileProcessor.Client
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;
    using ClientProxyBase;
    using DataTransferObjects;
    using DataTransferObjects.Responses;
    using Newtonsoft.Json;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="ClientProxyBase.ClientProxyBase" />
    /// <seealso cref="FileProcessor.Client.IFileProcessorClient" />
    public class FileProcessorClient : ClientProxyBase, IFileProcessorClient
    {
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
                                   HttpClient httpClient) : base(httpClient)
        {
            this.BaseAddressResolver = baseAddressResolver;

            // Add the API version header
            this.HttpClient.DefaultRequestHeaders.Add("api-version", "1.0");
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
        public async Task<FileDetails> GetFile(String accessToken,
                                               Guid estateId,
                                               Guid fileId,
                                               CancellationToken cancellationToken)
        {
            FileDetails response = null;

            String requestUri = this.BuildRequestUrl($"/api/files/{fileId}?estateId={estateId}");

            try
            {
                // Make the Http Call here
                HttpResponseMessage httpResponse = await this.HttpClient.GetAsync(requestUri, cancellationToken);

                // Process the response
                String content = await this.HandleResponse(httpResponse, cancellationToken);

                // call was successful so now deserialise the body to the response object
                response = JsonConvert.DeserializeObject<FileDetails>(content);
            }
            catch(Exception ex)
            {
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception($"Error getting file with Id {fileId}.", ex);

                throw exception;
            }

            return response;
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
        public async Task<FileImportLog> GetFileImportLog(String accessToken,
                                                          Guid fileImportLogId,
                                                          Guid estateId,
                                                          Guid? merchantId,
                                                          CancellationToken cancellationToken)
        {
            FileImportLog response = null;

            String requestUri = this.BuildRequestUrl($"/api/fileImportLogs/{fileImportLogId}?estateId={estateId}");

            if (merchantId.HasValue)
            {
                requestUri += $"&merchantId={merchantId}";
            }

            try
            {
                // Make the Http Call here
                HttpResponseMessage httpResponse = await this.HttpClient.GetAsync(requestUri, cancellationToken);

                // Process the response
                String content = await this.HandleResponse(httpResponse, cancellationToken);

                // call was successful so now deserialise the body to the response object
                response = JsonConvert.DeserializeObject<FileImportLog>(content);
            }
            catch(Exception ex)
            {
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception("Error getting file import log.", ex);

                throw exception;
            }

            return response;
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
        public async Task<FileImportLogList> GetFileImportLogs(String accessToken,
                                                               Guid estateId,
                                                               DateTime startDateTime,
                                                               DateTime endDateTime,
                                                               Guid? merchantId,
                                                               CancellationToken cancellationToken)
        {
            FileImportLogList response = null;

            String requestUri =
                this.BuildRequestUrl($"/api/fileImportLogs?estateId={estateId}&startDateTime={startDateTime.Date:yyyy-MM-dd}&endDateTime={endDateTime.Date:yyyy-MM-dd}");

            if (merchantId.HasValue)
            {
                requestUri += $"&merchantId={merchantId}";
            }

            try
            {
                // Make the Http Call here
                HttpResponseMessage httpResponse = await this.HttpClient.GetAsync(requestUri, cancellationToken);

                // Process the response
                String content = await this.HandleResponse(httpResponse, cancellationToken);

                // call was successful so now deserialise the body to the response object
                response = JsonConvert.DeserializeObject<FileImportLogList>(content);
            }
            catch(Exception ex)
            {
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception("Error getting list of file import logs.", ex);

                throw exception;
            }

            return response;
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
        public async Task<Guid> UploadFile(String accessToken,
                                           String fileName,
                                           Byte[] fileData,
                                           UploadFileRequest uploadFileRequest,
                                           CancellationToken cancellationToken)
        {
            Guid response = Guid.Empty;
            try
            {
                String requestUri = this.BuildRequestUrl("api/files");

                HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUri);
                MultipartFormDataContent formData = new MultipartFormDataContent();

                ByteArrayContent fileContent = new ByteArrayContent(fileData);
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
                formData.Add(fileContent, "file", fileName);
                formData.Add(new StringContent(uploadFileRequest.EstateId.ToString()), "request.EstateId");
                formData.Add(new StringContent(uploadFileRequest.MerchantId.ToString()), "request.MerchantId");
                formData.Add(new StringContent(uploadFileRequest.FileProfileId.ToString()), "request.FileProfileId");
                formData.Add(new StringContent(uploadFileRequest.UserId.ToString()), "request.UserId");

                httpRequest.Content = formData;
                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                HttpResponseMessage httpResponse = await this.HttpClient.SendAsync(httpRequest, cancellationToken);

                // Process the response
                String content = await this.HandleResponse(httpResponse, cancellationToken);

                // call was successful so now deserialise the body to the response object
                response = JsonConvert.DeserializeObject<Guid>(content);
            }
            catch(Exception ex)
            {
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception($"Error uploading file {fileName}.", ex);

                throw exception;
            }

            return response;
        }

        /// <summary>
        /// Builds the request URL.
        /// </summary>
        /// <param name="route">The route.</param>
        /// <returns></returns>
        private String BuildRequestUrl(String route)
        {
            String baseAddress = this.BaseAddressResolver("FileProcessorApi");

            String requestUri = $"{baseAddress}{route}";

            return requestUri;
        }

        #endregion
    }
}