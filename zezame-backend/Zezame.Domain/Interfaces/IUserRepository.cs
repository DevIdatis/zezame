using Zezame.Domain.Entities;
using static Zezame.Domain.Entities.User;

namespace Zezame.Domain.Interfaces {
    public interface IUserRepository : IRepository<User> {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByIdWithProjectsAsync(Guid id);
        Task<IEnumerable<User>> GetByRoleAsync(OrganizationRole role);
    }
}
