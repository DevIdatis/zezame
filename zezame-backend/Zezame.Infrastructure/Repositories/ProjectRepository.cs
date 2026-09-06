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

        public async Task<IEnumerable<Project>> GetAllWithStatsAsync(Guid userId, ProjectStatus? status = null) {
            var query = _dbSet
                .Where(p => !p.IsDeleted &&
                       (p.OwnerId == userId || p.Members.Any(m => m.UserId == userId)))
                .Include(p => p.Owner)
                .Include(p => p.Members)
                .Include(p => p.Activities)
                .Include(p => p.TimeEntries)
                .AsQueryable();

            if (status.HasValue)
                query = query.Where(p => p.Status == status.Value);

            return await query
                .AsNoTracking()
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task AddMemberAsync(ProjectMember member) {
            await _context.ProjectMembers.AddAsync(member);
            await _context.SaveChangesAsync();
        }
    }
}
