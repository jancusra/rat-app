using System.Security.Claims;

namespace Rat.Framework.Authentication
{
    /// <summary>
    /// Claims principal provider
    /// </summary>
    public partial interface IClaimsPrincipalProvider
    {
        /// <summary>
        /// Get current logged user claims principal
        /// </summary>
        /// <returns></returns>
        ClaimsPrincipal GetClaimsPrincipal();
    }
}
