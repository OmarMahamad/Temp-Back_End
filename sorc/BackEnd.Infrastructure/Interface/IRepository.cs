
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd.Infrastructure.Interface
{
    public interface IRepository<T> where T : class
    {
        Task AddItemAsync(T item);
        Task UpdateItemAsync(T item, Guid id);
        Task DeleteItemAsync(Guid id, bool isrange = false);
        Task DeleteItemAsync(T entity);
        Task<T> GetItemAsync(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IQueryable<T>>? include = null);

        Task<T> GetItemByIdAsync(Guid id, Func<IQueryable<T>, IQueryable<T>>? include = null);
        Task<List<T>> GetAllItemsAsync();
        Task<List<T>> GetAllItemsAsync(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IQueryable<T>>? include = null);

        // لو محتاج projection (اختيار properties محددة)
        Task<List<TResult>> GetAllItemsAsync<TResult>(Expression<Func<T, bool>> predicate, Expression<Func<T, TResult>> selector, Func<IQueryable<T>, IQueryable<T>>? include = null);
    }
}
