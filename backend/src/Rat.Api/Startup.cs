using System;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rat.Domain.Infrastructure;
using Rat.Framework;
using Rat.Framework.Authentication;
using Rat.Framework.Exceptions;
using Rat.Services;

namespace Rat.Api
{
    public class Startup
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public Startup(
            IConfiguration configuration,
            IWebHostEnvironment webHostEnvironment)
        {
            _configuration = configuration;
            _webHostEnvironment = webHostEnvironment;
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services">collection of services</param>
        public void ConfigureServices(IServiceCollection services)
        {
            //see https://docs.microsoft.com/dotnet/framework/network-programming/tls
            ServicePointManager.SecurityProtocol = SecurityProtocolType.SystemDefault;

            CommonSettingsManager.InitWebHostEnvironment(_webHostEnvironment);

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IAppTypeFinder, AppTypeFinder>();
            services.AddSingleton<IReflectionCache, ReflectionCache>();

            ConfigureServicesByLibraries(services);

            services.AddScoped<TokenManagerMiddleware>();
            services.AddScoped<ITokenManager, TokenManager>();
            services.AddScoped<IClaimsPrincipalProvider, HttpContextClaimsPrincipalProvider>();
            services.AddDistributedMemoryCache();

            services.AddScoped<IHashingService, HashingService>();
            services.AddScoped<ILanguageService, LanguageService>();
            services.AddScoped<ILocalizationService, LocalizationService>();
            services.AddScoped<ILogService, LogService>();
            services.AddScoped<IMenuService, MenuService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IEntityService, EntityService>();
            services.AddScoped<IEntityValidationService, EntityValidationService>();

            services.AddControllers();

            services.AddJwtBearerAuthentication(_configuration);
            services.AddApplicationCors(_configuration);
            services.AddApplicationOptions(_configuration);
        }

        /// <summary>
        /// Dynamic library scanning for the Rat project .dlls
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServicesByLibraries(IServiceCollection services)
        {
            var appTypeFinder = new AppTypeFinder();
            var startupInstances = appTypeFinder.FindClassesOfType<IAppStart>();

            var instances = startupInstances
                .Select(startup => (IAppStart)Activator.CreateInstance(startup))
                .OrderBy(startup => startup.Order);

            foreach (var instance in instances)
                instance.ConfigureServices(services);
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">application builder</param>
        /// <param name="env">web host environment</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            // Sits near the top so it also wraps exceptions thrown by routing, authentication,
            // authorization and our own TokenManagerMiddleware below.
            app.UseMiddleware<ErrorWrappingMiddleware>();

            app.UseRouting();

            // CORS must sit between routing and authentication so pre-flight
            // OPTIONS requests are answered and the auth cookie is allowed
            // cross-origin (no-op unless Cors:AllowedOrigins is configured).
            app.UseApplicationCors(_configuration);

            app.UseAuthentication();
            app.UseAuthorization();

            // Token deny-list (logout) check runs after authentication so it only inspects
            // already-authenticated requests, and downstream of ErrorWrappingMiddleware so its
            // own failures get wrapped instead of bubbling out raw.
            app.UseMiddleware<TokenManagerMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
