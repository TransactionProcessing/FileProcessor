using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileProcessor.BusinessLogic.RequestHandlers
{
    using System.IO;
    using System.IO.Abstractions;
    using System.Security.Cryptography;
    using MediatR;
    using System.Threading;
    using EstateManagement.Client;
    using EstateManagement.DataTransferObjects.Responses;
    using EventHandling;
    using FileAggregate;
    using FileFormatHandlers;
    using FileImportLogAggregate;
    using FIleProcessor.Models;
    using Managers;
    using Newtonsoft.Json;
    using Requests;
    using SecurityService.Client;
    using SecurityService.DataTransferObjects.Responses;
    using Shared.DomainDrivenDesign.EventSourcing;
    using Shared.EventStore.Aggregate;
    using Shared.Exceptions;
    using Shared.General;
    using Shared.Logger;
    using TransactionProcessor.Client;
    using TransactionProcessor.DataTransferObjects;
    using Exception = System.Exception;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="UploadFileRequest" />
    /// <seealso cref="SafaricomTopupRequest" />
    /// <seealso cref="ProcessTransactionForFileLineRequest" />
    public class FileRequestHandler : IRequestHandler<UploadFileRequest,Guid>,
                                      IRequestHandler<ProcessUploadedFileRequest>,
                                      IRequestHandler<SafaricomTopupRequest>,
                                      IRequestHandler<VoucherRequest>,
                                      IRequestHandler<ProcessTransactionForFileLineRequest>
    {
        /// <summary>
        /// The file processor manager
        /// </summary>
        private readonly IFileProcessorManager FileProcessorManager;

        private readonly IAggregateRepository<FileImportLogAggregate, DomainEventRecord.DomainEvent> FileImportLogAggregateRepository;

        /// <summary>
        /// The file aggregate repository
        /// </summary>
        private readonly IAggregateRepository<FileAggregate, DomainEventRecord.DomainEvent> FileAggregateRepository;

        /// <summary>
        /// The transaction processor client
        /// </summary>
        private readonly ITransactionProcessorClient TransactionProcessorClient;

        /// <summary>
        /// The estate client
        /// </summary>
        private readonly IEstateClient EstateClient;

        /// <summary>
        /// The security service client
        /// </summary>
        private readonly ISecurityServiceClient SecurityServiceClient;

        /// <summary>
        /// The file format handler resolver
        /// </summary>
        private readonly Func<String, IFileFormatHandler> FileFormatHandlerResolver;

        /// <summary>
        /// The file system
        /// </summary>
        private readonly IFileSystem FileSystem;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileRequestHandler"/> class.
        /// </summary>
        /// <param name="fileProcessorManager">The file processor manager.</param>
        /// <param name="fileAggregateRepository">The file aggregate repository.</param>
        /// <param name="transactionProcessorClient">The transaction processor client.</param>
        /// <param name="estateClient">The estate client.</param>
        /// <param name="securityServiceClient">The security service client.</param>
        /// <param name="fileFormatHandlerResolver">The file format handler resolver.</param>
        /// <param name="fileSystem">The file system.</param>
        public FileRequestHandler(IFileProcessorManager fileProcessorManager,
                                  IAggregateRepository<FileImportLogAggregate, DomainEventRecord.DomainEvent> fileImportLogAggregateRepository,
                                  IAggregateRepository<FileAggregate, DomainEventRecord.DomainEvent> fileAggregateRepository,
                                  ITransactionProcessorClient transactionProcessorClient,
                                  IEstateClient estateClient,
                                  ISecurityServiceClient securityServiceClient,
                                  Func<String, IFileFormatHandler> fileFormatHandlerResolver,
                                  IFileSystem fileSystem)
        {
            this.FileProcessorManager = fileProcessorManager;
            this.FileImportLogAggregateRepository = fileImportLogAggregateRepository;
            this.FileAggregateRepository = fileAggregateRepository;
            this.TransactionProcessorClient = transactionProcessorClient;
            this.EstateClient = estateClient;
            this.SecurityServiceClient = securityServiceClient;
            this.FileFormatHandlerResolver = fileFormatHandlerResolver;
            this.FileSystem = fileSystem;
        }

        // TODO: Create a domain service

        /// <summary>
        /// Handles the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="NotFoundException">No file profile found with Id {request.FileProfileId}</exception>
        /// <exception cref="System.IO.FileNotFoundException">File {file.FullName} not found</exception>
        /// <exception cref="System.IO.DirectoryNotFoundException">Directory {fileProfile.ListeningDirectory} not found</exception>
        public async Task<Guid> Handle(UploadFileRequest request,
                                       CancellationToken cancellationToken)
        {
            DateTime importLogDateTime = DateTime.Now;

            // This will now create the import log and add an event for the file being uploaded
            Guid importLogId = this.CreateGuidFromDateTime(importLogDateTime.Date);

            // Get the import log
            FileImportLogAggregate fileImportLogAggregate = await this.FileImportLogAggregateRepository.GetLatestVersion(importLogId, cancellationToken);

            if (fileImportLogAggregate.IsCreated == false)
            {
                // First file of the day so create
                fileImportLogAggregate.CreateImportLog(request.EstateId, importLogDateTime);
            }

            // Move the file
            FileProfile fileProfile = await this.FileProcessorManager.GetFileProfile(request.FileProfileId, cancellationToken);

            if (fileProfile == null)
            {
                throw new NotFoundException($"No file profile found with Id {request.FileProfileId}");
            }

            // Copy file from the temp location to file processing listening directory
            IFileInfo file = this.FileSystem.FileInfo.FromFileName(request.FilePath);
            if (file.Exists == false)
            {
                throw new FileNotFoundException($"File {file.FullName} not found");
            }
            String originalName = file.Name;

            if (this.FileSystem.Directory.Exists(fileProfile.ListeningDirectory) == false)
            {
                throw new DirectoryNotFoundException($"Directory {fileProfile.ListeningDirectory} not found");
            }
            
            // Read the file data
            String fileContent = null;
            //Open file for Read\Write
            using (Stream fs = file.Open(FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read))
            {
                //Create object of StreamReader by passing FileStream object on which it needs to operates on
                using (StreamReader sr = new StreamReader(fs))
                {
                    //Use ReadToEnd method to read all the content from file
                    fileContent = await sr.ReadToEndAsync();
                }
            }

            Guid fileId = this.CreateGuidFromFileData(fileContent);

            String fileDestination = $"{fileProfile.ListeningDirectory}//{request.EstateId:N}-{fileId:N}";
            file.MoveTo(fileDestination, overwrite: true);

            // Update Import log aggregate
            fileImportLogAggregate.AddImportedFile(fileId, request.MerchantId, request.UserId, request.FileProfileId, originalName, fileDestination, request.FileUploadedDateTime);

            // Save changes
            await this.FileImportLogAggregateRepository.SaveChanges(fileImportLogAggregate, cancellationToken);

            return fileId;
        }

        private Guid CreateGuidFromFileData(String fileContents)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                //Generate hash from the key
                Byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(fileContents));

                Byte[] j = bytes.Skip(Math.Max(0, bytes.Count() - 16)).ToArray(); //Take last 16

                //Create our Guid.
                return new Guid(j);
            }
        }

        private Guid CreateGuidFromDateTime(DateTime dateTime)
        {
            var bytes = BitConverter.GetBytes(dateTime.Ticks);

            Array.Resize(ref bytes, 16);

            var guid = new Guid(bytes);

            return guid;
        }

       
        public async Task<Unit> Handle(ProcessUploadedFileRequest request, CancellationToken cancellationToken)
        {
            // TODO: Should the file id be generated from the file uploaded to protect against duplicate files???
            FileAggregate fileAggregate = await this.FileAggregateRepository.GetLatestVersion(request.FileId, cancellationToken);

            fileAggregate.CreateFile(request.FileImportLogId, request.EstateId, request.MerchantId, request.UserId, request.FileProfileId, request.FilePath, request.FileUploadedDateTime);

            await this.FileAggregateRepository.SaveChanges(fileAggregate, cancellationToken);

            return new Unit();
        }

        /// <summary>
        /// Handles the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="System.IO.FileNotFoundException">File {file.FullName} not found</exception>
        /// <exception cref="NotFoundException">No file profile found with Id {request.FileProfileId}</exception>
        /// <exception cref="System.IO.DirectoryNotFoundException">
        /// Directory {inProgressFolder} not found
        /// or
        /// Directory {fileProfile.ProcessedDirectory} not found
        /// or
        /// Directory {fileProfile.FailedDirectory} not found
        /// </exception>
        public async Task<Unit> Handle(SafaricomTopupRequest request,
                                       CancellationToken cancellationToken)
        {
            FileAggregate fileAggregate = await this.FileAggregateRepository.GetLatestVersion(request.FileId, cancellationToken);

            if (fileAggregate.IsCreated == false)
                return new Unit();
            
            FileProfile fileProfile = await this.FileProcessorManager.GetFileProfile(request.FileProfileId, cancellationToken);

            if (fileProfile == null)
            {
                throw new NotFoundException($"No file profile found with Id {request.FileProfileId}");
            }
            
            IFileInfo inProgressFile = this.FileSystem.FileInfo.FromFileName(request.FileName);

            if (inProgressFile.Exists == false)
            {
                throw new FileNotFoundException($"File {inProgressFile.FullName} not found");
            }

            // Check the processed/failed directories exist
            if (this.FileSystem.Directory.Exists(fileProfile.ProcessedDirectory) == false)
            {
                throw new DirectoryNotFoundException($"Directory {fileProfile.ProcessedDirectory} not found");
            }

            if (this.FileSystem.Directory.Exists(fileProfile.FailedDirectory) == false)
            {
                throw new DirectoryNotFoundException($"Directory {fileProfile.FailedDirectory} not found");
            }
            String fileContent = null;
            //Open file for Read\Write
            using(Stream fs = inProgressFile.Open(FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read))
            {
                //Create object of StreamReader by passing FileStream object on which it needs to operates on
                using(StreamReader sr = new StreamReader(fs))
                {
                    //Use ReadToEnd method to read all the content from file
                    fileContent = await sr.ReadToEndAsync();
                }
            }

            if (String.IsNullOrEmpty(fileContent) == false)
            {
                String[] fileLines = fileContent.Split(fileProfile.LineTerminator);

                foreach (String fileLine in fileLines)
                {
                    fileAggregate.AddFileLine(fileLine);
                }
                
                await this.FileAggregateRepository.SaveChanges(fileAggregate, cancellationToken);
            }

            // TODO: Move file now
            inProgressFile.MoveTo($"{fileProfile.ProcessedDirectory}/{inProgressFile.Name}");
            
            return new Unit();
        }

        /// <summary>
        /// The transaction number
        /// </summary>
        private static Int32 TransactionNumber = 0;

        /// <summary>
        /// Handles the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        /// <exception cref="System.NotSupportedException">File Id [{request.FileId}] has no lines added</exception>
        /// <exception cref="NotFoundException">
        /// File Line Number {request.LineNumber} not found in File Id {request.FileId}
        /// or
        /// No file profile found with Id {fileDetails.FileProfileId}
        /// or
        /// Merchant not found with Id {fileDetails.MerchantId} on estate Id {fileDetails.EstateId}
        /// or
        /// No contracts found for Merchant Id {fileDetails.MerchantId} on estate Id {fileDetails.EstateId}
        /// or
        /// No merchant contract for operator Id {fileProfile.OperatorName} found for Merchant Id {merchant.MerchantId}
        /// or
        /// No variable value product found on the merchant contract for operator Id {fileProfile.OperatorName} and Merchant Id {merchant.MerchantId}
        /// </exception>
        public async Task<Unit> Handle(ProcessTransactionForFileLineRequest request,
                                       CancellationToken cancellationToken)
        {
            // Get the file aggregate, this tells us the file profile information
            FileAggregate fileAggregate = await this.FileAggregateRepository.GetLatestVersion(request.FileId, cancellationToken);
            
            FileDetails fileDetails = fileAggregate.GetFile();

            if (fileDetails.FileLines.Any() == false)
            {
                throw new NotSupportedException($"File Id [{request.FileId}] has no lines added");
            }

            FileLine fileLine = fileDetails.FileLines.SingleOrDefault(f => f.LineNumber == request.LineNumber);

            if (fileLine == null)
            {
                throw new NotFoundException($"File Line Number {request.LineNumber} not found in File Id {request.FileId}");
            }

            if (fileLine.ProcessingResult != ProcessingResult.NotProcessed)
            {
                // Line already processed
                return new Unit();
            }
            
            FileProfile fileProfile = await this.FileProcessorManager.GetFileProfile(fileDetails.FileProfileId, cancellationToken);

            if (fileProfile == null)
            {
                throw new NotFoundException($"No file profile found with Id {fileDetails.FileProfileId}");
            }

            // Determine if we need to actually process this file line
            if (this.FileLineCanBeIgnored(fileLine.LineData, fileProfile.FileFormatHandler))
            { 
                // Write something to aggregate to say line was explicity ignored
                fileAggregate.RecordFileLineAsIgnored(fileLine.LineNumber);
                await this.FileAggregateRepository.SaveChanges(fileAggregate, cancellationToken);
                return new Unit();
            }

            // need to now parse the line (based on the file format), this builds the metadata
            Dictionary<String, String> transactionMetadata = this.ParseFileLine(fileLine.LineData, fileProfile.FileFormatHandler);

            if (transactionMetadata == null)
            {
                // Line failed to parse so record this
                fileAggregate.RecordFileLineAsRejected(fileLine.LineNumber, "Invalid Format");
                await this.FileAggregateRepository.SaveChanges(fileAggregate, cancellationToken);
                return new Unit();
            }

            // Add the file data to the request metadata
            transactionMetadata.Add("FileId", request.FileId.ToString());
            transactionMetadata.Add("FileLineNumber", fileLine.LineNumber.ToString());

            String operatorName = fileProfile.OperatorName;
            if (transactionMetadata.ContainsKey("OperatorName"))
            {
                // extract the value
                operatorName = transactionMetadata["OperatorName"];
                transactionMetadata = transactionMetadata.Where(x => x.Key != "OperatorName").ToDictionary(x => x.Key, x => x.Value);
            }

            this.TokenResponse = await this.GetToken(cancellationToken);

            Interlocked.Increment(ref FileRequestHandler.TransactionNumber);

            // Get the merchant details
            MerchantResponse merchant = await this.EstateClient.GetMerchant(this.TokenResponse.AccessToken, fileDetails.EstateId, fileDetails.MerchantId, cancellationToken);
            if (merchant == null)
            {
                throw new NotFoundException($"Merchant not found with Id {fileDetails.MerchantId} on estate Id {fileDetails.EstateId}");
            }
            List<ContractResponse> contracts = await this.EstateClient.GetMerchantContracts(this.TokenResponse.AccessToken, fileDetails.EstateId, fileDetails.MerchantId, cancellationToken);

            if (contracts.Any() == false)
            {
                throw new NotFoundException($"No contracts found for Merchant Id {fileDetails.MerchantId} on estate Id {fileDetails.EstateId}");
            }

            ContractResponse? contract = null;
            if (fileProfile.OperatorName == "Voucher")
            {
                contract = contracts.SingleOrDefault(c => c.Description.Contains(operatorName));
            }
            else
            {
                contract = contracts.SingleOrDefault(c => c.OperatorName == operatorName);
            }
            

            if (contract == null)
            {
                throw new NotFoundException($"No merchant contract for operator Id {operatorName} found for Merchant Id {merchant.MerchantId}");
            }

            ContractProduct? product = contract.Products.SingleOrDefault(p => p.Value == null); // TODO: Is this enough or should the name be used and stored in file profile??

            if (product == null)
            {
                throw new NotFoundException($"No variable value product found on the merchant contract for operator Id {fileProfile.OperatorName} and Merchant Id {merchant.MerchantId}");
            }

            // Build a transaction request message
            SaleTransactionRequest saleTransactionRequest = new SaleTransactionRequest
            {
                EstateId = fileDetails.EstateId,
                MerchantId = fileDetails.MerchantId,
                TransactionDateTime = DateTime.Now,
                TransactionNumber = FileRequestHandler.TransactionNumber.ToString(),
                TransactionType = "Sale",
                ContractId = contract.ContractId,
                DeviceIdentifier = merchant.Devices.First().Value,
                OperatorIdentifier = contract.OperatorName,
                ProductId = product.ProductId,
                AdditionalTransactionMetadata = transactionMetadata,
            };
            
            SerialisedMessage serialisedRequestMessage = new SerialisedMessage
            {
                Metadata = new Dictionary<String, String>
                                                                 {
                                                                     {"estate_id", fileDetails.EstateId.ToString()},
                                                                     {"merchant_id", fileDetails.MerchantId.ToString()}
                                                                 },
                SerialisedData = JsonConvert.SerializeObject(saleTransactionRequest, new JsonSerializerSettings
                                                                                     {
                                                                                         TypeNameHandling = TypeNameHandling.All
                                                                                     })
            };

            Logger.LogInformation(serialisedRequestMessage.SerialisedData);

            // Send request to transaction processor
            SerialisedMessage serialisedResponseMessage = await this.TransactionProcessorClient.PerformTransaction(this.TokenResponse.AccessToken, serialisedRequestMessage, cancellationToken);

            // Get the sale transaction response
            SaleTransactionResponse saleTransactionResponse = JsonConvert.DeserializeObject<SaleTransactionResponse>(serialisedResponseMessage.SerialisedData);

            if (saleTransactionResponse.ResponseCode == "0000")
            {
                // record response against file line in file aggregate
                fileAggregate.RecordFileLineAsSuccessful(request.LineNumber, saleTransactionResponse.TransactionId);
            }
            else
            {
                fileAggregate.RecordFileLineAsFailed(request.LineNumber, saleTransactionResponse.TransactionId, saleTransactionResponse.ResponseCode, saleTransactionResponse.ResponseMessage);
            }

            // Save changes to file aggregate
            // TODO: Add retry round this save (maybe 3 retries)
            await this.FileAggregateRepository.SaveChanges(fileAggregate, cancellationToken);

            return new Unit();
        }

        /// <summary>
        /// Files the line can be ignored.
        /// </summary>
        /// <param name="domainEventFileLine">The domain event file line.</param>
        /// <param name="fileProfileFileFormatHandler">The file profile file format handler.</param>
        /// <returns></returns>
        private Boolean FileLineCanBeIgnored(String domainEventFileLine,
                                                 String fileProfileFileFormatHandler)
        {
            // Ignore empty files
            if (String.IsNullOrEmpty(domainEventFileLine))
                return true;

            IFileFormatHandler fileFormatHandler = this.FileFormatHandlerResolver(fileProfileFileFormatHandler);

            return fileFormatHandler.FileLineCanBeIgnored(domainEventFileLine);
        }

        /// <summary>
        /// Parses the file line.
        /// </summary>
        /// <param name="domainEventFileLine">The domain event file line.</param>
        /// <param name="fileProfileFileFormatHandler">The file profile file format handler.</param>
        /// <returns></returns>
        private Dictionary<String, String> ParseFileLine(String domainEventFileLine,
                                                        String fileProfileFileFormatHandler)
        {
            try
            {
                IFileFormatHandler fileFormatHandler = this.FileFormatHandlerResolver(fileProfileFileFormatHandler);

                return fileFormatHandler.ParseFileLine(domainEventFileLine);
            }
            catch(InvalidDataException iex)
            {
                Logger.LogWarning(iex.Message);
                return null;
            }
        }

        /// <summary>
        /// The token response
        /// </summary>
        private TokenResponse TokenResponse;

        /// <summary>
        /// Gets the token.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        private async Task<TokenResponse> GetToken(CancellationToken cancellationToken)
        {
            // Get a token to talk to the estate service
            String clientId = ConfigurationReader.GetValue("AppSettings", "ClientId");
            String clientSecret = ConfigurationReader.GetValue("AppSettings", "ClientSecret");
            Logger.LogInformation($"Client Id is {clientId}");
            Logger.LogInformation($"Client Secret is {clientSecret}");

            if (this.TokenResponse == null)
            {
                TokenResponse token = await this.SecurityServiceClient.GetToken(clientId, clientSecret, cancellationToken);
                Logger.LogInformation($"Token is {token.AccessToken}");
                return token;
            }

            if (this.TokenResponse.Expires.UtcDateTime.Subtract(DateTime.UtcNow) < TimeSpan.FromMinutes(2))
            {
                Logger.LogInformation($"Token is about to expire at {this.TokenResponse.Expires.DateTime:O}");
                TokenResponse token = await this.SecurityServiceClient.GetToken(clientId, clientSecret, cancellationToken);
                Logger.LogInformation($"Token is {token.AccessToken}");
                return token;
            }

            return this.TokenResponse;
        }

        public async Task<Unit> Handle(VoucherRequest request,
                                       CancellationToken cancellationToken)
        {
            FileAggregate fileAggregate = await this.FileAggregateRepository.GetLatestVersion(request.FileId, cancellationToken);

            if (fileAggregate.IsCreated == false)
                return new Unit();

            IFileInfo file = this.FileSystem.FileInfo.FromFileName(request.FileName);

            if (file.Exists == false)
            {
                throw new FileNotFoundException($"File {file.FullName} not found");
            }

            FileProfile fileProfile = await this.FileProcessorManager.GetFileProfile(request.FileProfileId, cancellationToken);

            if (fileProfile == null)
            {
                throw new NotFoundException($"No file profile found with Id {request.FileProfileId}");
            }

            String inProgressFolder = $"{fileProfile.ListeningDirectory}/inprogress/";
            if (this.FileSystem.Directory.Exists(inProgressFolder) == false)
            {
                throw new DirectoryNotFoundException($"Directory {inProgressFolder} not found");
            }
            String inProgressFilePath = $"{fileProfile.ListeningDirectory}/inprogress/{file.Name}";
            file.MoveTo(inProgressFilePath, true);

            IFileInfo inProgressFile = this.FileSystem.FileInfo.FromFileName(inProgressFilePath);

            // TODO: Check the processed/failed directories exist
            if (this.FileSystem.Directory.Exists(fileProfile.ProcessedDirectory) == false)
            {
                throw new DirectoryNotFoundException($"Directory {fileProfile.ProcessedDirectory} not found");
            }

            if (this.FileSystem.Directory.Exists(fileProfile.FailedDirectory) == false)
            {
                throw new DirectoryNotFoundException($"Directory {fileProfile.FailedDirectory} not found");
            }
            String fileContent = null;
            //Open file for Read\Write
            using (Stream fs = inProgressFile.Open(FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read))
            {
                //Create object of StreamReader by passing FileStream object on which it needs to operates on
                using (StreamReader sr = new StreamReader(fs))
                {
                    //Use ReadToEnd method to read all the content from file
                    fileContent = await sr.ReadToEndAsync();
                }
            }

            if (String.IsNullOrEmpty(fileContent) == false)
            {
                String[] fileLines = fileContent.Split(fileProfile.LineTerminator);

                foreach (String fileLine in fileLines)
                {
                    fileAggregate.AddFileLine(fileLine);
                }

                await this.FileAggregateRepository.SaveChanges(fileAggregate, cancellationToken);
            }

            // TODO: Move file now
            inProgressFile.MoveTo($"{fileProfile.ProcessedDirectory}/{inProgressFile.Name}");

            return new Unit();
        }
    }
}
