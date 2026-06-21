using System;
using System.Collections.Generic;
using System.Linq;
using FluentMigrator;
using FluentMigrator.Exceptions;
using FluentMigrator.Runner.Processors;

namespace Rat.DataStorage.Migrations
{
    /// <summary>
    /// Accesses the selected migration processor
    /// </summary>
    /// <remarks>
    /// This is only different from using <see cref="IMigrationProcessor"/>
    /// as constructor parameter when multiple databases should be supported
    public partial class DbProcessorAccessor : IProcessorAccessor
    {
        public DbProcessorAccessor(
            IEnumerable<IMigrationProcessor> processors)
        {
            ConfigureProcessor(processors.ToList());
        }

        /// <summary>
        /// Configure processor by configured data provider
        /// </summary>
        /// <param name="processors">available processors</param>
        /// <exception cref="ProcessorFactoryNotFoundException"></exception>
        protected virtual void ConfigureProcessor(
            IList<IMigrationProcessor> processors)
        {
            if (processors.Count == 0)
            {
                throw new ProcessorFactoryNotFoundException("No migration processor registered.");
            }

            Processor = DatabaseSettingsManager.GetSettings().DataProvider switch
            {
                DatabaseType.SqlServer => FindGenerator(processors, "SqlServer"),
                DatabaseType.MySql => FindGenerator(processors, "MySQL"),
                //DatabaseType.PostgreSQL => FindGenerator(processors, "Postgres"),
                _ => throw new ProcessorFactoryNotFoundException(
                    $@"A migration generator for Data provider type couldn't be found.")
            };
        }

        /// <summary>
        /// Find generator by data provider identifier
        /// </summary>
        /// <param name="processors">available processors</param>
        /// <param name="processorsId">data provider identifier</param>
        /// <returns>found migration processor</returns>
        /// <exception cref="ProcessorFactoryNotFoundException"></exception>
        protected IMigrationProcessor FindGenerator(IList<IMigrationProcessor> processors,
            string processorsId)
        {
            if (processors.FirstOrDefault(p =>
                    p.DatabaseType.Equals(processorsId, StringComparison.OrdinalIgnoreCase) ||
                    p.DatabaseTypeAliases.Any(a => a.Equals(processorsId, StringComparison.OrdinalIgnoreCase))) is
                IMigrationProcessor processor)
                return processor;

            var generatorNames = string.Join(", ",
                processors.Select(p => p.DatabaseType).Union(processors.SelectMany(p => p.DatabaseTypeAliases)));

            throw new ProcessorFactoryNotFoundException(
                $@"A migration generator with the ID {processorsId} couldn't be found. Available generators are: {generatorNames}");
        }

        /// <summary>
        /// Gets the selected migration processor
        /// </summary>
        public IMigrationProcessor Processor { get; protected set; } = null!;
    }
}
