using Microsoft.EntityFrameworkCore;
using Zezame.Domain.Base;
using Zezame.Domain.Interfaces;
using Zezame.Infrastructure.Persistence;

namespace Zezame.Infrastructure.Repositories {
    public class Repository<T>(ZezameDbContext context) : IRepository<T> where T : BaseEntity {
        protected readonly ZezameDbContext _context = context;
        protected readonly DbSet<T> _dbSet = context.Set<T>();

        public virtual async Task<T?> GetByIdAsync(Guid id)
            => await _dbSet.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);

        public virtual async Task<IEnumerable<T>> GetAllAsync()
            => await _dbSet.Where(e => !e.IsDeleted).ToListAsync();

        public virtual async Task<T> AddAsync(T entity) {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public virtual async Task UpdateAsync(T entity) {
            entity.UpdatedAt = DateTime.UtcNow;
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
        }

        public virtual async Task DeleteAsync(Guid id) {
            var entity = await _dbSet.FindAsync(id);
            if (entity != null) {
                entity.IsDeleted = true;
                entity.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public virtual async Task<bool> ExistsAsync(Guid id)
            => await _dbSet.AnyAsync(e => e.Id == id && !e.IsDeleted);
    }
}
