namespace Zezame.Api.Models {
    public class AuthDtos {
        public record RegisterDto(string Email, string Password, string FullName);
        public record LoginDto(string Email, string Password);
        public record TokenDto(string Token, DateTime Expires);
    }
}
