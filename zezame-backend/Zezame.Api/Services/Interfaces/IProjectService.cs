using Zezame.Api.Models.DTOs;

namespace Zezame.Api.Services.Interfaces {
    public interface IProjectService {
        Task<ProjectDto> CreateAsync(Guid coordinatorId, CreateProjectDto dto);
        Task<IEnumerable<ProjectDto>> GetUserProjectsAsync(Guid userId);
        Task<ProjectDto?> GetByIdAsync(Guid id);
        Task AddMemberAsync(Guid projectId, Guid coordinatorId, Guid userId);
    }
}
