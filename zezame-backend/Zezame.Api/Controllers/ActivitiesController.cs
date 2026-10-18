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

        public ActivitiesController(ActivityService activityService) {
            _activityService = activityService;
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

        [HttpGet("by-project/{projectId}")]
        public async Task<ActionResult<IEnumerable<ActivityDto>>> GetByProject(Guid projectId) {
            var activities = await _activityService.GetByProjectAsync(projectId, GetCurrentUserId());
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
