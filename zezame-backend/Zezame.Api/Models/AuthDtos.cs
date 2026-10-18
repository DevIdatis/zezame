using static Zezame.Domain.Entities.User;

namespace Zezame.Api.Models {
    public class AuthDtos {
        // El registro público crea una organización nueva y convierte al registrante
        // en Owner. Los demás usuarios entran por invitación (fase posterior).
        public record RegisterDto(
            string Email,
            string Password,
            string FullName,
            string OrganizationName
        );

        public record TokenDto(string Token, DateTime Expires, string RefreshToken, UserDto User);
        public record UserDto(Guid Id, string Email, string FullName, string Role);
        public record LoginDto(string Email, string Password);
        public record RefreshRequest(string RefreshToken);
    }
}
