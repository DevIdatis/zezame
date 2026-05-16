using static Zezame.Domain.Entities.User;

namespace Zezame.Api.Models {
    public class AuthDtos {
        public record RegisterDto(
            string Email,
            string Password,
            string FullName,
            UserRole? Role = null // Solo Admin puede asignar otros roles
        );

        public record TokenDto(string Token, DateTime Expires, UserDto User);
        public record UserDto(Guid Id, string Email, string FullName, string Role);
        public record LoginDto(string Email, string Password);
    }
}
