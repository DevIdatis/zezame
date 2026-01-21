using Microsoft.EntityFrameworkCore;
using Zezame.Domain.Entities;
using Zezame.Domain.Interfaces;
using Zezame.Infrastructure.Persistence;

namespace Zezame.Infrastructure.Repositories {
    public class UserRepository : IUserRepository {
        private readonly ZezameDbContext _db;

        public UserRepository(ZezameDbContext db) {
            _db = db;
        }

        public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) {
            return _db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
        }

        public async Task AddAsync(User user, CancellationToken ct = default) {
            _db.Users.Add(user);
            await _db.SaveChangesAsync(ct);
        }
    }
}
