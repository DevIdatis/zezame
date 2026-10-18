using Zezame.Api.Models.DTOs;
using Zezame.Domain.Entities;
using Zezame.Domain.Interfaces;

namespace Zezame.Api.Services {
    public class NotificationService {
        private readonly INotificationRepository _repo;

        public NotificationService(INotificationRepository repo) => _repo = repo;

        public async Task<IEnumerable<NotificationDto>> GetMineAsync(Guid userId)
            => (await _repo.GetByUserAsync(userId)).Select(MapToDto);

        public Task<int> UnreadCountAsync(Guid userId) => _repo.GetUnreadCountAsync(userId);

        public Task<bool> MarkReadAsync(Guid id, Guid userId) => _repo.MarkReadAsync(id, userId);

        public Task<int> MarkAllReadAsync(Guid userId) => _repo.MarkAllReadAsync(userId);

        public Task<bool> DeleteAsync(Guid id, Guid userId) => _repo.DeleteForUserAsync(id, userId);

        // Helper usado por otros servicios para generar notificaciones.
        // No debe romper la operación principal si algo falla al notificar.
        public async Task NotifyAsync(Guid userId, string title, string message, NotificationType type) {
            try {
                await _repo.AddAsync(new Notification {
                    UserId = userId,
                    Title = title,
                    Message = message,
                    Type = type
                });
            } catch {
                // Notificar es best-effort; nunca debe tumbar el flujo que la origina.
            }
        }

        private static NotificationDto MapToDto(Notification n)
            => new(n.Id, n.Title, n.Message, n.Type.ToString(), n.IsRead, n.CreatedAt);
    }
}
