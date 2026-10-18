using Microsoft.EntityFrameworkCore;
using Zezame.Domain.Entities;
using Zezame.Domain.Interfaces;
using Zezame.Infrastructure.Persistence;

namespace Zezame.Infrastructure.Repositories {
    public class InvitationRepository(ZezameDbContext context)
        : Repository<Invitation>(context), IInvitationRepository {

        public Task<Invitation?> GetByTokenHashAsync(string tokenHash)
            => _dbSet.FirstOrDefaultAsync(i => i.TokenHash == tokenHash);

        public async Task<IEnumerable<Invitation>> GetByOrganizationAsync(Guid organizationId)
            => await _dbSet
                .Where(i => i.OrganizationId == organizationId)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();

        public Task RevokePendingForEmailAsync(Guid organizationId, string email, DateTime when)
            => _dbSet
                .Where(i => i.OrganizationId == organizationId
                            && i.Email == email
                            && i.Status == InvitationStatus.Pending)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(i => i.Status, InvitationStatus.Revoked)
                    .SetProperty(i => i.UpdatedAt, when));
    }
}
