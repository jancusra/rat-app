using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FluentMigrator.Runner;

namespace Rat.Api
{
    public class Program
    {
        /// <summary>
        /// The main starting point of the application
        /// </summary>
        /// <param name="args">app arguments</param>
        public static async Task Main(string[] args)
        {
            var webHost = CreateHostBuilder(args).Build();
            ApplyUpMigration(webHost.Services);

            await webHost.RunAsync();
        }

        /// <summary>
        /// Creating an application host builder by configuration
        /// </summary>
        /// <param name="args">app arguments</param>
        /// <returns>final host builder</returns>
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

        /// <summary>
        /// Update the database model (apply configured migrations)
        /// </summary>
        /// <param name="serviceProvider">web services</param>
        private static void ApplyUpMigration(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
                runner.MigrateUp();
            }
        }
    }
}
