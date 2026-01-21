namespace Zezame.Domain.Entities {
    public class User {
        public Guid Id { get; set; }

        public string Email { get; set; } = null!;

        public string PasswordHash { get; set; } = null!;

        public string FullName { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        public ICollection<TimeEntry> TimeEntries { get; set; } = [];
    }
}
