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
using Rat.Domain.Entities;
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
                var key = Encoding.ASCII.GetBytes(_jwtOptions.Value.SecretKey);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(
                        new Claim[]
                        {
                            new Claim(ClaimTypes.Email, user.Email),
                            new Claim(CustomClaimTypes.Id, user.Id.ToString()),
                            new Claim(CustomClaimTypes.IsAdmin, (await _userService.IsUserAdminAsync(user.Id)).ToString())
                        }),
                    Expires = DateTime.UtcNow.AddMinutes(_jwtOptions.Value.ExpiryMinutes),
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                _httpContextAccessor.HttpContext.Response.Cookies.Append(
                    _jwtOptions.Value.AuthorizationCookieKey,
                    tokenString,
                    new CookieOptions
                    {
                        HttpOnly = true
                    });
            }
            else
            {
                throw new NonExistingEntityEntryException(nameof(User));
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

            _httpContextAccessor.HttpContext.Response.Cookies.Delete(_jwtOptions.Value.AuthorizationCookieKey);
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
