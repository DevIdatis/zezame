using Zezame.Domain.Base;

namespace Zezame.Domain.Entities {
    public enum ProjectStatus {
        Active,
        Paused,
        Completed
    }

    public class Project : BaseEntity {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string Color { get; set; } = "from-coral to-yellow"; // Tailwind class
        public string? Emoji { get; set; }
        public ProjectStatus Status { get; set; } = ProjectStatus.Active;
        public DateTime? Deadline { get; set; }
        public int? ClientId { get; set; }
        // Organización propietaria del proyecto (tenant).
        public Guid OrganizationId { get; set; }
        public Organization Organization { get; set; } = null!;

        // Relaciones
        public Guid OwnerId { get; set; }
        public User Owner { get; set; } = null!;

        public ICollection<Activity> Activities { get; set; } = new List<Activity>();
        public ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
        public ICollection<ProjectMember> Members { get; set; } = new List<ProjectMember>();
    }
}
