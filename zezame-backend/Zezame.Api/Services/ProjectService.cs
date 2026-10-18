using Zezame.Api.Exceptions;
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
            // Verificar que puede crear proyectos (Manager/Admin/Owner).
            await _auth.EnsureCanCreateProjectsAsync(coordinatorId);

            var owner = await _users.GetByIdAsync(coordinatorId)
                ?? throw new NotFoundException("Usuario no encontrado");
            if (owner.OrganizationId is null)
                throw new InvalidOperationException("El usuario no pertenece a ninguna organización");

            var project = new Project {
                Name = dto.Name,
                Description = dto.Description,
                Color = dto.Color ?? "from-coral to-yellow",
                Emoji = dto.Emoji,
                Deadline = dto.Deadline,
                Status = ProjectStatus.Active,
                OwnerId = coordinatorId,
                OrganizationId = owner.OrganizationId.Value
            };

            await _projects.AddAsync(project);

            return MapToDto(project, owner.FullName);
        }

        public async Task<IEnumerable<ProjectDto>> GetProjectsAsync(Guid userId, ProjectStatus? status = null) {
            var projects = await _projects.GetAllWithStatsAsync(userId, status);
            return projects.Select(p => MapToDto(p, p.Owner?.FullName ?? "Unknown"));
        }

        public async Task<ProjectDto?> GetByIdAsync(Guid id, Guid userId) {
            await _auth.EnsureCanAccessProjectAsync(userId, id);
            var project = await _projects.GetByIdWithMembersAsync(id);
            return project == null ? null : MapToDto(project, project.Owner?.FullName ?? "Unknown");
        }

        public async Task<ProjectDto> UpdateAsync(Guid projectId, Guid userId, UpdateProjectDto dto) {
            await _auth.EnsureCanManageProjectAsync(userId, projectId);

            var project = await _projects.GetByIdWithMembersAsync(projectId)
                ?? throw new NotFoundException("Proyecto no encontrado");

            if (dto.Name is not null) project.Name = dto.Name;
            if (dto.Description is not null) project.Description = dto.Description;
            if (dto.Color is not null) project.Color = dto.Color;
            if (dto.Emoji is not null) project.Emoji = dto.Emoji;
            if (dto.Deadline.HasValue) project.Deadline = dto.Deadline;
            if (dto.Status is not null && Enum.TryParse<ProjectStatus>(dto.Status, true, out var st))
                project.Status = st;

            await _projects.UpdateAsync(project);
            return MapToDto(project, project.Owner?.FullName ?? "Unknown");
        }

        public async Task DeleteAsync(Guid projectId, Guid userId) {
            await _auth.EnsureCanManageProjectAsync(userId, projectId);
            await _projects.DeleteAsync(projectId); // soft-delete
        }

        public async Task AddMemberAsync(Guid projectId, Guid coordinatorId, Guid userId) {
            await _auth.EnsureCanManageProjectAsync(coordinatorId, projectId);

            var project = await _projects.GetByIdAsync(projectId)
                ?? throw new NotFoundException("Proyecto no encontrado");

            var user = await _users.GetByIdAsync(userId)
                ?? throw new NotFoundException("Usuario no encontrado");

            if (await _projects.IsMemberAsync(projectId, userId))
                throw new InvalidOperationException("Usuario ya es miembro");

            await _projects.AddMemberAsync(new ProjectMember {
                ProjectId = projectId,
                UserId = userId
            });
        }

        private static ProjectDto MapToDto(Project p, string ownerName) {
            var totalMinutes = p.TimeEntries?.Sum(te => te.DurationMin ?? 0) ?? 0;
            var hours = Math.Round(totalMinutes / 60.0, 1);

            var tasksTotal = p.Activities?.Count ?? 0;
            var tasksCompleted = p.Activities?.Count(a => a.Status == ActivityStatus.Done) ?? 0;
            var progress = tasksTotal > 0
                ? (int)Math.Round((double)tasksCompleted / tasksTotal * 100)
                : 0;

            return new ProjectDto(
                p.Id,
                p.Name,
                p.Description,
                p.Color,
                p.Emoji,
                p.Status.ToString(),
                p.Deadline,
                hours,
                progress,
                p.Members?.Count ?? 0,
                tasksCompleted,
                tasksTotal,
                p.OwnerId,
                ownerName,
                p.CreatedAt
            );
        }
    }
}
