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

        /// <summary>
        /// Ambient transactional connection set for the duration of <see cref="ExecuteInTransactionAsync"/>.
        /// While non-null, insert/update/delete run on it instead of opening their own connection, so they
        /// share one transaction. Single instance per scope/request and only used sequentially.
        /// </summary>
        private DataConnection _transactionConnection;

        protected abstract IDataProvider LinqToDbDataProvider { get; }

        protected abstract DbConnection GetInternalDbConnection(string connectionString);

        public virtual async Task<TEntity> InsertEntityAsync<TEntity>(TEntity entity) where TEntity : TableEntity
        {
            return await ExecuteOnConnectionAsync(async connection =>
            {
                entity.Id = await connection.InsertWithInt32IdentityAsync(entity);
                return entity;
            });
        }

        public virtual async Task UpdateEntityAsync<TEntity>(TEntity entity) where TEntity : TableEntity
        {
            await ExecuteOnConnectionAsync(connection => connection.UpdateAsync(entity));
        }

        public virtual async Task DeleteEntityAsync<TEntity>(TEntity entity) where TEntity : TableEntity
        {
            await ExecuteOnConnectionAsync(connection => connection.DeleteAsync(entity));
        }

        public virtual async Task ExecuteInTransactionAsync(Func<Task> action)
        {
            if (action is null)
                throw new ArgumentNullException(nameof(action));

            // Already inside a transaction (nested call) -> join it so the whole unit of work
            // commits or rolls back together.
            if (_transactionConnection != null)
            {
                await action();
                return;
            }

            using var dataConnection = CreateDataConnection();
            await dataConnection.BeginTransactionAsync();
            _transactionConnection = dataConnection;

            try
            {
                await action();
                await dataConnection.CommitTransactionAsync();
            }
            catch
            {
                await dataConnection.RollbackTransactionAsync();
                throw;
            }
            finally
            {
                _transactionConnection = null;
            }
        }

        /// <summary>
        /// Run a write operation on the ambient transaction connection when a transaction is active,
        /// otherwise on a fresh short-lived connection disposed right after.
        /// </summary>
        /// <typeparam name="TResult">operation result type</typeparam>
        /// <param name="operation">operation to run against the connection</param>
        /// <returns>operation result</returns>
        private async Task<TResult> ExecuteOnConnectionAsync<TResult>(Func<DataConnection, Task<TResult>> operation)
        {
            if (_transactionConnection != null)
            {
                return await operation(_transactionConnection);
            }

            using var dataConnection = CreateDataConnection();
            return await operation(dataConnection);
        }

        public virtual ITable<TEntity> GetTable<TEntity>() where TEntity : TableEntity
        {
            // Not thread-safe by design: the provider is scoped per request and calls run
            // sequentially, so this lazy init is safe. Do NOT parallelize repository calls
            // (e.g. Task.WhenAll) sharing this provider — the DataContext isn't thread-safe.
            // NOTE: reads go through this DataContext (its own connection) and do NOT join the
            // ambient transaction from ExecuteInTransactionAsync — only writes do. So a query
            // run inside a transaction won't see that transaction's uncommitted changes and may
            // even deadlock under stricter isolation. Don't rely on read-after-write within a
            // transaction until GetTable also honours _transactionConnection.
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
                CommandTimeout = 60
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
