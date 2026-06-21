using System;
using System.Collections.Generic;
using System.Linq;
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

            /*if (publishEvent)
                await _eventPublisher.EntityInsertedAsync(entity);*/
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

            /*if (publishEvent)
                await _eventPublisher.EntityInsertedAsync(entity);*/
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

                /*if (publishEvent)
                    await _eventPublisher.EntityInsertedAsync(entity);*/
            }
        }

        public virtual async Task<IList<TEntity>> GetAllAsync<TEntity>() where TEntity : TableEntity
        {
            var queryAll = Table<TEntity>();

            if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)))
            {
                queryAll = queryAll.OfType<ISoftDelete>().Where(e => !e.Deleted).OfType<TEntity>();
            }

            return await queryAll.ToListAsync();
        }

        public virtual IQueryable<TEntity> Table<TEntity>() where TEntity : TableEntity
        {
            return _dataProvider.GetTable<TEntity>();
        }
    }
}