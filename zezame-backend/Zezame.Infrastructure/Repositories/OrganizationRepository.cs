using Zezame.Domain.Entities;
using Zezame.Domain.Interfaces;
using Zezame.Infrastructure.Persistence;

namespace Zezame.Infrastructure.Repositories {
    public class OrganizationRepository(ZezameDbContext db) : Repository<Organization>(db), IOrganizationRepository {
        
    }
}
