using Zezame.Domain.Base;

namespace Zezame.Domain.Entities {
    public enum EntryType { Work, Break }
    public class TimeEntry : BaseEntity {
        public Guid UserId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int? DurationMin { get; set; }
        public string? Description { get; set; }
        public EntryType Type { get; set; } = EntryType.Work;
        public User User { get; set; } = null!;
        public Guid? ProjectId { get; set; } 
        public Project? Project { get; set; }
        public Guid? ActivityId { get; set; } 
        public Activity? Activity { get; set; }
    }
}
