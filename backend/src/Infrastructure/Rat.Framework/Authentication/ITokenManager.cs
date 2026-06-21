using System.Threading.Tasks;

namespace Rat.Framework.Authentication
{
    /// <summary>
    /// JWT token manager
    /// </summary>
    public partial interface ITokenManager
    {
        /// <summary>
        /// Create a token for a logged user
        /// </summary>
        /// <param name="email">user email</param>
        /// <param name="password">user password</param>
        Task CreateTokenAsync(string email, string password);

        /// <summary>
        /// Determine if the current token is active
        /// </summary>
        /// <returns>bool result</returns>
        Task<bool> IsCurrentTokenActiveAsync();

        /// <summary>
        /// Determine if token by string is active
        /// </summary>
        /// <param name="token">input token</param>
        /// <returns>bool result</returns>
        Task<bool> IsTokenActiveAsync(string token);

        /// <summary>
        /// Deactivate current token (user logout)
        /// </summary>
        Task DeactivateCurrentTokenAsync();

        /// <summary>
        /// Deactivate token by string
        /// </summary>
        /// <param name="token">input token</param>
        Task DeactivateTokenAsync(string token);
    }
}
