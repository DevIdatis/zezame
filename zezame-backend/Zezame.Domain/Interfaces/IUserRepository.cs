using Zezame.Domain.Entities;

namespace Zezame.Domain.Interfaces {
    public interface IUserRepository {
        Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
        Task AddAsync(User user, CancellationToken ct = default);
    }
}
