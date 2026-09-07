using Zezame.Domain.Base;

namespace Zezame.Domain.Entities {
    // Activity.cs (Tareas dentro de un proyecto)
    public class Activity : BaseEntity {
        public string Name { get; set; } = null!; // "Diseño de landing page"
        public string? Description { get; set; }
        public string? Emoji { get; set; }
        public ActivityStatus Status { get; set; } = ActivityStatus.Todo;
        public int? EstimatedMinutes { get; set; } // Estimación opcional

        // Relaciones
        public Guid ProjectId { get; set; }
        public Project Project { get; set; } = null!;

        public Guid? AssignedToId { get; set; } // Opcional: asignado a quién
        public User? AssignedTo { get; set; }

        public Guid? CreatedById { get; set; }
        public User? CreatedBy { get; set; }

        public ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
    }

    public enum ActivityStatus {
        Todo,
        InProgress,
        Done,
        Cancelled
    }
}
