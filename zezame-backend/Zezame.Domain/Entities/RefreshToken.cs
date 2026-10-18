using System.ComponentModel.DataAnnotations.Schema;
using Zezame.Domain.Base;

namespace Zezame.Domain.Entities {
    /// <summary>
    /// Token de refresco persistido y revocable. Se guarda solo el hash (SHA-256)
    /// del token, nunca el valor en claro, para que una fuga de BD no sea utilizable.
    /// </summary>
    public class RefreshToken : BaseEntity {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public string TokenHash { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
        public DateTime? RevokedAt { get; set; }

        [NotMapped] public bool IsRevoked => RevokedAt != null;
        [NotMapped] public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
        [NotMapped] public bool IsActive => !IsRevoked && !IsExpired;
    }
}
