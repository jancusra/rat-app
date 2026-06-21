using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using FluentMigrator;
using FluentMigrator.Builders.Schema;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;
using Rat.Domain;
using Rat.Domain.Infrastructure;
using Rat.DataStorage.DataProviders;
using Rat.DataStorage.Migrations;

namespace Rat.DataStorage
{
    public partial class AppDbStart : IAppStart
    {
        /// <summary>
        /// Method to configure all database services at application start
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            var migrationAssemblies = new AppTypeFinder().FindClassesOfType<MigrationBase>()
                .Select(x => x.Assembly)
                .Where(a => !string.IsNullOrEmpty(a.FullName) && !a.FullName.Contains("FluentMigrator.Runner"))
                .Distinct()
                .ToArray();

            services
                .AddFluentMigratorCore()

                .AddScoped<IMigrationRunner, MigrationRunner>()
                .AddScoped<ISchemaExpressionRoot, SchemaExpressionRoot>()
                .AddScoped<IProcessorAccessor, DbProcessorAccessor>()

                .AddScoped<ICreationTableManager, CreationTableManager>()
                .AddScoped<IAlterationTableManager, AlterationTableManager>()
                .AddScoped<IConnectionStringAccessor>(x => DatabaseSettingsManager.GetSettings())

                .ConfigureRunner(rb =>
                    rb.AddSqlServer().AddMySql5()/*.AddPostgres()*/
                        .ScanIn(migrationAssemblies).For.Migrations())

                .AddTransient<IDataProviderManager, DataProviderManager>()
                .AddTransient(serviceProvider =>
                    serviceProvider.GetRequiredService<IDataProviderManager>().DataProvider)

                .AddScoped(typeof(IRepository), typeof(Repository));
        }

        public int Order => 10;
    }
}
