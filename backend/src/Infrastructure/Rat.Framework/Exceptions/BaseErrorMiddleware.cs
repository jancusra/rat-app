using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Rat.Domain.Responses;

namespace Rat.Framework.Exceptions
{
    /// <summary>
    /// Represents base error middleware
    /// </summary>
    public abstract class BaseErrorMiddleware
    {
        /// <summary>
        /// Write the pontential issue to the response 
        /// </summary>
        /// <param name="context">HTTP context</param>
        /// <param name="responseState">response state</param>
        protected virtual async Task SendResponseIfNotStarted(HttpContext context, ResponseState responseState)
        {
            if (!context.Response.HasStarted)
            {
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = responseState.HttpStatusCode;
                var response = new BaseResponse(responseState.Code, responseState.Message);
                var json = JsonConvert.SerializeObject(response);
                await context.Response.WriteAsync(json);
            }
        }
    }
}
