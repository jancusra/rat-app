using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Rat.Domain.Exceptions;
using Rat.Domain.Responses;
using Rat.Services;

namespace Rat.Framework.Exceptions
{
    /// <summary>
    /// Error wrapping middleware to log api issues
    /// </summary>
    public partial class ErrorWrappingMiddleware : BaseErrorMiddleware
    {
        private readonly RequestDelegate next;

        public ErrorWrappingMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        /// <summary>
        /// Method to log api warnings/errors
        /// </summary>
        /// <param name="context">HTTP context</param>
        /// <param name="logger">logger service</param>
        public async Task Invoke(HttpContext context, ILogService logger)
        {
            try
            {
                await next.Invoke(context);
            }
            catch (BaseResponseException baseResponseException)
            {
                await logger.WarningAsync(baseResponseException.Message, baseResponseException);
                await SendResponseIfNotStarted(context, baseResponseException.ResponseState);
            }
            catch (Exception ex)
            {
                await logger.ErrorAsync(ex.Message, ex);
                await SendResponseIfNotStarted(context, new ResponseState { Code = 10000, HttpStatusCode = 500, Message = "Non expected error" });
            }
        }
    }
}
