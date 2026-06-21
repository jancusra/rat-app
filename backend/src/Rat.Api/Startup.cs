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
            app.UseMiddleware<TokenManagerMiddleware>();
            app.UseMiddleware<ErrorWrappingMiddleware>();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
