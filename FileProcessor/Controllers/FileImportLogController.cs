namespace FileProcessor.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using BusinessLogic.Managers;
    using Common;
    using FIleProcessor.Models;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.ControllerBase" />
    [ExcludeFromCodeCoverage]
    [Route(FileImportLogController.ControllerRoute)]
    [ApiController]
    [Authorize]
    public class FileImportLogController : ControllerBase
    {
        #region Fields

        /// <summary>
        /// The manager
        /// </summary>
        private readonly IFileProcessorManager Manager;

        /// <summary>
        /// The model factory
        /// </summary>
        private readonly IModelFactory ModelFactory;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FileImportLogController"/> class.
        /// </summary>
        /// <param name="manager">The manager.</param>
        /// <param name="modelFactory">The model factory.</param>
        public FileImportLogController(IFileProcessorManager manager,
                                       IModelFactory modelFactory)
        {
            this.Manager = manager;
            this.ModelFactory = modelFactory;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the import logs.
        /// </summary>
        /// <param name="estateId">The estate identifier.</param>
        /// <param name="startDateTime">The start date time.</param>
        /// <param name="endDateTime">The end date time.</param>
        /// <param name="merchantId">The merchant identifier.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetImportLogs([FromQuery] Guid estateId,
                                                       [FromQuery] DateTime startDateTime,
                                                       [FromQuery] DateTime endDateTime,
                                                       [FromQuery] Guid? merchantId,
                                                       CancellationToken cancellationToken)
        {
            List<FileImportLog> fileImportLogs = await this.Manager.GetFileImportLogs(estateId, startDateTime, endDateTime, merchantId, cancellationToken);

            return this.Ok(this.ModelFactory.ConvertFrom(fileImportLogs));
        }

        [HttpGet]
        [Route("{fileImportLogId}")]
        public async Task<IActionResult> GetImportLog([FromRoute] Guid fileImportLogId,
                                                           [FromQuery] Guid estateId,
                                                           [FromQuery] Guid? merchantId,
                                                           CancellationToken cancellationToken)
        {
            FileImportLog fileImportLog = await this.Manager.GetFileImportLog(fileImportLogId, estateId, merchantId, cancellationToken);

            return this.Ok(this.ModelFactory.ConvertFrom(fileImportLog));
        }

        #endregion

            #region Others

            /// <summary>
            /// The controller name
            /// </summary>
        public const String ControllerName = "fileImportLogs";

        /// <summary>
        /// The controller route
        /// </summary>
        private const String ControllerRoute = "api/" + FileImportLogController.ControllerName;

        #endregion
    }
}