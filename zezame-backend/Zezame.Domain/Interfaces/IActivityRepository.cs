using Zezame.Domain.Entities;

namespace Zezame.Domain.Interfaces {
    public interface IActivityRepository : IRepository<Activity> {
        Task<IEnumerable<Activity>> GetByProjectAsync(Guid projectId);
        Task<IEnumerable<Activity>> GetByUserAsync(Guid userId);
        Task<IEnumerable<Activity>> GetByAssigneeAsync(Guid userId);
    }
}
