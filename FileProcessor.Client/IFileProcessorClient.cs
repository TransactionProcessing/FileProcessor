using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileProcessor.Client
{
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading;
    using ClientProxyBase;
    using DataTransferObjects;
    using DataTransferObjects.Responses;
    using Newtonsoft.Json;

    public interface IFileProcessorClient
    {
        //GetImportLogs
        // GET api/fileImportLogs?estateId={estateId}&startDateTime={startDateTime}&endDateTime={endDateTime}&merchantId={merchantId}
        //GetImportLog
        // GET api/fileImportLogs/{fileImportLogId}?estateId={estateId}&merchantId={merchantId}

        Task<FileImportLogList> GetFileImportLogs(String accessToken, 
                                                  Guid estateId,
                                                  DateTime startDateTime,
                                                  DateTime endDateTime,
                                                  Guid? merchantId,
                                                  CancellationToken cancellationToken);

        Task<FileImportLog> GetFileImportLog(String accessToken, 
                                              Guid fileImportLogId, 
                                              Guid estateId,
                                              Guid? merchantId,
                                              CancellationToken cancellationToken);

        Task UploadFile(String accessToken, UploadFileRequest uploadFileRequest, CancellationToken cancellation);

        Task<FileDetails> GetFile(String accessToken, Guid fileId, CancellationToken cancellationToken);
    }

    public class FileProcessorClient : ClientProxyBase, IFileProcessorClient
    {
        private readonly Func<String, String> BaseAddressResolver;

        public async Task<FileImportLogList> GetFileImportLogs(String accessToken,
                                                               Guid estateId,
                                                               DateTime startDateTime,
                                                               DateTime endDateTime,
                                                               Guid? merchantId,
                                                               CancellationToken cancellationToken)
        {
            FileImportLogList response = null;

            String requestUri = this.BuildRequestUrl($"/api/fileImportLogs?estateId={estateId}&startDateTime={startDateTime.Date:yyyy-MM-dd}&endDateTime={endDateTime.Date:yyyy-MM-dd}");

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
            catch (Exception ex)
            {
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception($"Error getting list of file import logs.", ex);

                throw exception;
            }

            return response;
        }

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
            catch (Exception ex)
            {
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception($"Error getting file import log.", ex);

                throw exception;
            }

            return response;
        }

        public async Task UploadFile(String accessToken,
                                     UploadFileRequest uploadFileRequest,
                                     CancellationToken cancellation)
        {
        }

        public async Task<FileDetails> GetFile(String accessToken,
                                               Guid fileId,
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
            catch (Exception ex)
            {
                // An exception has occurred, add some additional information to the message
                Exception exception = new Exception($"Error getting file import log.", ex);

                throw exception;
            }
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

        public FileProcessorClient(Func<String, String> baseAddressResolver, HttpClient httpClient) : base(httpClient)
        {
            this.BaseAddressResolver = baseAddressResolver;

            // Add the API version header
            this.HttpClient.DefaultRequestHeaders.Add("api-version", "1.0");
        }
    }
}
