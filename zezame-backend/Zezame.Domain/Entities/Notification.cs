using Zezame.Domain.Base;

namespace Zezame.Domain.Entities {
    public enum NotificationType {
        Info,
        Welcome,
        Activity,
        Project
    }

    public class Notification : BaseEntity {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Message { get; set; } = null!;
        public NotificationType Type { get; set; } = NotificationType.Info;
        public bool IsRead { get; set; } = false;
    }
}
