using Microsoft.EntityFrameworkCore.Metadata;
using Zezame.Api.Email;
using Zezame.Api.Exceptions;
using Zezame.Api.Models.DTOs;
using Zezame.Api.Security;
using Zezame.Api.Services.Interfaces;
using Zezame.Domain.Entities;
using Zezame.Domain.Interfaces;
using static Zezame.Api.Models.AuthDtos;

namespace Zezame.Api.Services {
    public class InvitationService(
        IUserRepository users,
        IInvitationRepository invitations,
        IEmailSender email,
        ITokenIssuer tokenIssuer,
        IConfiguration cfg) {

        private readonly IUserRepository _users = users;
        private readonly IInvitationRepository _invitations = invitations;
        private readonly IEmailSender _email = email;
        private readonly ITokenIssuer _tokenIssuer = tokenIssuer;
        private readonly IConfiguration _cfg = cfg;

        private static readonly TimeSpan InvitationTtl = TimeSpan.FromDays(7);
        private string FrontendUrl => (_cfg["FrontendUrl"] ?? "http://localhost:4200").TrimEnd('/');

        public async Task<InvitationDto> CreateAsync(Guid inviterUserId, CreateInvitationDto dto) {
            var inviter = await _users.GetByIdAsync(inviterUserId)
                ?? throw new UnauthorizedException();
            if (inviter.OrganizationId is not Guid orgId)
                throw new InvalidOperationException("El usuario no pertenece a ninguna organización");

            if (!Enum.TryParse<OrganizationRole>(dto.Role, ignoreCase: true, out var role)
                || role == OrganizationRole.Owner)
                throw new InvalidOperationException("Rol de invitación no válido (usa Employee, Manager o Admin)");

            var email = dto.Email.ToLower();
            if (await _users.GetByEmailAsync(email) != null)
                throw new InvalidOperationException("Ya existe un usuario con ese email");

            // Una sola invitación pendiente por email y organización.
            await _invitations.RevokePendingForEmailAsync(orgId, email, DateTime.UtcNow);

            var raw = TokenUtils.GenerateUrlToken();
            var invitation = new Invitation {
                OrganizationId = orgId,
                Email = email,
                Role = role,
                TokenHash = TokenUtils.Hash(raw),
                ExpiresAt = DateTime.UtcNow.Add(InvitationTtl),
                Status = InvitationStatus.Pending,
                InvitedByUserId = inviterUserId,
                CreatedAt = DateTime.UtcNow
            };
            await _invitations.AddAsync(invitation);

            await SendInvitationEmailAsync(email, role, raw);
            return ToDto(invitation);
        }

        public async Task<IEnumerable<InvitationDto>> ListAsync(Guid actingUserId) {
            var user = await _users.GetByIdAsync(actingUserId);
            if (user?.OrganizationId is not Guid orgId)
                return Enumerable.Empty<InvitationDto>();

            var items = await _invitations.GetByOrganizationAsync(orgId);
            return items.Select(ToDto);
        }

        public async Task RevokeAsync(Guid actingUserId, Guid invitationId) {
            var invitation = await LoadForOrgAsync(actingUserId, invitationId);
            if (invitation.Status == InvitationStatus.Pending) {
                invitation.Status = InvitationStatus.Revoked;
                await _invitations.UpdateAsync(invitation);
            }
        }

        public async Task ResendAsync(Guid actingUserId, Guid invitationId) {
            var invitation = await LoadForOrgAsync(actingUserId, invitationId);
            if (invitation.Status != InvitationStatus.Pending)
                throw new InvalidOperationException("La invitación no está pendiente");

            var raw = TokenUtils.GenerateUrlToken();
            invitation.TokenHash = TokenUtils.Hash(raw);
            invitation.ExpiresAt = DateTime.UtcNow.Add(InvitationTtl);
            await _invitations.UpdateAsync(invitation);

            await SendInvitationEmailAsync(invitation.Email, invitation.Role, raw);
        }

        public async Task<TokenDto> AcceptAsync(AcceptInvitationDto dto) {
            var invitation = await _invitations.GetByTokenHashAsync(TokenUtils.Hash(dto.Token));
            if (invitation is null || invitation.Status != InvitationStatus.Pending
                || DateTime.UtcNow >= invitation.ExpiresAt)
                throw new InvalidOperationException("Invitación no válida o caducada");

            if (await _users.GetByEmailAsync(invitation.Email) != null)
                throw new InvalidOperationException("Ya existe un usuario con ese email");

            var user = new User {
                Email = invitation.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                FullName = dto.FullName,
                Role = invitation.Role,
                OrganizationId = invitation.OrganizationId,
                EmailConfirmed = true, // llegó a través del email de invitación
                CreatedAt = DateTime.UtcNow
            };
            await _users.AddAsync(user);

            invitation.Status = InvitationStatus.Accepted;
            invitation.AcceptedAt = DateTime.UtcNow;
            await _invitations.UpdateAsync(invitation);

            return await _tokenIssuer.IssueTokensAsync(user);
        }

        private async Task<Invitation> LoadForOrgAsync(Guid actingUserId, Guid invitationId) {
            var user = await _users.GetByIdAsync(actingUserId);
            var invitation = await _invitations.GetByIdAsync(invitationId);
            // No se confirma la existencia de invitaciones de otra organización.
            if (invitation is null || user?.OrganizationId != invitation.OrganizationId)
                throw new NotFoundException("Invitación no encontrada");
            return invitation;
        }

        private Task SendInvitationEmailAsync(string email, OrganizationRole role, string rawToken)
            => _email.SendAsync(email, "Te han invitado a Zezame",
                $"Te han invitado a unirte a una organización en Zezame como {role}.\n" +
                $"Acepta la invitación aquí (caduca en 7 días):\n" +
                $"{FrontendUrl}/accept-invitation?token={rawToken}");

        private static InvitationDto ToDto(Invitation i) =>
            new(i.Id, i.Email, i.Role.ToString(), i.Status.ToString(), i.ExpiresAt, i.CreatedAt);
    }
}
