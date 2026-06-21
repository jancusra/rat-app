using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Rat.Domain;
using Rat.Domain.Exceptions;
using Rat.Services;

namespace Rat.Framework.Authentication
{
    public partial class TokenManager : ITokenManager
    {
        private readonly IDistributedCache _cache;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserService _userService;
        private readonly IOptions<JwtOptions> _jwtOptions;

        public TokenManager(
            IDistributedCache cache,
            IHttpContextAccessor httpContextAccessor,
            IUserService userService,
            IOptions<JwtOptions> jwtOptions)
        {
            _cache = cache;
            _httpContextAccessor = httpContextAccessor;
            _userService = userService;
            _jwtOptions = jwtOptions;
        }

        public virtual async Task CreateTokenAsync(string email, string password)
        {
            var user = await _userService.LoginUserValidationAsync(email, password);

            if (user != null)
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_jwtOptions.Value.SecretKey);

                // Single expiry shared by the JWT and the cookie so the cookie disappears
                // exactly when the token stops being valid (no stale cookie / no mismatch).
                var expiresUtc = DateTime.UtcNow.AddMinutes(_jwtOptions.Value.ExpiryMinutes);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(
                        new Claim[]
                        {
                            new Claim(ClaimTypes.Email, user.Email),
                            new Claim(CustomClaimTypes.Id, user.Id.ToString()),
                            new Claim(CustomClaimTypes.IsAdmin, (await _userService.IsUserAdminAsync(user.Id)).ToString())
                        }),
                    Expires = expiresUtc,
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                var cookieOptions = BuildAuthCookieOptions();
                cookieOptions.Expires = expiresUtc;

                _httpContextAccessor.HttpContext.Response.Cookies.Append(
                    _jwtOptions.Value.AuthorizationCookieKey,
                    tokenString,
                    cookieOptions);
            }
            else
            {
                throw new InvalidCredentialsException();
            }
        }

        public virtual async Task<bool> IsCurrentTokenActiveAsync()
        {
            return await IsTokenActiveAsync(GetCurrentToken());
        }

        public virtual async Task<bool> IsTokenActiveAsync(string token)
        {
            return await _cache.GetStringAsync(GetKey(token)) == null;
        }

        public virtual async Task DeactivateCurrentTokenAsync()
        {
            await DeactivateTokenAsync(GetCurrentToken());
        }

        public virtual async Task DeactivateTokenAsync(string token)
        {
            await _cache.SetStringAsync(GetKey(token),
                " ", new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow =
                        TimeSpan.FromMinutes(_jwtOptions.Value.ExpiryMinutes)
                });

            _httpContextAccessor.HttpContext.Response.Cookies.Delete(
                _jwtOptions.Value.AuthorizationCookieKey, BuildAuthCookieOptions());
        }

        /// <summary>
        /// Build the cookie options for the auth cookie. Used for both writing (login)
        /// and deleting (logout) so the attributes match — otherwise the browser keeps
        /// the original cookie. Secure is forced on when SameSite=None (browser requirement).
        /// </summary>
        /// <returns>configured cookie options</returns>
        private CookieOptions BuildAuthCookieOptions()
        {
            var sameSite = Enum.TryParse<SameSiteMode>(_jwtOptions.Value.CookieSameSite, true, out var mode)
                ? mode : SameSiteMode.Lax;

            return new CookieOptions
            {
                HttpOnly = true,
                Secure = _jwtOptions.Value.CookieSecure || sameSite == SameSiteMode.None,
                SameSite = sameSite
            };
        }

        private string GetCurrentToken()
        {
            string cookieToken = string.Empty;

            _httpContextAccessor.HttpContext.Request.Cookies.TryGetValue(_jwtOptions.Value.AuthorizationCookieKey, out cookieToken);

            return cookieToken;
        }

        private string GetKey(string token)
        {
            return $"tokens:{token}:deactivated";
        }
    }
}
