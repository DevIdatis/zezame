using System.ComponentModel.DataAnnotations.Schema;
using Zezame.Domain.Base;

namespace Zezame.Domain.Entities {
    public enum UserTokenPurpose {
        EmailVerification = 0,
        PasswordReset = 1
    }

    /// <summary>
    /// Token de un solo uso y con caducidad para verificación de email y reset de
    /// contraseña. Se guarda solo el hash (SHA-256), nunca el valor en claro.
    /// </summary>
    public class UserToken : BaseEntity {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public UserTokenPurpose Purpose { get; set; }
        public string TokenHash { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
        public DateTime? UsedAt { get; set; }

        [NotMapped] public bool IsUsed => UsedAt != null;
        [NotMapped] public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
        [NotMapped] public bool IsValid => !IsUsed && !IsExpired;
    }
}
