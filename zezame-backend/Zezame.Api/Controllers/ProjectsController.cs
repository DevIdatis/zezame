using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Zezame.Api.Models.DTOs;
using Zezame.Api.Services.Interfaces;

namespace Zezame.Api.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProjectsController : ControllerBase {
        private readonly IProjectService _projectService;
        private readonly IHttpContextAccessor _httpContext;

        public ProjectsController(IProjectService projectService, IHttpContextAccessor httpContext) {
            _projectService = projectService;
            _httpContext = httpContext;
        }

        [HttpPost]
        [Authorize(Policy = "RequireCoordinator")]
        public async Task<ActionResult<ProjectDto>> Create(CreateProjectDto dto) {
            var userId = GetCurrentUserId();
            var project = await _projectService.CreateAsync(userId, dto);
            return Ok(project);
        }

        [HttpGet("my")]
        public async Task<ActionResult<IEnumerable<ProjectDto>>> GetMyProjects() {
            var userId = GetCurrentUserId();
            var projects = await _projectService.GetUserProjectsAsync(userId);
            return Ok(projects);
        }

        [HttpPost("{projectId}/members/{userId}")]
        [Authorize(Policy = "RequireCoordinator")]
        public async Task<IActionResult> AddMember(Guid projectId, Guid userId) {
            var currentUserId = GetCurrentUserId();
            await _projectService.AddMemberAsync(projectId, currentUserId, userId);
            return NoContent();
        }

        private Guid GetCurrentUserId() {
            var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (claim == null)
                throw new UnauthorizedAccessException("No se encontró el UserId en los claims");

            return Guid.Parse(claim.Value);
        }
    }

}
