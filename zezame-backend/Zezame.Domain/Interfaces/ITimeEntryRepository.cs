using Zezame.Domain.Entities;

namespace Zezame.Domain.Interfaces {
    public interface ITimeEntryRepository : IRepository<TimeEntry> {
        Task<TimeEntry?> GetRunningAsync(Guid userId, EntryType type = EntryType.Work, CancellationToken ct = default);
        Task<IReadOnlyList<TimeEntry>> ListAsync(Guid userId, DateTime from, DateTime to, CancellationToken ct = default);

        Task<IEnumerable<TimeEntry>> GetByUserAsync(Guid userId, DateTime? from = null, DateTime? to = null);
        Task<IEnumerable<TimeEntry>> GetAllInRangeAsync(DateTime from, DateTime to);
        Task<TimeEntry?> GetActiveByUserAsync(Guid userId);
        Task<IEnumerable<TimeEntry>> GetByProjectAsync(Guid projectId);
        Task<IEnumerable<TimeEntry>> GetByActivityAsync(Guid activityId);
    }
}
