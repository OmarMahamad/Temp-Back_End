using BackEnd.Infrastructure.DataBase;
using BackEnd.Infrastructure.Interface;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BackEnd.Infrastructure.Implementation
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly DbApp _dbApp;
        protected readonly DbSet<T> _dbSet;

        public Repository(DbApp dbApp)
        {
            _dbApp = dbApp;
            _dbSet = _dbApp.Set<T>();
        }

        #region Add Item
        public async Task AddItemAsync(T item)
        {
            await _dbSet.AddAsync(item);
        }
        #endregion

        #region Delete Item
        public async Task DeleteItemAsync(Guid id, bool isRange = false)
        {
            var item = await GetItemByIdAsync(id);
            if (item == null)
                throw new KeyNotFoundException("العنصر المطلوب حذفه غير موجود.");

            if (isRange)
                _dbSet.RemoveRange(item);
            else
                _dbSet.Remove(item);
        }

        public Task DeleteItemAsync(T entity)
        {
            _dbSet.Remove(entity);
            return Task.CompletedTask;
        }
        #endregion

        #region Get Item(s)
        public async Task<T?> GetItemAsync(
            Expression<Func<T, bool>> predicate,
            Func<IQueryable<T>, IQueryable<T>>? include = null)
        {
            IQueryable<T> query = _dbSet;

            if (include != null)
                query = include(query);

            return await query.FirstOrDefaultAsync(predicate);
        }

        public async Task<List<T>> GetAllItemsAsync(
            Expression<Func<T, bool>>? predicate = null,
            Func<IQueryable<T>, IQueryable<T>>? include = null)
        {
            IQueryable<T> query = _dbSet;

            if (include != null)
                query = include(query);

            if (predicate != null)
                query = query.Where(predicate);

            return await query.ToListAsync();
        }

        public async Task<T?> GetItemByIdAsync(Guid id, Func<IQueryable<T>, IQueryable<T>>? include = null)
        {
            IQueryable<T> query = _dbSet;

            if (include != null)
                query = include(query);

            return await query.FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == id);
        }
        #endregion

        #region Update Item
        public async Task UpdateItemAsync(T item, Guid id)
        {
            var existing = await GetItemByIdAsync(id);
            if (existing == null)
                throw new KeyNotFoundException("العنصر غير موجود للتحديث.");

            _dbSet.Entry(existing).CurrentValues.SetValues(item);
        }
        #endregion

        #region Optional generic queries
        public Task<List<T>> GetAllItemsAsync()
        {
            return _dbSet.ToListAsync();
        }

        public async Task<List<TResult>> GetAllItemsAsync<TResult>(
            Expression<Func<T, bool>> predicate,
            Expression<Func<T, TResult>> selector,
            Func<IQueryable<T>, IQueryable<T>>? include = null)
        {
            IQueryable<T> query = _dbSet;

            if (include != null)
                query = include(query);

            return await query.Where(predicate).Select(selector).ToListAsync();
        }
        #endregion
    }
}
