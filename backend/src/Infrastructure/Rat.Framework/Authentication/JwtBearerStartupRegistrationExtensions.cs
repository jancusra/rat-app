using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Rat.Framework.Authentication
{
    public static class JwtBearerStartupRegistrationExtensions
    {
        /// <summary>
        /// Minimum length of the JWT secret key (HMAC-SHA256 needs at least a 256-bit / 32-byte key)
        /// </summary>
        private const int MinSecretKeyLength = 32;

        /// <summary>
        /// Startup method to register JWT bearer authentication
        /// </summary>
        /// <param name="services">collection of services</param>
        /// <param name="configuration">application configuration</param>
        /// <exception cref="InvalidOperationException">thrown when the JWT secret key is missing or too short</exception>
        public static void AddJwtBearerAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtSection = configuration.GetSection("JWT");
            var jwtOptions = new JwtOptions();
            jwtSection.Bind(jwtOptions);

            if (string.IsNullOrWhiteSpace(jwtOptions.SecretKey) || jwtOptions.SecretKey.Length < MinSecretKeyLength)
            {
                throw new InvalidOperationException(
                    $"JWT secret key is missing or too short (needs at least {MinSecretKeyLength} characters). " +
                    "Provide it via the JWT__SecretKey environment variable (see JWT_SECRET_KEY in .env) " +
                    "or the JWT:SecretKey configuration value.");
            }

            if (jwtOptions.ExpiryMinutes <= 0)
            {
                throw new InvalidOperationException(
                    $"JWT:ExpiryMinutes is missing or invalid ('{jwtOptions.ExpiryMinutes}'); it must be greater than 0, " +
                    "otherwise tokens expire immediately and every login fails.");
            }

            var issuerKey = Encoding.UTF8.GetBytes(jwtOptions.SecretKey);

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    RequireExpirationTime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(issuerKey),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true
                };
                x.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        context.Token = context.Request.Cookies[jwtOptions.AuthorizationCookieKey];
                        return Task.CompletedTask;
                    }
                };
            });

            services.Configure<JwtOptions>(jwtSection);
        }
    }
}
