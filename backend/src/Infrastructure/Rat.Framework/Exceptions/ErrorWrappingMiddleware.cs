using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
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
        /// <param name="logger">logger service (persists to the database)</param>
        /// <param name="fallbackLogger">framework logger used when database logging fails</param>
        public async Task Invoke(HttpContext context, ILogService logger, ILogger<ErrorWrappingMiddleware> fallbackLogger)
        {
            try
            {
                await next.Invoke(context);
            }
            catch (BaseResponseException baseResponseException)
            {
                await SafeLogAsync(
                    () => logger.WarningAsync(baseResponseException.Message, baseResponseException),
                    fallbackLogger, baseResponseException.Message, baseResponseException);
                await SendResponseIfNotStarted(context, baseResponseException.ResponseState);
            }
            catch (Exception ex)
            {
                await SafeLogAsync(
                    () => logger.ErrorAsync(ex.Message, ex),
                    fallbackLogger, ex.Message, ex);
                await SendResponseIfNotStarted(context, new ResponseState { Code = 10000, HttpStatusCode = 500, Message = "Non expected error" });
            }
        }

        /// <summary>
        /// Persist a log entry, falling back to the framework logger (console/stderr) when the
        /// primary (database) logging throws - e.g. the database is unavailable. This prevents a
        /// secondary logging failure from masking the original error and crashing the request.
        /// </summary>
        /// <param name="logAction">primary database logging action</param>
        /// <param name="fallbackLogger">fallback logger</param>
        /// <param name="message">original error message</param>
        /// <param name="exception">original exception</param>
        private static async Task SafeLogAsync(
            Func<Task> logAction, ILogger fallbackLogger, string message, Exception exception)
        {
            try
            {
                await logAction();
            }
            catch (Exception loggingException)
            {
                fallbackLogger.LogError(exception, "{Message}", message);
                fallbackLogger.LogError(loggingException, "Failed to persist the log entry to the database; original error logged above.");
            }
        }
    }
}
