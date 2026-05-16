using Zezame.Api.Exceptions;
using Zezame.Api.Services.Interfaces;
using Zezame.Domain.Interfaces;
using static Zezame.Domain.Entities.User;

namespace Zezame.Api.Services {
    public class AuthorizationService : IAuthorizationService {
        private readonly IUserRepository _users;
        private readonly IProjectRepository _projects;

        public AuthorizationService(IUserRepository users, IProjectRepository projects) {
            _users = users;
            _projects = projects;
        }

        public async Task EnsureCanCreateProjectsAsync(Guid userId) {
            var user = await _users.GetByIdAsync(userId);
            if (user == null) throw new UnauthorizedException();

            if (user.Role != UserRole.Coordinator && user.Role != UserRole.Admin)
                throw new ForbiddenException("Solo coordinadores y admins pueden crear proyectos");
        }

        public async Task EnsureCanManageProjectAsync(Guid userId, Guid projectId) {
            var user = await _users.GetByIdAsync(userId);
            if (user == null) throw new UnauthorizedException();

            // Admin puede todo
            if (user.Role == UserRole.Admin) return;

            // Owner puede gestionar su proyecto
            var project = await _projects.GetByIdAsync(projectId);
            if (project?.OwnerId == userId) return;

            // Coordinator puede si es miembro
            if (user.Role == UserRole.Coordinator && await _projects.IsMemberAsync(projectId, userId))
                return;

            throw new ForbiddenException("No tienes permisos para gestionar este proyecto");
        }

        public async Task EnsureCanAssignActivitiesAsync(Guid userId, Guid projectId) {
            // Misma lógica que ManageProject para asignar
            await EnsureCanManageProjectAsync(userId, projectId);
        }
    }
}
