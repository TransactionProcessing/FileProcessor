using FileProcessor.BusinessLogic.Requests;
using FileProcessor.DataTransferObjects.Responses;
using MediatR;
using Microsoft.AspNetCore.Http;
using Shared.EventStore.Aggregate;
using Shared.Results;
using Shared.Results.Web;
using SimpleResults;

namespace FileProcessor.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using Common;
    using Models;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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
        
        private readonly IMediator Mediator;

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
        public FileImportLogController(IMediator mediator,
                                           IModelFactory modelFactory)
        {
            this.Mediator = mediator;
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
        public async Task<IResult> GetImportLogs([FromQuery] Guid estateId,
                                                       [FromQuery] DateTime startDateTime,
                                                       [FromQuery] DateTime endDateTime,
                                                       [FromQuery] Guid? merchantId,
                                                       CancellationToken cancellationToken) {
            FileQueries.GetImportLogsQuery query = new(estateId, startDateTime, endDateTime, merchantId);

            Result<List<FileImportLog>> result = await this.Mediator.Send(query, cancellationToken);
            
            return ResponseFactory.FromResult(result, (r) => this.ModelFactory.ConvertFrom(r));
        }

        [HttpGet]
        [Route("{fileImportLogId}")]
        public async Task<IResult> GetImportLog([FromRoute] Guid fileImportLogId,
                                                           [FromQuery] Guid estateId,
                                                           [FromQuery] Guid? merchantId,
                                                           CancellationToken cancellationToken)
        {
            FileQueries.GetImportLogQuery query = new(fileImportLogId, estateId, merchantId);

            Result<FileImportLog> result = await this.Mediator.Send(query, cancellationToken);

            return ResponseFactory.FromResult(result, (r) => this.ModelFactory.ConvertFrom(r));
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