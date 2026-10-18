using Zezame.Api.Exceptions;
using Zezame.Api.Services.Interfaces;
using Zezame.Domain.Entities;
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

            if (user.Role is not (OrganizationRole.Manager or OrganizationRole.Admin or OrganizationRole.Owner))
                throw new ForbiddenException("Solo managers, admins y owners pueden crear proyectos");
        }

        public async Task EnsureCanAccessProjectAsync(Guid userId, Guid projectId) {
            var user = await _users.GetByIdAsync(userId);
            if (user == null) throw new UnauthorizedException();

            // Admin y Owner pueden leer cualquier proyecto de la organización.
            if (user.Role is OrganizationRole.Admin or OrganizationRole.Owner) return;

            var project = await _projects.GetByIdAsync(projectId);
            // Si no existe, o el usuario no es propietario ni miembro, respondemos 404:
            // no confirmamos la existencia de proyectos ajenos (evita IDOR/enumeración).
            if (project == null) throw new NotFoundException("Proyecto no encontrado");
            if (project.OwnerId == userId) return;
            if (await _projects.IsMemberAsync(projectId, userId)) return;

            throw new NotFoundException("Proyecto no encontrado");
        }

        public async Task EnsureCanManageProjectAsync(Guid userId, Guid projectId) {
            var user = await _users.GetByIdAsync(userId);
            if (user == null) throw new UnauthorizedException();

            // Admin y Owner de la organización pueden gestionar cualquier proyecto.
            if (user.Role is OrganizationRole.Admin or OrganizationRole.Owner) return;

            // El propietario puede gestionar su proyecto.
            var project = await _projects.GetByIdAsync(projectId);
            if (project?.OwnerId == userId) return;

            // Un Manager puede gestionar si es miembro del proyecto.
            if (user.Role == OrganizationRole.Manager && await _projects.IsMemberAsync(projectId, userId))
                return;

            throw new ForbiddenException("No tienes permisos para gestionar este proyecto");
        }

        public async Task EnsureCanAssignActivitiesAsync(Guid userId, Guid projectId) {
            // Misma lógica que ManageProject para asignar
            await EnsureCanManageProjectAsync(userId, projectId);
        }
    }
}
