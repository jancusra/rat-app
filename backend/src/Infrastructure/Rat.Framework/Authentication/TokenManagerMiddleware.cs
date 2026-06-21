using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Rat.Framework.Authentication
{
    /// <summary>
    /// Middleware to handle request and check token
    /// </summary>
    public class TokenManagerMiddleware : IMiddleware
    {
        private readonly ITokenManager _tokenManager;

        public TokenManagerMiddleware(ITokenManager tokenManager)
        {
            _tokenManager = tokenManager;
        }
        
        /// <summary>
        /// Continue to the next if token is active
        /// </summary>
        /// <param name="context">HTTP context</param>
        /// <param name="next">next delegate</param>
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            // JWT authentication already validated the token; here we only enforce the
            // deny-list (logged-out tokens), and only for requests that actually carry an
            // authenticated identity. Anonymous requests (e.g. login) are left to the
            // authorization pipeline rather than paying for a cache lookup here.
            if (context.User?.Identity?.IsAuthenticated != true)
            {
                await next(context);

                return;
            }

            if (await _tokenManager.IsCurrentTokenActiveAsync())
            {
                await next(context);

                return;
            }

            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        }
    }
}