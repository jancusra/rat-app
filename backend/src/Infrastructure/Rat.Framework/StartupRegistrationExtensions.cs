using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rat.Domain.Options;
using Rat.Domain.Types;

namespace Rat.Framework
{
    public static class StartupRegistrationExtensions
    {
        /// <summary>
        /// Name of the CORS policy registered for the frontend origin(s).
        /// </summary>
        public const string CorsPolicyName = "RatFrontendCors";

        /// <summary>
        /// Add application options at startup
        /// </summary>
        /// <param name="services">collection of services</param>
        /// <param name="configuration">application configuration</param>
        public static void AddApplicationOptions(this IServiceCollection services, IConfiguration configuration)
        {
            var usersSection = configuration.GetSection("User");
            var usersOptions = new UserOptions();
            usersSection.Bind(usersOptions);

            if (!Enum.IsDefined(typeof(HashType), usersOptions.PasswordHashing))
            {
                throw new InvalidOperationException(
                    $"User:PasswordHashing is missing or invalid ('{usersOptions.PasswordHashing}'). " +
                    $"Set it to a defined {nameof(HashType)} value (e.g. Pbkdf2SHA512).");
            }

            services.Configure<UserOptions>(usersSection);
        }

        /// <summary>
        /// Register a CORS policy for the configured frontend origin(s).
        /// Only does anything when "Cors:AllowedOrigins" is set; otherwise the
        /// API stays without CORS (correct for same-origin / reverse-proxy setups).
        /// AllowCredentials is required so the browser sends the auth cookie, which
        /// in turn forbids a wildcard origin — hence the explicit origin list.
        /// </summary>
        /// <param name="services">collection of services</param>
        /// <param name="configuration">application configuration</param>
        public static void AddApplicationCors(this IServiceCollection services, IConfiguration configuration)
        {
            var corsSection = configuration.GetSection("Cors");
            var corsOptions = new CorsOptions();
            corsSection.Bind(corsOptions);

            var origins = SplitOrigins(corsOptions.AllowedOrigins);

            if (origins.Length == 0)
                return;

            services.AddCors(options =>
            {
                options.AddPolicy(CorsPolicyName, policy =>
                    policy.WithOrigins(origins)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials());
            });
        }

        /// <summary>
        /// Apply the CORS policy registered by <see cref="AddApplicationCors"/>.
        /// No-op when no origins were configured, so it is always safe to call.
        /// Must run after UseRouting and before UseAuthentication.
        /// </summary>
        /// <param name="app">application builder</param>
        /// <param name="configuration">application configuration</param>
        public static void UseApplicationCors(this IApplicationBuilder app, IConfiguration configuration)
        {
            var origins = SplitOrigins(configuration["Cors:AllowedOrigins"]);

            if (origins.Length == 0)
                return;

            app.UseCors(CorsPolicyName);
        }

        /// <summary>
        /// Parse a comma-/semicolon-separated origin list into trimmed, non-empty entries.
        /// </summary>
        /// <param name="allowedOrigins">raw configured value</param>
        /// <returns>distinct origins (empty when nothing is configured)</returns>
        private static string[] SplitOrigins(string allowedOrigins)
        {
            if (string.IsNullOrWhiteSpace(allowedOrigins))
                return Array.Empty<string>();

            return allowedOrigins
                .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(origin => origin.Trim())
                .Where(origin => origin.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
    }
}
