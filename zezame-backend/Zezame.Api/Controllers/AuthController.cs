using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Zezame.Api.Models.DTOs;
using Zezame.Api.Services;
using static Zezame.Api.Models.AuthDtos;

namespace Zezame.Api.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    [EnableRateLimiting("auth")]
    public class AuthController : ControllerBase {
        private readonly AuthService _auth;

        public AuthController(AuthService auth) {
            _auth = auth;
        }

        [HttpPost("register")]
        public async Task<ActionResult<TokenDto>> Register(RegisterDto dto)
            => Ok(await _auth.RegisterAsync(dto));

        [HttpPost("login")]
        public async Task<ActionResult<TokenDto>> Login(LoginDto dto)
            => Ok(await _auth.LoginAsync(dto));

        [HttpPost("refresh")]
        public async Task<ActionResult<TokenDto>> Refresh(RefreshRequest dto)
            => Ok(await _auth.RefreshAsync(dto.RefreshToken));

        [HttpPost("logout")]
        public async Task<IActionResult> Logout(RefreshRequest dto) {
            await _auth.LogoutAsync(dto.RefreshToken);
            return NoContent();
        }

        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail(VerifyEmailRequest dto) {
            await _auth.VerifyEmailAsync(dto.Token);
            return NoContent();
        }

        [HttpPost("resend-verification")]
        public async Task<IActionResult> ResendVerification(ResendVerificationRequest dto) {
            await _auth.ResendVerificationAsync(dto.Email);
            return NoContent(); // respuesta neutra: no revela si el email existe
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest dto) {
            await _auth.ForgotPasswordAsync(dto.Email);
            return NoContent(); // respuesta neutra: no revela si el email existe
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest dto) {
            await _auth.ResetPasswordAsync(dto.Token, dto.NewPassword);
            return NoContent();
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto dto) {
            var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("No se encontró el UserId en los claims");
            await _auth.ChangePasswordAsync(Guid.Parse(claim.Value), dto);
            return NoContent();
        }
    }
}
