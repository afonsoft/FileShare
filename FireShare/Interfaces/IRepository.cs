using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace FireShare.Interfaces
{
    public interface IRepository<TEntity, TKey> where TEntity : class
    {
        Task<int> AddAsync(TEntity entity);
        Task<int> AddRangeAsync(IEnumerable<TEntity> entity);
        Task<int> DeleteAsync(TEntity entity);
        Task<int> DeleteByIdAsync(TKey id);
        Task<int> DeleteRangeAsync(Expression<Func<TEntity, bool>> filter);
        Task<int> DeleteRangeAsync(IEnumerable<TEntity> entity);
        Task<List<TEntity>> GetAsync();
        Task<List<TEntity>> GetAsync(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, object>> orderBy = null);
        Task<TEntity> GetByIdAsync(TKey id);
        Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> filter = null);
        Task<KeyValuePair<int, List<TEntity>>> GetPagination(Expression<Func<TEntity, bool>> filter, int page = 1, int count = 10);
        Task<KeyValuePair<int, List<TEntity>>> GetPagination(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, object>> orderBy, Expression<Func<TEntity, object>> orderByDescending, int page = 1, int count = 10);
        Task<int> UpdateAsync(TEntity entity);
        Task<int> UpdateRangeAsync(IEnumerable<TEntity> entity);
        Task<int> UpdateByIdAsync(TEntity entity, TKey id);
    }
}
