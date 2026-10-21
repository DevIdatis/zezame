using Zezame.Api.Models.DTOs;
using Zezame.Domain.Entities;
using static Zezame.Api.Models.AuthDtos;

namespace Zezame.Api.Services.Interfaces {
    public interface IProjectService {
        Task<ProjectDto> CreateAsync(Guid coordinatorId, CreateProjectDto dto);
        Task<IEnumerable<ProjectDto>> GetProjectsAsync(Guid userId, ProjectStatus? status = null);
        Task<ProjectDto?> GetByIdAsync(Guid id, Guid userId);
        Task<ProjectDto> UpdateAsync(Guid projectId, Guid userId, UpdateProjectDto dto);
        Task DeleteAsync(Guid projectId, Guid userId);
        Task AddMemberAsync(Guid projectId, Guid coordinatorId, Guid userId);
        Task<IEnumerable<UserOrgDto>> GetMembersSameOrganization(Guid userId);
        Task RemoveMemberAsync(Guid projectId, Guid coordinatorId, Guid userId);
        Task<IEnumerable<UserOrgDto>> GetProjectMembersAsync(Guid projectId, Guid userId);
    }
}
