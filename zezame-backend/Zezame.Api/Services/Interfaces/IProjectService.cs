using Zezame.Api.Models.DTOs;
using Zezame.Domain.Entities;

namespace Zezame.Api.Services.Interfaces {
    public interface IProjectService {
        Task<ProjectDto> CreateAsync(Guid coordinatorId, CreateProjectDto dto);
        Task<IEnumerable<ProjectDto>> GetProjectsAsync(Guid userId, ProjectStatus? status = null);
        Task<ProjectDto?> GetByIdAsync(Guid id);
        Task<ProjectDto> UpdateAsync(Guid projectId, Guid userId, UpdateProjectDto dto);
        Task DeleteAsync(Guid projectId, Guid userId);
        Task AddMemberAsync(Guid projectId, Guid coordinatorId, Guid userId);
    }
}
