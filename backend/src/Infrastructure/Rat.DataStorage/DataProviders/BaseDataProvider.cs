using System;
using System.Data.Common;
using System.Threading.Tasks;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Mapping;
using LinqToDB.DataProvider;
using Rat.DataStorage.Mapping;
using Rat.Domain;
using Rat.Domain.Infrastructure;

namespace Rat.DataStorage.DataProviders
{
    /// <summary>
    /// Base abstract class for data providers
    /// </summary>
    public abstract class BaseDataProvider : IDisposable
    {
        /// <summary>
        /// Reusable query context for the lifetime of this provider instance (one per scope/request).
        /// linq2db's DataContext opens and closes the underlying connection per query, so it is meant
        /// to be reused; it is disposed together with the provider by the DI container at scope end.
        /// </summary>
        private DataContext _queryDataContext;

        protected abstract IDataProvider LinqToDbDataProvider { get; }

        protected abstract DbConnection GetInternalDbConnection(string connectionString);

        public virtual async Task<TEntity> InsertEntityAsync<TEntity>(TEntity entity) where TEntity : TableEntity
        {
            using var dataContext = CreateDataConnection();
            entity.Id = await dataContext.InsertWithInt32IdentityAsync(entity);

            return entity;
        }

        public virtual async Task UpdateEntityAsync<TEntity>(TEntity entity) where TEntity : TableEntity
        {
            using var dataContext = CreateDataConnection();
            await dataContext.UpdateAsync(entity);
        }

        public virtual async Task DeleteEntityAsync<TEntity>(TEntity entity) where TEntity : TableEntity
        {
            using var dataContext = CreateDataConnection();
            await dataContext.DeleteAsync(entity);
        }

        public virtual ITable<TEntity> GetTable<TEntity>() where TEntity : TableEntity
        {
            _queryDataContext ??= new DataContext(LinqToDbDataProvider, GetCurrentConnectionString())
            {
                MappingSchema = GetMappingSchema()
            };

            return _queryDataContext.GetTable<TEntity>();
        }

        /// <summary>
        /// Dispose the reusable query context. Invoked by the DI container when the
        /// scope (request) that owns this provider instance ends.
        /// </summary>
        public void Dispose()
        {
            ((IDisposable)_queryDataContext)?.Dispose();
            _queryDataContext = null;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Get provider mapping schema
        /// </summary>
        /// <returns>instance of mapping schema</returns>
        private MappingSchema GetMappingSchema()
        {
            return Singleton<MappingSchema>.GetOrCreate(() => new MappingSchema(LinqToDbDataProvider.Name)
            {
                MetadataReader = new FluentMigratorMetadataProvider()
            });
        }

        /// <summary>
        /// Create database connection
        /// </summary>
        /// <returns>database connection</returns>
        protected virtual DataConnection CreateDataConnection()
        {
            return CreateDataConnection(LinqToDbDataProvider);
        }

        /// <summary>
        /// Create database connection by provider
        /// </summary>
        /// <param name="dataProvider">specific data provider</param>
        /// <returns>database connection</returns>
        /// <exception cref="ArgumentNullException"></exception>
        protected virtual DataConnection CreateDataConnection(IDataProvider dataProvider)
        {
            if (dataProvider is null)
                throw new ArgumentNullException(nameof(dataProvider));

            var dataConnection = new DataConnection(dataProvider, CreateDbConnection(), GetMappingSchema())
            {
                CommandTimeout = 60 //DataSettingsManager.GetSqlCommandTimeout()
            };

            return dataConnection;
        }

        /// <summary>
        /// Create database connection by connection string
        /// </summary>
        /// <param name="connectionString">connection string</param>
        /// <returns>database connection</returns>
        protected virtual DbConnection CreateDbConnection(string connectionString = null)
        {
            return GetInternalDbConnection(!string.IsNullOrEmpty(connectionString) ? connectionString : GetCurrentConnectionString());
        }

        /// <summary>
        /// Get current data provider connection string
        /// </summary>
        /// <returns>database connection string</returns>
        protected string GetCurrentConnectionString()
        {
            return DatabaseSettingsManager.GetSettings().ConnectionString;
        }
    }
}
