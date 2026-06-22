using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
        /// Register a CORS policy for the frontend. Does nothing unless either
        /// "Cors:AllowedOrigins" is set or "Cors:AllowPrivateNetwork" is true;
        /// otherwise the API stays without CORS (correct for same-origin /
        /// reverse-proxy setups). AllowCredentials is required so the browser
        /// sends the auth cookie, which forbids a wildcard origin — so we either
        /// use the explicit list or reflect each allowed origin individually.
        /// An explicit list, when present, takes precedence over private-network mode.
        /// </summary>
        /// <param name="services">collection of services</param>
        /// <param name="configuration">application configuration</param>
        public static void AddApplicationCors(this IServiceCollection services, IConfiguration configuration)
        {
            var corsOptions = new CorsOptions();
            configuration.GetSection("Cors").Bind(corsOptions);

            var origins = SplitOrigins(corsOptions.AllowedOrigins);

            if (origins.Length == 0 && !corsOptions.AllowPrivateNetwork)
                return;

            services.AddCors(options =>
            {
                options.AddPolicy(CorsPolicyName, policy =>
                {
                    policy.AllowAnyHeader().AllowAnyMethod().AllowCredentials();

                    if (origins.Length > 0)
                        policy.WithOrigins(origins);
                    else
                        policy.SetIsOriginAllowed(IsLoopbackOrPrivateOrigin);
                });
            });
        }

        /// <summary>
        /// Apply the CORS policy registered by <see cref="AddApplicationCors"/>.
        /// No-op when no policy was configured, so it is always safe to call.
        /// Must run after UseRouting and before UseAuthentication.
        /// </summary>
        /// <param name="app">application builder</param>
        /// <param name="configuration">application configuration</param>
        public static void UseApplicationCors(this IApplicationBuilder app, IConfiguration configuration)
        {
            var corsOptions = new CorsOptions();
            configuration.GetSection("Cors").Bind(corsOptions);

            if (SplitOrigins(corsOptions.AllowedOrigins).Length == 0 && !corsOptions.AllowPrivateNetwork)
                return;

            app.UseCors(CorsPolicyName);
        }

        /// <summary>
        /// True when the origin's host is localhost, a loopback address, or a
        /// private-network IP (RFC 1918 / link-local / IPv6 ULA). Used to let
        /// both localhost and LAN clients in without listing every IP.
        /// </summary>
        /// <param name="origin">request origin, e.g. "http://192.168.1.50:3000"</param>
        /// <returns>true when the origin is loopback or private-network</returns>
        private static bool IsLoopbackOrPrivateOrigin(string origin)
        {
            if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                return false;

            if (string.Equals(uri.Host, "localhost", StringComparison.OrdinalIgnoreCase))
                return true;

            if (!IPAddress.TryParse(uri.Host, out var ip))
                return false;

            return IPAddress.IsLoopback(ip) || IsPrivateNetwork(ip);
        }

        /// <summary>
        /// True for private-network addresses: IPv4 RFC 1918 (10/8, 172.16/12,
        /// 192.168/16) and link-local (169.254/16); IPv6 link-local (fe80::/10)
        /// and unique-local (fc00::/7).
        /// </summary>
        /// <param name="ip">address to test</param>
        /// <returns>true when the address is in a private range</returns>
        private static bool IsPrivateNetwork(IPAddress ip)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                var b = ip.GetAddressBytes();
                return b[0] == 10
                    || (b[0] == 172 && b[1] >= 16 && b[1] <= 31)
                    || (b[0] == 192 && b[1] == 168)
                    || (b[0] == 169 && b[1] == 254);
            }

            if (ip.AddressFamily == AddressFamily.InterNetworkV6)
                return ip.IsIPv6LinkLocal || (ip.GetAddressBytes()[0] & 0xFE) == 0xFC;

            return false;
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
