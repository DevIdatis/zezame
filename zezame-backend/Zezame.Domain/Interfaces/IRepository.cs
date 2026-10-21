using System.Linq.Expressions;
using Zezame.Domain.Base;

namespace Zezame.Domain.Interfaces {
    public interface IRepository<T> where T : BaseEntity {
        Task<T?> GetByIdAsync(Guid id);
        Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null);
        Task<T> AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
    }
}
