using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LinqToDB;
using Rat.Domain;
using Rat.Domain.EntityTypes;

namespace Rat.DataStorage
{
    /// <summary>
    /// Methods defining basic database operations with entities
    /// </summary>
    public partial class Repository : IRepository
    {
        private readonly IDbDataProvider _dataProvider;

        public Repository(
            IDbDataProvider dataProvider)
        {
            _dataProvider = dataProvider;
        }

        public virtual async Task<TEntity> GetByIdAsync<TEntity>(int id) where TEntity : TableEntity
        {
            return await _dataProvider.GetTable<TEntity>().FirstOrDefaultAsync(x => x.Id == id);
        }

        public virtual async Task InsertAsync<TEntity>(TEntity entity) where TEntity : TableEntity
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (entity is IAuditable)
            {
                ((IAuditable)entity).CreatedUTC = DateTime.UtcNow;
            }

            await _dataProvider.InsertEntityAsync(entity);
        }

        public virtual async Task UpdateAsync<TEntity>(TEntity entity) where TEntity : TableEntity
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (entity is IAuditable)
            {
                ((IAuditable)entity).ModifiedUTC = DateTime.UtcNow;
            }

            await _dataProvider.UpdateEntityAsync(entity);
        }

        public virtual async Task DeleteAsync<TEntity>(int id) where TEntity : TableEntity
        {
            var entity = await GetByIdAsync<TEntity>(id);

            if (entity != null)
            {
                if (entity is ISoftDelete)
                {
                    ((ISoftDelete)entity).Deleted = true;
                    await _dataProvider.UpdateEntityAsync(entity);
                }
                else
                {
                    await _dataProvider.DeleteEntityAsync(entity);
                }
            }
        }

        public virtual async Task<IList<TEntity>> GetAllAsync<TEntity>() where TEntity : TableEntity
        {
            var queryAll = Table<TEntity>();

            if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)))
            {
                queryAll = queryAll.Where(BuildNotDeletedPredicate<TEntity>());
            }

            return await queryAll.ToListAsync();
        }

        /// <summary>
        /// Build a "not soft-deleted" predicate (e => !e.Deleted) typed over the concrete
        /// entity. Accessing the concrete property (instead of OfType on the ISoftDelete
        /// interface) translates reliably to a SQL WHERE clause in LinqToDB.
        /// </summary>
        /// <typeparam name="TEntity">entity type implementing ISoftDelete</typeparam>
        /// <returns>predicate selecting only non-deleted entities</returns>
        private static Expression<Func<TEntity, bool>> BuildNotDeletedPredicate<TEntity>() where TEntity : TableEntity
        {
            var parameter = Expression.Parameter(typeof(TEntity), "e");
            var deletedProperty = Expression.Property(parameter, nameof(ISoftDelete.Deleted));

            return Expression.Lambda<Func<TEntity, bool>>(Expression.Not(deletedProperty), parameter);
        }

        public virtual IQueryable<TEntity> Table<TEntity>() where TEntity : TableEntity
        {
            return _dataProvider.GetTable<TEntity>();
        }
    }
}