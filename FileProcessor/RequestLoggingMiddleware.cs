using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Shared.General;

namespace Shared.Middleware
{
    using System.Security.Policy;
    using NLog;
    using Logger = Logger.Logger;

    public class RequestLoggingMiddlewareTemp
    {
        #region Fields

        /// <summary>
        /// The next
        /// </summary>
        private readonly RequestDelegate next;

        #endregion
        
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="RequestLoggingMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next.</param>
        public RequestLoggingMiddlewareTemp(RequestDelegate next)
        {
            this.next = next;
        }
        #endregion

        #region Public Methods

        #region public async Task Invoke(HttpContext context)        
        /// <summary>
        /// Invokes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context)
        {
            var requestBodyStream = new MemoryStream();
            var originalRequestBody = context.Request.Body;

            await context.Request.Body.CopyToAsync(requestBodyStream);
            requestBodyStream.Seek(0, SeekOrigin.Begin);

            var url = UriHelper.GetDisplayUrl(context.Request);
            var requestBodyText = new StreamReader(requestBodyStream).ReadToEnd();
            StringBuilder logMessage = new StringBuilder();
            logMessage.Append($"Request: Method: {context.Request.Method} Url: {url}");
            if (!String.IsNullOrEmpty(requestBodyText))
            {
                logMessage.Append(" ");
                logMessage.Append($"Body: {requestBodyText}");
            }

            LogMessage(url, logMessage);

            requestBodyStream.Seek(0, SeekOrigin.Begin);
            context.Request.Body = requestBodyStream;

            await next(context);
            context.Request.Body = originalRequestBody;
        }

        internal static Boolean IsHealthCheckRequest(String url) => url.EndsWith("/health");

        internal static void LogMessage(String url,
                                       StringBuilder message)
        {
            if (IsHealthCheckRequest(url))
            {
                // TODO: new logger method??
                Logger.LogInformation($"HEALTH_CHECK | {message}");
            }
            else
            {
                Logger.LogInformation($"{message}");
            }
        }

        #endregion

        #endregion
    }
}
