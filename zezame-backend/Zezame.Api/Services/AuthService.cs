using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Zezame.Domain.Entities;
using Zezame.Domain.Interfaces;
using static Zezame.Api.Models.AuthDtos;
using static Zezame.Domain.Entities.User;

namespace Zezame.Api.Services {
    public class AuthService(IUserRepository users, IConfiguration cfg) {
        private readonly IUserRepository _users = users;
        private readonly IConfiguration _cfg = cfg;

        public async Task<TokenDto> RegisterAsync(RegisterDto dto) {
            var email = dto.Email.ToLower();

            if (await _users.GetByEmailAsync(email) != null)
                throw new InvalidOperationException("Email ya registrado");

            // Solo Admin puede crear Coordinadores o Admins
            //var assignedRole = UserRole.User;
            //if (dto.Role.HasValue && creatorRole == UserRole.Admin) {
            var assignedRole = dto.Role!.Value;
            //}

            var user = new User {
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                FullName = dto.FullName,
                Role = assignedRole,
                CreatedAt = DateTime.UtcNow,
            };

            await _users.AddAsync(user);
            return GenerateToken(user);
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
                new Claim("role", ((int)user.Role).ToString()) // Para fácil parsing en frontend
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
