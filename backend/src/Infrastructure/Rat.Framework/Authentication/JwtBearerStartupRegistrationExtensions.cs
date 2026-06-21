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
        /// Startup method to register JWT bearer authentication
        /// </summary>
        /// <param name="services">collection of services</param>
        /// <param name="configuration">application configuration</param>
        public static void AddJwtBearerAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtSection = configuration.GetSection("JWT");
            var jwtOptions = new JwtOptions();
            jwtSection.Bind(jwtOptions);
            var issuerKey = Encoding.ASCII.GetBytes(jwtOptions.SecretKey);

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
                    RequireExpirationTime = false,
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
