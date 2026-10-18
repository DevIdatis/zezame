using Zezame.Domain.Entities;

namespace Zezame.Domain.Interfaces {
    public interface IRefreshTokenRepository : IRepository<RefreshToken> {
        /// <summary>Busca un token por su hash, incluyendo el usuario asociado.</summary>
        Task<RefreshToken?> GetByTokenHashAsync(string tokenHash);

        /// <summary>Revoca de golpe todos los tokens activos de un usuario.</summary>
        Task RevokeAllActiveForUserAsync(Guid userId, DateTime when);
    }
}
