using Microsoft.EntityFrameworkCore;
using Zezame.Domain.Entities;
using Zezame.Domain.Interfaces;
using Zezame.Infrastructure.Persistence;

namespace Zezame.Infrastructure.Repositories {
    public class NotificationRepository : Repository<Notification>, INotificationRepository {
        public NotificationRepository(ZezameDbContext context) : base(context) { }

        public async Task<IEnumerable<Notification>> GetByUserAsync(Guid userId, int take = 50)
            => await _dbSet
                .Where(n => n.UserId == userId && !n.IsDeleted)
                .OrderByDescending(n => n.CreatedAt)
                .Take(take)
                .ToListAsync();

        public async Task<int> GetUnreadCountAsync(Guid userId)
            => await _dbSet.CountAsync(n => n.UserId == userId && !n.IsRead && !n.IsDeleted);

        public async Task<bool> MarkReadAsync(Guid id, Guid userId) {
            var notification = await _dbSet
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId && !n.IsDeleted);
            if (notification == null) return false;

            notification.IsRead = true;
            notification.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> MarkAllReadAsync(Guid userId) {
            var unread = await _dbSet
                .Where(n => n.UserId == userId && !n.IsRead && !n.IsDeleted)
                .ToListAsync();
            foreach (var n in unread) {
                n.IsRead = true;
                n.UpdatedAt = DateTime.UtcNow;
            }
            await _context.SaveChangesAsync();
            return unread.Count;
        }

        public async Task<bool> DeleteForUserAsync(Guid id, Guid userId) {
            var notification = await _dbSet
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId && !n.IsDeleted);
            if (notification == null) return false;

            notification.IsDeleted = true;
            notification.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
