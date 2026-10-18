using Zezame.Domain.Entities;

namespace Zezame.Domain.Interfaces {
    public interface IInvitationRepository : IRepository<Invitation> {
        /// <summary>Busca una invitación por el hash de su token (uso anónimo al aceptar).</summary>
        Task<Invitation?> GetByTokenHashAsync(string tokenHash);

        /// <summary>Invitaciones de una organización (más recientes primero).</summary>
        Task<IEnumerable<Invitation>> GetByOrganizationAsync(Guid organizationId);

        /// <summary>Revoca las invitaciones pendientes de ese email en la organización.</summary>
        Task RevokePendingForEmailAsync(Guid organizationId, string email, DateTime when);
    }
}
