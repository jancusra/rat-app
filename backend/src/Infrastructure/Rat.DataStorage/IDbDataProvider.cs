using System.Threading.Tasks;
using LinqToDB;
using Rat.Domain;

namespace Rat.DataStorage
{
    public partial interface IDbDataProvider
    {
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
