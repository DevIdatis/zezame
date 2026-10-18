using Microsoft.EntityFrameworkCore;
using Zezame.Domain.Entities;
using Zezame.Domain.Interfaces;
using Zezame.Infrastructure.Persistence;

namespace Zezame.Infrastructure.Repositories {
    public class UserTokenRepository(ZezameDbContext context)
        : Repository<UserToken>(context), IUserTokenRepository {

        public Task<UserToken?> GetByHashAsync(UserTokenPurpose purpose, string tokenHash)
            => _dbSet.Include(t => t.User)
                     .FirstOrDefaultAsync(t => t.Purpose == purpose && t.TokenHash == tokenHash);

        public Task InvalidatePendingAsync(Guid userId, UserTokenPurpose purpose, DateTime when)
            => _dbSet
                .Where(t => t.UserId == userId && t.Purpose == purpose && t.UsedAt == null)
                .ExecuteUpdateAsync(s => s.SetProperty(t => t.UsedAt, when));
    }
}
