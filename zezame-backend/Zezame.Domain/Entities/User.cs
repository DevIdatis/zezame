using Zezame.Domain.Base;

namespace Zezame.Domain.Entities {
    /// <summary>
    /// Rol del usuario dentro de su organización. Los valores enteros se conservan
    /// respecto al enum anterior (User=0, Coordinator=1, Admin=2) para no requerir
    /// migración de datos: 0→Employee, 1→Manager, 2→Admin. Owner (3) es nuevo.
    /// </summary>
    public enum OrganizationRole {
        Employee = 0,
        Manager = 1,
        Admin = 2,
        Owner = 3
    }
    public class User : BaseEntity {
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public OrganizationRole Role { get; set; } = OrganizationRole.Employee;

        // Administrador de la plataforma (nivel SaaS, por encima de las organizaciones).
        // No pertenece a ninguna organización: OrganizationId es null en ese caso.
        public bool IsPlatformAdmin { get; set; } = false;
        public string? Timezone { get; set; }
        public string? Language { get; set; }
        // Organización a la que pertenece (null solo para Platform Admin).
        public Guid? OrganizationId { get; set; }
        public Organization? Organization { get; set; }

        // Relaciones
        public ICollection<Project> OwnedProjects { get; set; } = [];
        public ICollection<ProjectMember> ProjectMemberships { get; set; } = [];
        public ICollection<Activity> AssignedActivities { get; set; } = [];
        public ICollection<TimeEntry> TimeEntries { get; set; } = [];
    }
}
