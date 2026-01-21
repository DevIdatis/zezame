using Microsoft.AspNetCore.Mvc;
using Zezame.Api.Services;
using static Zezame.Api.Models.AuthDtos;

namespace Zezame.Api.Controllers {
    [ApiController]
    [Route("api/[controller]")]
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
    }
}
