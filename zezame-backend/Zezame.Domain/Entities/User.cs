using Zezame.Domain.Base;

namespace Zezame.Domain.Entities {
    public class User : BaseEntity {
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public UserRole Role { get; set; } = UserRole.User;
        public string? Timezone { get; set; }
        public string? Language { get; set; }

        // Relaciones
        public ICollection<Project> OwnedProjects { get; set; } = [];
        public ICollection<ProjectMember> ProjectMemberships { get; set; } = [];
        public ICollection<Activity> AssignedActivities { get; set; } = [];
        public ICollection<TimeEntry> TimeEntries { get; set; } = [];
        public enum UserRole {
            User = 0,        
            Coordinator = 1, 
            Admin = 2        
        }
    }
}
