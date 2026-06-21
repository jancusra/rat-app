using System;
using System.Threading.Tasks;
using LinqToDB;
using Rat.Domain;

namespace Rat.DataStorage
{
    public partial interface IDbDataProvider
    {
        /// <summary>
        /// Run the given action inside a single database transaction. All writes performed
        /// through this provider during the action share the transaction and are committed
        /// together, or rolled back if the action throws.
        /// </summary>
        /// <param name="action">unit of work to run transactionally</param>
        Task ExecuteInTransactionAsync(Func<Task> action);

        /// <summary>
        /// Insert entity to the database
        /// </summary>
        /// <typeparam name="TEntity">the type of entity to insert</typeparam>
        /// <param name="entity">entity</param>
        /// <returns>final inserted entity</returns>
        Task<TEntity> InsertEntityAsync<TEntity>(TEntity entity) where TEntity : TableEntity;

        /// <summary>
        /// Update entity in the database
        /// </summary>
        /// <typeparam name="TEntity">the type of entity to update</typeparam>
        /// <param name="entity">entity</param>
        Task UpdateEntityAsync<TEntity>(TEntity entity) where TEntity : TableEntity;

        /// <summary>
        /// Delete entity in the database
        /// </summary>
        /// <typeparam name="TEntity">the type of entity to delete</typeparam>
        /// <param name="entity">entity</param>
        Task DeleteEntityAsync<TEntity>(TEntity entity) where TEntity : TableEntity;

        /// <summary>
        /// Get data provider entity table
        /// </summary>
        /// <typeparam name="TEntity">the type of entity</typeparam>
        /// <returns>entity table</returns>
        ITable<TEntity> GetTable<TEntity>() where TEntity : TableEntity;
    }
}
