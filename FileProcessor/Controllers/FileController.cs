namespace FileProcessor.Controllers
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessLogic.Managers;
    using Common;
    using DataTransferObjects;
    using FIleProcessor.Models;
    using MediatR;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Shared.General;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.ControllerBase" />
    [ExcludeFromCodeCoverage]
    [Route(FileController.ControllerRoute)]
    [ApiController]
    [Authorize]
    public class FileController : ControllerBase
    {
        #region Fields

        /// <summary>
        /// The mediator
        /// </summary>
        private readonly IMediator Mediator;

        /// <summary>
        /// The manager
        /// </summary>
        private readonly IFileProcessorManager Manager;

        private readonly IModelFactory ModelFactory;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FileController" /> class.
        /// </summary>
        /// <param name="mediator">The mediator.</param>
        /// <param name="manager">The manager.</param>
        public FileController(IMediator mediator, IFileProcessorManager manager, IModelFactory modelFactory)
        {
            this.Mediator = mediator;
            this.Manager = manager;
            this.ModelFactory = modelFactory;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Uploads the file.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="formCollection">The form collection.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [Route("")]
        [HttpPost]
        [DisableRequestSizeLimit]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadFile([FromForm] UploadFileRequest request,
                                                    [FromForm] IFormCollection formCollection,
                                                    CancellationToken cancellationToken)
        {
            IFormFile file = formCollection.Files.First();

            String temporaryFileLocation = ConfigurationReader.GetValue("AppSettings", "TemporaryFileLocation");
            String fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');

            String fullPath = Path.Combine(temporaryFileLocation, fileName);

            using(FileStream stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream, cancellationToken);
            }

            if (request.UploadDateTime == DateTime.MinValue)
            {
                request.UploadDateTime = DateTime.Now;
            }

            // Create a command with the file in it
            BusinessLogic.Requests.UploadFileRequest uploadFileRequest =
                new BusinessLogic.Requests.UploadFileRequest(request.EstateId, request.MerchantId, request.UserId, fullPath, request.FileProfileId, request.UploadDateTime);

            Guid fileId = await this.Mediator.Send(uploadFileRequest, cancellationToken);

            return this.Accepted(fileId);
        }

        /// <summary>
        /// Gets the file.
        /// </summary>
        /// <param name="fileId">The file identifier.</param>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{fileId}")]
        public async Task<IActionResult> GetFile([FromRoute] Guid fileId,
                                                 [FromQuery] Guid estateId,
                                                 CancellationToken cancellationToken)
        {
            FileDetails fileDetailsModel = await this.Manager.GetFile(fileId, estateId, cancellationToken);

            return this.Ok(this.ModelFactory.ConvertFrom(fileDetailsModel));
        }

        #endregion

        #region Others

        /// <summary>
        /// The controller name
        /// </summary>
        public const String ControllerName = "files";

        /// <summary>
        /// The controller route
        /// </summary>
        private const String ControllerRoute = "api/" + FileController.ControllerName;

        #endregion
    }
}