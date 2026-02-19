using Zezame.Domain.Entities;

namespace Zezame.Domain.Interfaces {
    public interface ITimeEntryRepository {
        Task<TimeEntry?> GetRunningAsync(Guid userId, EntryType type = EntryType.Work, CancellationToken ct = default);
        Task AddAsync(TimeEntry entry, CancellationToken ct = default);
        Task UpdateAsync(TimeEntry entry, CancellationToken ct = default);
        Task<IReadOnlyList<TimeEntry>> ListAsync(Guid userId, DateTime from, DateTime to, CancellationToken ct = default);
    }
}
