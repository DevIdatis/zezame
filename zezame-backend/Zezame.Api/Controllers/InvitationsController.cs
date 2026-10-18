using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Zezame.Api.Models.DTOs;
using Zezame.Api.Services;
using Zezame.Domain.Entities;

namespace Zezame.Api.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "RequireAdmin")] // Owner y Admin gestionan invitaciones
    public class InvitationsController : ControllerBase {
        private readonly InvitationService _invitations;

        public InvitationsController(InvitationService invitations) => _invitations = invitations;

        [HttpPost]
        public async Task<ActionResult<InvitationDto>> Create(CreateInvitationDto dto)
            => Ok(await _invitations.CreateAsync(GetCurrentUserId(), dto));

        [HttpGet]
        public async Task<ActionResult<IEnumerable<InvitationDto>>> List()
            => Ok(await _invitations.ListAsync(GetCurrentUserId()));

        [HttpPost("{id}/revoke")]
        public async Task<IActionResult> Revoke(Guid id) {
            await _invitations.RevokeAsync(GetCurrentUserId(), id);
            return NoContent();
        }

        [HttpPost("{id}/resend")]
        public async Task<IActionResult> Resend(Guid id) {
            await _invitations.ResendAsync(GetCurrentUserId(), id);
            return NoContent();
        }

        private Guid GetCurrentUserId() {
            var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("No se encontró el UserId en los claims");
            return Guid.Parse(claim.Value);
        }
    }
}
