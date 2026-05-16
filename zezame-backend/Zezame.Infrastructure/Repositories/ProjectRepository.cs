using Microsoft.EntityFrameworkCore;
using Zezame.Domain.Entities;
using Zezame.Domain.Interfaces;
using Zezame.Infrastructure.Persistence;

namespace Zezame.Infrastructure.Repositories {
    public class ProjectRepository(ZezameDbContext context) : Repository<Project>(context), IProjectRepository {
        public async Task<IEnumerable<Project>> GetByUserAsync(Guid userId) {
            var owned = await _dbSet.Where(p => p.OwnerId == userId && !p.IsDeleted).ToListAsync();
            var memberOf = await _context.ProjectMembers
                .Where(pm => pm.UserId == userId)
                .Select(pm => pm.Project)
                .Where(p => !p.IsDeleted)
                .ToListAsync();

            return owned.Concat(memberOf).DistinctBy(p => p.Id);
        }

        public async Task<IEnumerable<Project>> GetByOwnerAsync(Guid ownerId)
            => await _dbSet.Where(p => p.OwnerId == ownerId && !p.IsDeleted).ToListAsync();

        public async Task<Project?> GetByIdWithMembersAsync(Guid id)
            => await _dbSet
                .Include(p => p.Members)
                .ThenInclude(m => m.User)
                .Include(p => p.Activities)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

        public async Task<bool> IsMemberAsync(Guid projectId, Guid userId)
            => await _context.ProjectMembers
                .AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);
    }
}
