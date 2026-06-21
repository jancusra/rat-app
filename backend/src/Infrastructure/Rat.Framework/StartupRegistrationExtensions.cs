using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rat.Domain.Options;

namespace Rat.Framework
{
    public static class StartupRegistrationExtensions
    {
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
            services.Configure<UserOptions>(usersSection);
        }
    }
}
