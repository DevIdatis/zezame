using Zezame.Domain.Entities;

namespace Zezame.Domain.Interfaces {
    public interface IUserTokenRepository : IRepository<UserToken> {
        /// <summary>Busca un token por propósito y hash, incluyendo el usuario.</summary>
        Task<UserToken?> GetByHashAsync(UserTokenPurpose purpose, string tokenHash);

        /// <summary>Invalida (marca como usados) los tokens pendientes de ese propósito del usuario.</summary>
        Task InvalidatePendingAsync(Guid userId, UserTokenPurpose purpose, DateTime when);
    }
}
