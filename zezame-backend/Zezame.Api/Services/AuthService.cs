using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Zezame.Api.Models.DTOs;
using Zezame.Domain.Entities;
using Zezame.Domain.Interfaces;
using static Zezame.Api.Models.AuthDtos;
using static Zezame.Domain.Entities.User;

namespace Zezame.Api.Services {
    public class AuthService(IUserRepository users, IRepository<Organization> orgs, IConfiguration cfg) {
        private readonly IUserRepository _users = users;
        private readonly IRepository<Organization> _orgs = orgs;
        private readonly IConfiguration _cfg = cfg;

        public async Task<TokenDto> RegisterAsync(RegisterDto dto) {
            var email = dto.Email.ToLower();

            if (await _users.GetByEmailAsync(email) != null)
                throw new InvalidOperationException("Email ya registrado");

            // El registro público crea una organización nueva y convierte al
            // registrante en su Owner. El resto de usuarios entrará por invitación.
            var organization = new Organization {
                Name = dto.OrganizationName,
                CreatedAt = DateTime.UtcNow
            };
            await _orgs.AddAsync(organization);

            var user = new User {
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                FullName = dto.FullName,
                Role = OrganizationRole.Owner,
                OrganizationId = organization.Id,
                CreatedAt = DateTime.UtcNow,
            };

            await _users.AddAsync(user);
            return GenerateToken(user);
        }

        public async Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto) {
            var user = await _users.GetByIdAsync(userId)
                ?? throw new UnauthorizedAccessException("Usuario no encontrado");

            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
                throw new UnauthorizedAccessException("La contraseña actual no es correcta");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            await _users.UpdateAsync(user);
        }

        public async Task<TokenDto> LoginAsync(LoginDto dto) {
            var email = dto.Email.ToLower();
            var user = await _users.GetByEmailAsync(email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Credenciales inválidas");

            return GenerateToken(user);
        }

        private TokenDto GenerateToken(User user) {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_cfg["Jwt:Key"]!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Name, user.FullName),
                new Claim(ClaimTypes.Role, user.Role.ToString()), // ROL EN JWT
                new Claim("role", ((int)user.Role).ToString()), // Para fácil parsing en frontend
                // Claim de organización (tenant): lo consumirá la resolución de tenant en Fase 3.
                new Claim("org_id", user.OrganizationId?.ToString() ?? string.Empty),
                new Claim("is_platform_admin", user.IsPlatformAdmin ? "true" : "false")
            };

            var expires = DateTime.UtcNow.AddMinutes(
                int.Parse(_cfg["Jwt:ExpireMinutes"]!));

            var token = new JwtSecurityToken(
                issuer: _cfg["Jwt:Issuer"],
                audience: _cfg["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds);

            var userDto = new UserDto(user.Id, user.Email, user.FullName, user.Role.ToString());

            return new TokenDto(
                new JwtSecurityTokenHandler().WriteToken(token),
                expires,
                userDto
            );
        }
    }
}
