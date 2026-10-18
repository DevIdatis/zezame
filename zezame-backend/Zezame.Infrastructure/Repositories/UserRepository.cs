using Microsoft.EntityFrameworkCore;
using Zezame.Domain.Entities;
using Zezame.Domain.Interfaces;
using Zezame.Infrastructure.Persistence;

namespace Zezame.Infrastructure.Repositories {
    public class UserRepository(ZezameDbContext db) : Repository<User>(db), IUserRepository {
        public async Task<User?> GetByEmailAsync(string email)
        => await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower() && !u.IsDeleted);

        public async Task<User?> GetByIdWithProjectsAsync(Guid id)
            => await _dbSet
                .Include(u => u.OwnedProjects)
                .Include(u => u.ProjectMemberships)
                .ThenInclude(pm => pm.Project)
                .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);

        public async Task<IEnumerable<User>> GetByRoleAsync(OrganizationRole role)
            => await _dbSet.Where(u => u.Role == role && !u.IsDeleted).ToListAsync();
    }
}
