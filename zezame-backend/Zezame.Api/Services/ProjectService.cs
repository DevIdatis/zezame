using Zezame.Api.Models.DTOs;
using Zezame.Api.Services.Interfaces;
using Zezame.Domain.Entities;
using Zezame.Domain.Interfaces;

namespace Zezame.Api.Services {
    public class ProjectService(
        IProjectRepository projects,
        IUserRepository users,
        Interfaces.IAuthorizationService auth) : IProjectService {
        private readonly IProjectRepository _projects = projects;
        private readonly IUserRepository _users = users;
        private readonly Interfaces.IAuthorizationService _auth = auth;

        public async Task<ProjectDto> CreateAsync(Guid coordinatorId, CreateProjectDto dto) {
            // Verificar que es Coordinator o Admin
            await _auth.EnsureCanCreateProjectsAsync(coordinatorId);

            var project = new Project {
                Name = dto.Name,
                Description = dto.Description,
                Color = dto.Color ?? "#3b82f6",
                OwnerId = coordinatorId
            };

            await _projects.AddAsync(project);

            return MapToDto(project);
        }

        public async Task<IEnumerable<ProjectDto>> GetUserProjectsAsync(Guid userId) {
            var projects = await _projects.GetByUserAsync(userId);
            return projects.Select(MapToDto);
        }

        public async Task<ProjectDto?> GetByIdAsync(Guid id) {
            var project = await _projects.GetByIdWithMembersAsync(id);
            return project == null ? null : MapToDto(project);
        }

        public async Task AddMemberAsync(Guid projectId, Guid coordinatorId, Guid userId) {
            await _auth.EnsureCanManageProjectAsync(coordinatorId, projectId);

            var project = await _projects.GetByIdAsync(projectId);
            if (project == null) throw new Exception("Proyecto no encontrado");

            var user = await _users.GetByIdAsync(userId);
            if (user == null) throw new Exception("Usuario no encontrado");

            if (await _projects.IsMemberAsync(projectId, userId))
                throw new InvalidOperationException("Usuario ya es miembro");

            var member = new ProjectMember {
                ProjectId = projectId,
                UserId = userId
            };

            // Agregar mediante contexto o repositorio específico
        }

        private static ProjectDto MapToDto(Project p) => new(
            p.Id,
            p.Name,
            p.Color,
            p.Description,
            p.OwnerId,
            p.Owner?.FullName ?? "Unknown",
            p.Members?.Count ?? 0,
            p.Activities?.Count ?? 0,
            p.CreatedAt
        );
    }
}
