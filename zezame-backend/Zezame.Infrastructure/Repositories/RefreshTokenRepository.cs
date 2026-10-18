using Microsoft.EntityFrameworkCore;
using Zezame.Domain.Entities;
using Zezame.Domain.Interfaces;
using Zezame.Infrastructure.Persistence;

namespace Zezame.Infrastructure.Repositories {
    public class RefreshTokenRepository(ZezameDbContext context)
        : Repository<RefreshToken>(context), IRefreshTokenRepository {

        public Task<RefreshToken?> GetByTokenHashAsync(string tokenHash)
            => _dbSet.Include(rt => rt.User).ThenInclude(x => x.Organization)
                     .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash);

        public Task RevokeAllActiveForUserAsync(Guid userId, DateTime when)
            => _dbSet
                .Where(rt => rt.UserId == userId && rt.RevokedAt == null && rt.ExpiresAt > DateTime.UtcNow)
                .ExecuteUpdateAsync(s => s.SetProperty(rt => rt.RevokedAt, when));
    }
}
