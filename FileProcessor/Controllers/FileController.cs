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
    using DataTransferObjects;
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

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FileController"/> class.
        /// </summary>
        /// <param name="mediator">The mediator.</param>
        public FileController(IMediator mediator)
        {
            this.Mediator = mediator;
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

            // Create a command with the file in it
            BusinessLogic.Requests.UploadFileRequest uploadFileRequest =
                new BusinessLogic.Requests.UploadFileRequest(request.EstateId, request.MerchantId, request.UserId, fullPath, request.FileProfileId, DateTime.Now);

            Guid fileId = await this.Mediator.Send(uploadFileRequest, cancellationToken);

            return this.Accepted(fileId);
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