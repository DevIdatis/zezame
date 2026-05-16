using Microsoft.EntityFrameworkCore;
using Zezame.Domain.Entities;
using Zezame.Domain.Interfaces;
using Zezame.Infrastructure.Persistence;

namespace Zezame.Infrastructure.Repositories {
    public class ActivityRepository(ZezameDbContext context) : Repository<Activity>(context), IActivityRepository {
        public async Task<IEnumerable<Activity>> GetByProjectAsync(Guid projectId)
            => await _dbSet
                .Where(a => a.ProjectId == projectId && !a.IsDeleted)
                .Include(a => a.AssignedTo)
                .ToListAsync();

        public async Task<IEnumerable<Activity>> GetByUserAsync(Guid userId)
            => await _dbSet
                .Where(a => a.CreatedById == userId && !a.IsDeleted)
                .Include(a => a.Project)
                .ToListAsync();

        public async Task<IEnumerable<Activity>> GetByAssigneeAsync(Guid userId)
            => await _dbSet
                .Where(a => a.AssignedToId == userId && !a.IsDeleted)
                .Include(a => a.Project)
                .ToListAsync();
    }
}
