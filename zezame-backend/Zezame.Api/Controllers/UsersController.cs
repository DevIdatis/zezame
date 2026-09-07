using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Zezame.Api.Exceptions;
using Zezame.Api.Models.DTOs;
using Zezame.Domain.Interfaces;

namespace Zezame.Api.Controllers {
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase {
        private readonly IUserRepository _users;

        public UsersController(IUserRepository users) => _users = users;

        [HttpGet("me")]
        public async Task<ActionResult<ProfileDto>> GetMe() {
            var user = await _users.GetByIdAsync(UserGuid())
                ?? throw new NotFoundException("Usuario no encontrado");
            return Ok(ToDto(user));
        }

        [HttpPut("me")]
        public async Task<ActionResult<ProfileDto>> UpdateMe(UpdateProfileDto dto) {
            var user = await _users.GetByIdAsync(UserGuid())
                ?? throw new NotFoundException("Usuario no encontrado");

            if (!string.IsNullOrWhiteSpace(dto.Email) &&
                !dto.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase)) {
                var existing = await _users.GetByEmailAsync(dto.Email);
                if (existing != null && existing.Id != user.Id)
                    throw new InvalidOperationException("El email ya está en uso");
                user.Email = dto.Email.ToLower();
            }

            if (!string.IsNullOrWhiteSpace(dto.FullName)) user.FullName = dto.FullName;
            if (dto.Timezone != null) user.Timezone = dto.Timezone;
            if (dto.Language != null) user.Language = dto.Language;

            await _users.UpdateAsync(user);
            return Ok(ToDto(user));
        }

        private static ProfileDto ToDto(Domain.Entities.User u) =>
            new(u.Id, u.Email, u.FullName, u.Role.ToString(), u.Timezone, u.Language);

        private Guid UserGuid() {
            var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (claim == null)
                throw new UnauthorizedAccessException("No se encontró el UserId en los claims");
            return Guid.Parse(claim.Value);
        }
    }
}
