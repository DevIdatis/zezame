using Zezame.Domain.Entities;

namespace Zezame.Domain.Interfaces {
    public interface IProjectRepository : IRepository<Project> {
        Task<IEnumerable<Project>> GetByUserAsync(Guid userId);
        Task<IEnumerable<Project>> GetByOwnerAsync(Guid ownerId);
        Task<Project?> GetByIdWithMembersAsync(Guid id);
        Task<bool> IsMemberAsync(Guid projectId, Guid userId);
    }
}
