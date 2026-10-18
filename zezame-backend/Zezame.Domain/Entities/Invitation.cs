using Zezame.Domain.Base;

namespace Zezame.Domain.Entities {
    public enum InvitationStatus {
        Pending = 0,
        Accepted = 1,
        Revoked = 2
    }

    /// <summary>
    /// Invitación para que una persona se una a una organización con un rol. El token
    /// es de un solo uso, con caducidad, y se guarda solo su hash (SHA-256).
    /// </summary>
    public class Invitation : BaseEntity {
        public Guid OrganizationId { get; set; }
        public Organization Organization { get; set; } = null!;

        public string Email { get; set; } = null!; // en minúsculas
        public OrganizationRole Role { get; set; }  // Employee/Manager/Admin (nunca Owner)

        public string TokenHash { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
        public InvitationStatus Status { get; set; } = InvitationStatus.Pending;

        public Guid InvitedByUserId { get; set; }
        public DateTime? AcceptedAt { get; set; }
    }
}
