using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Rat.Framework.Authentication
{
    public partial class HttpContextClaimsPrincipalProvider : IClaimsPrincipalProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpContextClaimsPrincipalProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public virtual ClaimsPrincipal GetClaimsPrincipal()
        {
            return _httpContextAccessor.HttpContext.User;
        }
    }
}
