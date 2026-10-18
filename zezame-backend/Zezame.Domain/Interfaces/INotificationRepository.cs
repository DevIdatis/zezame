using Zezame.Domain.Entities;

namespace Zezame.Domain.Interfaces {
    public interface INotificationRepository : IRepository<Notification> {
        Task<IEnumerable<Notification>> GetByUserAsync(Guid userId, int take = 50);
        Task<int> GetUnreadCountAsync(Guid userId);
        Task<bool> MarkReadAsync(Guid id, Guid userId);
        Task<int> MarkAllReadAsync(Guid userId);
        Task<bool> DeleteForUserAsync(Guid id, Guid userId);
    }
}
