using System.Linq;
using System.Security.Cryptography;
using Shared.Results;

namespace FileProcessor.Client {
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using ClientProxyBase;
    using DataTransferObjects;
    using DataTransferObjects.Requests;
    using ApiFileDetails = DataTransferObjects.Responses.FileDetails;
    using ApiFileImportLog = DataTransferObjects.Responses.FileImportLog;
    using DataTransferObjects.Responses;
    using FileProcessor.Models;
    using SimpleResults;

    public enum SortDirection {
        Ascending,
        Descending,
    }
    public record PagingRequest(Int32 PageNumber, Int32 PageSize, SortDirection Direction);

    public interface IFileProcessorClient {

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

        Task<Result<ApiFileDetails>> GetFile(String accessToken,
                                             Guid estateId,
                                             Guid fileId,
                                             CancellationToken cancellationToken);

        /// <returns></returns>
        Task<Result<ApiFileImportLog>> GetFileImportLog(String accessToken,
                                                        Guid fileImportLogId,
                                                        Guid estateId,
                                                        Guid? merchantId,
                                                        CancellationToken cancellationToken);

        Task<Result<FileImportLogList>> GetFileImportLogs(String accessToken,
                                                          Guid estateId,
                                                          DateOnly startDate,
                                                          DateOnly endDate,
                                                          Guid? merchantId,
                                                          PagingRequest pagingRequest,
                                                          CancellationToken cancellationToken);

        Task<Result<Guid>> UploadFile(String accessToken,
                                      String fileName,
                                      Byte[] fileData,
                                      UploadFileRequest uploadFileRequest,
                                      CancellationToken cancellationToken);

    }

    public class FileProcessorClient : ClientProxyBase, IFileProcessorClient {

        private readonly Func<String, String> BaseAddressResolver;

        public FileProcessorClient(Func<String, String> baseAddressResolver,
                                   HttpClient httpClient,
                                   Func<object, string> serialise,
                                   Func<string, Type, object> deserialise) : base(httpClient, serialise, deserialise) {
            this.BaseAddressResolver = baseAddressResolver;
        }

        #region Methods

        public async Task<Result<List<FileProfile>>> GetFileProfiles(String accessToken,
                                                                     CancellationToken cancellationToken)
        {
            String requestUri = this.BuildRequestUrl("/api/file-profiles");
            return await this.Get<List<FileProfile>>(requestUri, accessToken, null, cancellationToken);
        }

        public async Task<Result<FileProfile>> GetFileProfile(String accessToken,
                                                              Guid fileProfileId,
                                                              CancellationToken cancellationToken)
        {
            String requestUri = this.BuildRequestUrl($"/api/file-profiles/{fileProfileId}");
            return await this.Get<FileProfile>(requestUri, accessToken, null, cancellationToken);
        }

        public async Task<Result<List<FileProfile>>> GetFileProfileVersions(String accessToken,
                                                                             Guid fileProfileId,
                                                                             CancellationToken cancellationToken)
        {
            String requestUri = this.BuildRequestUrl($"/api/file-profiles/{fileProfileId}/versions");
            return await this.Get<List<FileProfile>>(requestUri, accessToken, null, cancellationToken);
        }

        public async Task<Result<FileProfile>> GetFileProfileVersion(String accessToken,
                                                                      Guid fileProfileId,
                                                                      Guid fileProfileVersionId,
                                                                      CancellationToken cancellationToken)
        {
            String requestUri = this.BuildRequestUrl($"/api/file-profiles/{fileProfileId}/versions/{fileProfileVersionId}");
            return await this.Get<FileProfile>(requestUri, accessToken, null, cancellationToken);
        }

        public async Task<Result<FileProfile>> CreateFileProfile(String accessToken,
                                                                 CreateFileProfileRequest request,
                                                                 CancellationToken cancellationToken)
        {
            String requestUri = this.BuildRequestUrl("/api/file-profiles");
            return await this.Post<CreateFileProfileRequest, FileProfile>(requestUri, request, accessToken, cancellationToken);
        }

        public async Task<Result<FileProfile>> UpdateFileProfile(String accessToken,
                                                                 Guid fileProfileId,
                                                                 UpdateFileProfileRequest request,
                                                                 CancellationToken cancellationToken)
        {
            String requestUri = this.BuildRequestUrl($"/api/file-profiles/{fileProfileId}");
            return await this.Patch<UpdateFileProfileRequest>(requestUri, request, accessToken, cancellationToken);
        }

        public async Task<Result> ArchiveFileProfile(String accessToken,
                                                     Guid fileProfileId,
                                                     CancellationToken cancellationToken)
        {
            String requestUri = this.BuildRequestUrl($"/api/file-profiles/{fileProfileId}");
            Result<FileProfile> result = await this.Delete(requestUri, accessToken, cancellationToken);

            if (result.IsFailed)
            {
                return ResultHelpers.CreateFailure(result);
            }

            return Result.Success();
        }

        public async Task<Result<ApiFileDetails>> GetFile(String accessToken,
                                                          Guid estateId,
                                                          Guid fileId,
                                                          CancellationToken cancellationToken) {
            String requestUri = this.BuildRequestUrl($"/api/files/{fileId}?estateId={estateId}");

            try {
                Result<ApiFileDetails> result = await this.Get<ApiFileDetails>(requestUri, accessToken, cancellationToken);

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

        public async Task<Result<ApiFileImportLog>> GetFileImportLog(String accessToken,
                                                                     Guid fileImportLogId,
                                                                     Guid estateId,
                                                                     Guid? merchantId,
                                                                     CancellationToken cancellationToken) {
            String requestUri = this.BuildRequestUrl($"/api/fileImportLogs/{fileImportLogId}?estateId={estateId}");

            if (merchantId.HasValue) {
                requestUri += $"&merchantId={merchantId}";
            }

            try {
                Result<ApiFileImportLog> result = await this.Get<ApiFileImportLog>(requestUri, accessToken, cancellationToken);

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

        public async Task<Result<FileImportLogList>> GetFileImportLogs(String accessToken,
                                                               Guid estateId,
                                                               DateOnly startDate,
                                                               DateOnly endDate,
                                                               Guid? merchantId,
                                                               PagingRequest pagingRequest,
                                                               CancellationToken cancellationToken) {

            Boolean descending = pagingRequest.Direction switch {
                                     SortDirection.Ascending => false,
                                     _ => true
                                 };

            String requestUri = this.BuildRequestUrl(
                $"/api/fileImportLogs?estateId={estateId}&startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}&pageNumber={pagingRequest.PageNumber}&pageSize={pagingRequest.PageSize}&descending={descending}");

            if (merchantId.HasValue)
                requestUri += $"&merchantId={merchantId}";

            Result<FileImportLogList> result = await this.Get<FileImportLogList>(requestUri, accessToken, cancellationToken);
            return result.IsFailed ? ResultHelpers.CreateFailure(result) : result;
        }

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

        private String BuildRequestUrl(String route) {
            String baseAddress = this.BaseAddressResolver("FileProcessorApi");

            String requestUri = $"{baseAddress}{route}";

            return requestUri;
        }


        #endregion
    }
}
