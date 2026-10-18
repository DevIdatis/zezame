using Zezame.Api.Exceptions;
using Zezame.Api.Models.DTOs;
using Zezame.Api.Services.Interfaces;
using Zezame.Domain.Entities;
using Zezame.Domain.Interfaces;

namespace Zezame.Api.Services {
    public class ActivityService {
        private readonly IActivityRepository _activities;
        private readonly IProjectRepository _projects;
        private readonly IAuthorizationService _auth;
        private readonly NotificationService _notifications;

        public ActivityService(
            IActivityRepository activities,
            IProjectRepository projects,
            IAuthorizationService auth,
            NotificationService notifications) {
            _activities = activities;
            _projects = projects;
            _auth = auth;
            _notifications = notifications;
        }

        public async Task<ActivityDto> CreateAsync(Guid coordinatorId, CreateActivityDto dto) {
            // Verificar acceso al proyecto
            await _auth.EnsureCanManageProjectAsync(coordinatorId, dto.ProjectId);

            var project = await _projects.GetByIdAsync(dto.ProjectId)
                ?? throw new NotFoundException("Proyecto no encontrado");

            var activity = new Activity {
                Name = dto.Name,
                Description = dto.Description,
                Emoji = dto.Emoji,
                ProjectId = dto.ProjectId,
                OrganizationId = project.OrganizationId,
                AssignedToId = dto.AssignedToId,
                CreatedById = coordinatorId,
                EstimatedMinutes = dto.EstimatedMinutes
            };

            await _activities.AddAsync(activity);

            // Avisar a la persona asignada (si no es quien la crea)
            if (dto.AssignedToId.HasValue && dto.AssignedToId.Value != coordinatorId) {
                await _notifications.NotifyAsync(
                    dto.AssignedToId.Value,
                    "Nueva actividad asignada 📌",
                    $"Se te ha asignado la actividad «{activity.Name}».",
                    NotificationType.Activity);
            }

            return MapToDto(activity);
        }

        public async Task<IEnumerable<ActivityDto>> GetMyActivitiesAsync(Guid userId) {
            var activities = await _activities.GetByAssigneeAsync(userId);
            return activities.Select(MapToDto);
        }

        public async Task<IEnumerable<ActivityDto>> GetByProjectAsync(Guid projectId, Guid userId) {
            // Comprobación de pertenencia: impide listar actividades de proyectos ajenos (IDOR).
            await _auth.EnsureCanAccessProjectAsync(userId, projectId);
            var activities = await _activities.GetByProjectAsync(projectId);
            return activities.Select(MapToDto);
        }

        private static ActivityDto MapToDto(Activity a) => new(
            a.Id,
            a.Name,
            a.Description,
            a.Emoji,
            a.Status.ToString(),
            a.ProjectId,
            a.Project?.Name ?? "Unknown",
            a.AssignedTo?.FullName,
            a.EstimatedMinutes,
            a.TimeEntries?.Sum(te => te.DurationMin),
            a.CreatedAt
        );
    }
}
