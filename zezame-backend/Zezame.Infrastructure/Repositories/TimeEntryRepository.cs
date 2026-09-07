using Microsoft.EntityFrameworkCore;
using System;
using Zezame.Domain.Entities;
using Zezame.Domain.Interfaces;
using Zezame.Infrastructure.Persistence;

namespace Zezame.Infrastructure.Repositories {
    public class TimeEntryRepository : Repository<TimeEntry>, ITimeEntryRepository {
        public TimeEntryRepository(ZezameDbContext context) : base(context) { }

        public Task<TimeEntry?> GetRunningAsync(Guid userId, EntryType type = EntryType.Work, CancellationToken ct = default) {
            return _dbSet
                      .FirstOrDefaultAsync(x => x.UserId == userId && x.Type == type && !x.EndTime.HasValue, ct);
        }

        public async Task<IReadOnlyList<TimeEntry>> ListAsync(Guid userId, DateTime from, DateTime to, CancellationToken ct = default) {
            return await _dbSet
                            .Where(x => x.UserId == userId && x.StartTime >= from && x.StartTime <= to)
                            .OrderByDescending(x => x.StartTime)
                            .ToListAsync(ct);
        }

        public async Task<IEnumerable<TimeEntry>> GetByUserAsync(Guid userId, DateTime? from = null, DateTime? to = null) {
            var query = _dbSet.Where(te => te.UserId == userId && !te.IsDeleted);

            if (from.HasValue)
                query = query.Where(te => te.StartTime >= from.Value);
            if (to.HasValue)
                query = query.Where(te => te.StartTime <= to.Value);

            return await query
                .OrderByDescending(te => te.StartTime)
                .Include(te => te.Project)
                .Include(te => te.Activity)
                .ToListAsync();
        }

        public async Task<IEnumerable<TimeEntry>> GetAllInRangeAsync(DateTime from, DateTime to)
            => await _dbSet
                .Where(te => te.StartTime >= from && te.StartTime <= to && !te.IsDeleted)
                .Include(te => te.User)
                .ToListAsync();

        public async Task<TimeEntry?> GetActiveByUserAsync(Guid userId)
            => await _dbSet
                .Where(te => te.UserId == userId && !te.EndTime.HasValue && !te.IsDeleted)
                .OrderByDescending(te => te.StartTime)
                .FirstOrDefaultAsync();

        public async Task<IEnumerable<TimeEntry>> GetByProjectAsync(Guid projectId)
            => await _dbSet
                .Where(te => te.ProjectId == projectId && !te.IsDeleted)
                .Include(te => te.User)
                .ToListAsync();

        public async Task<IEnumerable<TimeEntry>> GetByActivityAsync(Guid activityId)
            => await _dbSet
                .Where(te => te.ActivityId == activityId && !te.IsDeleted)
                .ToListAsync();
    }
}
