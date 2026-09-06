using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Zezame.Api.Models.DTOs;
using Zezame.Api.Services.Interfaces;
using Zezame.Domain.Entities;

namespace Zezame.Api.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProjectsController : ControllerBase {
        private readonly IProjectService _projectService;

        public ProjectsController(IProjectService projectService) {
            _projectService = projectService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProjectDto>>> List([FromQuery] string? status) {
            var userId = GetCurrentUserId();
            ProjectStatus? parsed = null;
            if (!string.IsNullOrWhiteSpace(status) &&
                !status.Equals("all", StringComparison.OrdinalIgnoreCase) &&
                Enum.TryParse<ProjectStatus>(status, true, out var st)) {
                parsed = st;
            }

            var projects = await _projectService.GetProjectsAsync(userId, parsed);
            return Ok(projects);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProjectDto>> GetById(Guid id) {
            var project = await _projectService.GetByIdAsync(id);
            return project == null ? NotFound() : Ok(project);
        }

        [HttpPost]
        [Authorize(Policy = "RequireCoordinator")]
        public async Task<ActionResult<ProjectDto>> Create(CreateProjectDto dto) {
            var userId = GetCurrentUserId();
            var project = await _projectService.CreateAsync(userId, dto);
            return Ok(project);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "RequireCoordinator")]
        public async Task<ActionResult<ProjectDto>> Update(Guid id, UpdateProjectDto dto) {
            var userId = GetCurrentUserId();
            var project = await _projectService.UpdateAsync(id, userId, dto);
            return Ok(project);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "RequireCoordinator")]
        public async Task<IActionResult> Delete(Guid id) {
            var userId = GetCurrentUserId();
            await _projectService.DeleteAsync(id, userId);
            return NoContent();
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
