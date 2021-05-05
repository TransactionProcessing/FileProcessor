using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileProcessor.Controllers
{
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Net;
    using System.Net.Http.Headers;
    using System.Threading;
    using BusinessLogic.Managers;
    using DataTransferObjects;
    using MediatR;
    using Microsoft.AspNetCore.Authorization;
    using Shared.General;

    [ExcludeFromCodeCoverage]
    [Route(FileController.ControllerRoute)]
    [ApiController]
    //[Authorize]
    public class FileController : ControllerBase
    {
        private readonly IMediator Mediator;

        public FileController(IMediator mediator, IFileProcessorManager fileProcessorManager)
        {
            this.Mediator = mediator;
           
        }

        [Route("")]
        [HttpPost, DisableRequestSizeLimit]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadFile([FromForm] UploadFileRequest request, [FromForm] IFormCollection formCollection, CancellationToken cancellationToken)
        {
            var file = formCollection.Files.First();
            
            var temporaryFileLocation = ConfigurationReader.GetValue("AppSettings", "TemporaryFileLocation");
            Guid fileId = Guid.NewGuid();
            var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');

            var fullPath = Path.Combine(temporaryFileLocation, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream, cancellationToken);
            }

            // Create a command with the file in it
            BusinessLogic.Requests.UploadFileRequest uploadFileRequest =
                new BusinessLogic.Requests.UploadFileRequest(request.EstateId, request.MerchantId, fileId, request.UserId, fullPath, request.FileProfileId);

            await this.Mediator.Send(uploadFileRequest, cancellationToken);

            return this.Accepted(fileId);
        }

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
