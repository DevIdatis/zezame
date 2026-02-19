namespace Zezame.Domain.Entities {
    public enum EntryType { Work, Break, Lunch }
    public class TimeEntry {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public int? DurationMin { get; set; }

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public EntryType Type { get; set; } = EntryType.Work;

        public User User { get; set; } = null!;
    }
}
