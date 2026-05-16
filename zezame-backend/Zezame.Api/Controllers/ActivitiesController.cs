using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Zezame.Api.Models.DTOs;
using Zezame.Api.Services;

namespace Zezame.Api.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ActivitiesController : ControllerBase {
        private readonly ActivityService _activityService;
        private readonly IHttpContextAccessor _httpContext;

        public ActivitiesController(ActivityService activityService, IHttpContextAccessor httpContext) {
            _activityService = activityService;
            _httpContext = httpContext;
        }

        [HttpPost]
        [Authorize(Policy = "RequireCoordinator")]
        public async Task<ActionResult<ActivityDto>> Create(CreateActivityDto dto) {
            var userId = GetCurrentUserId();
            var activity = await _activityService.CreateAsync(userId, dto);
            return Ok(activity);
        }

        [HttpGet("my")]
        public async Task<ActionResult<IEnumerable<ActivityDto>>> GetMyActivities() {
            var userId = GetCurrentUserId();
            var activities = await _activityService.GetMyActivitiesAsync(userId);
            return Ok(activities);
        }

        private Guid GetCurrentUserId() {
            var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (claim == null)
                throw new UnauthorizedAccessException("No se encontró el UserId en los claims");

            return Guid.Parse(claim.Value);
        }
    }
}
