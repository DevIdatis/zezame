using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Zezame.Api.Models.DTOs;
using Zezame.Api.Services;

namespace Zezame.Api.Controllers {
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase {
        private readonly NotificationService _notifications;

        public NotificationsController(NotificationService notifications) => _notifications = notifications;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<NotificationDto>>> List()
            => Ok(await _notifications.GetMineAsync(UserGuid()));

        [HttpGet("unread-count")]
        public async Task<ActionResult<int>> UnreadCount()
            => Ok(await _notifications.UnreadCountAsync(UserGuid()));

        [HttpPatch("{id}/read")]
        public async Task<IActionResult> MarkRead(Guid id) {
            var ok = await _notifications.MarkReadAsync(id, UserGuid());
            return ok ? NoContent() : NotFound();
        }

        [HttpPost("read-all")]
        public async Task<ActionResult<int>> MarkAllRead()
            => Ok(await _notifications.MarkAllReadAsync(UserGuid()));

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id) {
            var ok = await _notifications.DeleteAsync(id, UserGuid());
            return ok ? NoContent() : NotFound();
        }

        private Guid UserGuid() {
            var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (claim == null)
                throw new UnauthorizedAccessException("No se encontró el UserId en los claims");
            return Guid.Parse(claim.Value);
        }
    }
}
