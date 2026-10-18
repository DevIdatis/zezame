using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Zezame.Api.Models.DTOs;
using Zezame.Domain.Entities;
using Zezame.Domain.Interfaces;
using static Zezame.Api.Models.AuthDtos;

namespace Zezame.Api.Services {
    public class AuthService(
        IUserRepository users,
        IRepository<Organization> orgs,
        IRefreshTokenRepository refreshTokens,
        IConfiguration cfg) {

        private readonly IUserRepository _users = users;
        private readonly IRepository<Organization> _orgs = orgs;
        private readonly IRefreshTokenRepository _refreshTokens = refreshTokens;
        private readonly IConfiguration _cfg = cfg;

        private int AccessMinutes => int.TryParse(_cfg["Jwt:AccessTokenMinutes"], out var m) ? m : 15;
        private int RefreshDays => int.TryParse(_cfg["Jwt:RefreshTokenDays"], out var d) ? d : 7;

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
            return await IssueTokensAsync(user);
        }

        public async Task<TokenDto> LoginAsync(LoginDto dto) {
            var email = dto.Email.ToLower();
            var user = await _users.GetByEmailAsync(email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Credenciales inválidas");

            await EnsureActiveAsync(user);
            return await IssueTokensAsync(user);
        }

        public async Task<TokenDto> RefreshAsync(string rawRefreshToken) {
            if (string.IsNullOrWhiteSpace(rawRefreshToken))
                throw new UnauthorizedAccessException("Token de refresco no válido");

            var stored = await _refreshTokens.GetByTokenHashAsync(HashToken(rawRefreshToken))
                ?? throw new UnauthorizedAccessException("Token de refresco no válido");

            // Reutilización de un token ya revocado: posible robo -> revocar todos.
            if (stored.IsRevoked) {
                await _refreshTokens.RevokeAllActiveForUserAsync(stored.UserId, DateTime.UtcNow);
                throw new UnauthorizedAccessException("Token de refresco no válido");
            }
            if (stored.IsExpired)
                throw new UnauthorizedAccessException("Token de refresco caducado");

            var user = stored.User;
            await EnsureActiveAsync(user);

            // Rotación: revocar el token usado y emitir uno nuevo.
            stored.RevokedAt = DateTime.UtcNow;
            await _refreshTokens.UpdateAsync(stored);

            return await IssueTokensAsync(user);
        }

        public async Task LogoutAsync(string rawRefreshToken) {
            if (string.IsNullOrWhiteSpace(rawRefreshToken)) return;

            var stored = await _refreshTokens.GetByTokenHashAsync(HashToken(rawRefreshToken));
            if (stored != null && !stored.IsRevoked) {
                stored.RevokedAt = DateTime.UtcNow;
                await _refreshTokens.UpdateAsync(stored);
            }
        }

        public async Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto) {
            var user = await _users.GetByIdAsync(userId)
                ?? throw new UnauthorizedAccessException("Usuario no encontrado");

            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
                throw new UnauthorizedAccessException("La contraseña actual no es correcta");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            await _users.UpdateAsync(user);

            // Cambiar la contraseña invalida el resto de sesiones del usuario.
            await _refreshTokens.RevokeAllActiveForUserAsync(userId, DateTime.UtcNow);
        }

        // Un usuario o una organización desactivados no pueden iniciar sesión ni renovar.
        private async Task EnsureActiveAsync(User user) {
            if (!user.IsActive)
                throw new UnauthorizedAccessException("La cuenta está desactivada");

            if (user.OrganizationId is Guid orgId) {
                var org = await _orgs.GetByIdAsync(orgId);
                if (org == null || !org.IsActive)
                    throw new UnauthorizedAccessException("La organización está desactivada");
            }
        }

        private async Task<TokenDto> IssueTokensAsync(User user) {
            var (accessToken, expires) = CreateAccessToken(user);

            var rawRefresh = GenerateRefreshTokenRaw();
            await _refreshTokens.AddAsync(new RefreshToken {
                UserId = user.Id,
                TokenHash = HashToken(rawRefresh),
                ExpiresAt = DateTime.UtcNow.AddDays(RefreshDays),
                CreatedAt = DateTime.UtcNow
            });

            var userDto = new UserDto(user.Id, user.Email, user.FullName, user.Role.ToString());
            return new TokenDto(accessToken, expires, rawRefresh, userDto);
        }

        private (string token, DateTime expires) CreateAccessToken(User user) {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_cfg["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Name, user.FullName),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("role", ((int)user.Role).ToString()),
                new Claim("org_id", user.OrganizationId?.ToString() ?? string.Empty),
                new Claim("is_platform_admin", user.IsPlatformAdmin ? "true" : "false")
            };

            var expires = DateTime.UtcNow.AddMinutes(AccessMinutes);
            var token = new JwtSecurityToken(
                issuer: _cfg["Jwt:Issuer"],
                audience: _cfg["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds);

            return (new JwtSecurityTokenHandler().WriteToken(token), expires);
        }

        private static string GenerateRefreshTokenRaw()
            => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

        private static string HashToken(string raw)
            => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw)));
    }
}
