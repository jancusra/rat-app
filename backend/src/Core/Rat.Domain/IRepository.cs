using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rat.Domain
{
    /// <summary>
    /// Methods defining basic database operations with entities
    /// </summary>
    public partial interface IRepository
    {
        /// <summary>
        /// Get specific entity by ID
        /// </summary>
        /// <typeparam name="TEntity">the type of entity to get</typeparam>
        /// <param name="id">entity identifier</param>
        /// <returns>found entity or null</returns>
        Task<TEntity> GetByIdAsync<TEntity>(int id) where TEntity : TableEntity;

        /// <summary>
        /// Insert entity to the database
        /// </summary>
        /// <typeparam name="TEntity">the type of entity to insert</typeparam>
        /// <param name="entity">entity</param>
        Task InsertAsync<TEntity>(TEntity entity) where TEntity : TableEntity;

        /// <summary>
        /// Update entity in the database
        /// </summary>
        /// <typeparam name="TEntity">the type of entity to update</typeparam>
        /// <param name="entity">entity</param>
        Task UpdateAsync<TEntity>(TEntity entity) where TEntity : TableEntity;

        /// <summary>
        /// Delete entity in the database by ID
        /// </summary>
        /// <typeparam name="TEntity">the type of entity to delete</typeparam>
        /// <param name="id">entity identifier</param>
        Task DeleteAsync<TEntity>(int id) where TEntity : TableEntity;

        /// <summary>
        /// Get all entities
        /// </summary>
        /// <typeparam name="TEntity">the type of entity to get</typeparam>
        /// <returns>collection of all entities</returns>
        Task<IList<TEntity>> GetAllAsync<TEntity>() where TEntity : TableEntity;

        /// <summary>
        /// Represents repository table
        /// </summary>
        /// <typeparam name="TEntity">the type of entity</typeparam>
        /// <returns>queryable of all entities</returns>
        IQueryable<TEntity> Table<TEntity>() where TEntity : TableEntity;
    }
}
