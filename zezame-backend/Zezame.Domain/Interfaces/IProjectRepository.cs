using Zezame.Domain.Entities;

namespace Zezame.Domain.Interfaces {
    public interface IProjectRepository : IRepository<Project> {
        Task<IEnumerable<Project>> GetByUserAsync(Guid userId);
        Task<IEnumerable<Project>> GetByOwnerAsync(Guid ownerId);
        Task<Project?> GetByIdWithMembersAsync(Guid id);
        Task<bool> IsMemberAsync(Guid projectId, Guid userId);

        // Proyectos del usuario (propios o como miembro) con navegaciones para calcular stats
        Task<IEnumerable<Project>> GetAllWithStatsAsync(Guid userId, ProjectStatus? status = null);
        Task AddMemberAsync(ProjectMember member);

        // IDs de usuarios con los que el usuario comparte proyecto (propietarios y
        // miembros de sus proyectos, incluido él mismo). Para acotar el ranking de equipo.
        Task<IEnumerable<Guid>> GetTeammateUserIdsAsync(Guid userId);
        Task RemoveMemberAsync(Guid projectId, Guid userId);
    }
}
