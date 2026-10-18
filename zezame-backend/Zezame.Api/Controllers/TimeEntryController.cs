using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Zezame.Api.Services;
using static Zezame.Api.Models.TimeEntryDtos;

namespace Zezame.Api.Controllers {
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TimeEntriesController : ControllerBase {
        private readonly TimeService _svc;
        public TimeEntriesController(TimeService svc) => _svc = svc;

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<EntryVm>>> List([FromQuery] DateTime from, [FromQuery] DateTime to) {
            var fromUtc = DateTime.SpecifyKind(from, DateTimeKind.Utc);
            var toUtc = DateTime.SpecifyKind(to, DateTimeKind.Utc);

            return Ok(await _svc.ListAsync(UserGuid(), fromUtc, toUtc));
        }

        [HttpPost("createManual")]
        public async Task<ActionResult<EntryVm>> CreateManual(CreateManualEntryDto dto)
            => Ok(await _svc.CreateManualAsync(UserGuid(), dto));

        [HttpPost("start")]
        public async Task<ActionResult<EntryVm>> Start(StartEntryDto dto)
            => Ok(await _svc.StartAsync(UserGuid(), dto));

        [HttpPatch("stop")]
        public async Task<ActionResult<EntryVm>> Stop(StopEntryDto dto)
            => Ok(await _svc.StopAsync(UserGuid(), dto));

        [HttpPost("pause")]
        public async Task<ActionResult<EntryVm>> Pause()
            => Ok(await _svc.PauseAsync(UserGuid()));

        [HttpPost("resume")]
        public async Task<ActionResult<EntryVm>> Resume()
            => Ok(await _svc.ResumeAsync(UserGuid()));

        private Guid UserGuid() {
            var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (claim == null)
                throw new UnauthorizedAccessException("No se encontró el UserId en los claims");

            return Guid.Parse(claim.Value);
        }
    }
}
