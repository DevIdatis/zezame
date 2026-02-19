using Microsoft.EntityFrameworkCore;
using System;
using Zezame.Domain.Entities;
using Zezame.Domain.Interfaces;
using Zezame.Infrastructure.Persistence;

namespace Zezame.Infrastructure.Repositories {
    public class TimeEntryRepository : ITimeEntryRepository {
        private readonly ZezameDbContext _db;

        public TimeEntryRepository(ZezameDbContext db) {
            _db = db;
        }

        public Task<TimeEntry?> GetRunningAsync(Guid userId, EntryType type = EntryType.Work, CancellationToken ct = default) {
            return _db.TimeEntries
                      .FirstOrDefaultAsync(x => x.UserId == userId && x.Type == type && !x.EndTime.HasValue, ct);
        }

        public async Task AddAsync(TimeEntry entry, CancellationToken ct = default) {
            _db.TimeEntries.Add(entry);
            await _db.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(TimeEntry entry, CancellationToken ct = default) {
            _db.TimeEntries.Update(entry);
            await _db.SaveChangesAsync(ct);
        }

        public async Task<IReadOnlyList<TimeEntry>> ListAsync(Guid userId, DateTime from, DateTime to, CancellationToken ct = default) {
            return await _db.TimeEntries
                            .Where(x => x.UserId == userId && x.StartTime >= from && x.StartTime <= to)
                            .OrderByDescending(x => x.StartTime)
                            .ToListAsync(ct);
        }
    }
}
