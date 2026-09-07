using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Zezame.Api.Models.DTOs;
using Zezame.Api.Services;

namespace Zezame.Api.Controllers {
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase {
        private readonly ReportService _reports;

        public ReportsController(ReportService reports) => _reports = reports;

        // GET /api/reports/calendar?month=YYYY-MM
        [HttpGet("calendar")]
        public async Task<ActionResult<IEnumerable<CalendarDayDto>>> Calendar([FromQuery] string month) {
            if (string.IsNullOrWhiteSpace(month) ||
                !DateTime.TryParseExact(month + "-01", "yyyy-MM-dd", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var parsed)) {
                throw new InvalidOperationException("Formato de mes inválido. Usa YYYY-MM.");
            }

            var data = await _reports.GetCalendarAsync(UserGuid(), parsed.Year, parsed.Month);
            return Ok(data);
        }

        // GET /api/reports/summary?month=YYYY-MM
        [HttpGet("summary")]
        public async Task<ActionResult<SummaryDto>> Summary([FromQuery] string month) {
            var (y, m) = ParseMonth(month);
            return Ok(await _reports.GetSummaryAsync(UserGuid(), y, m));
        }

        // GET /api/reports/by-project?month=YYYY-MM
        [HttpGet("by-project")]
        public async Task<ActionResult<IEnumerable<ProjectBreakdownDto>>> ByProject([FromQuery] string month) {
            var (y, m) = ParseMonth(month);
            return Ok(await _reports.GetByProjectAsync(UserGuid(), y, m));
        }

        // GET /api/reports/weekly?month=YYYY-MM
        [HttpGet("weekly")]
        public async Task<ActionResult<IEnumerable<WeeklyPointDto>>> Weekly([FromQuery] string month) {
            var (y, m) = ParseMonth(month);
            return Ok(await _reports.GetWeeklyAsync(UserGuid(), y, m));
        }

        // GET /api/reports/team?month=YYYY-MM (solo coordinadores/admins)
        [HttpGet("team")]
        [Authorize(Policy = "RequireCoordinator")]
        public async Task<ActionResult<IEnumerable<TeamMemberDto>>> Team([FromQuery] string month) {
            var (y, m) = ParseMonth(month);
            return Ok(await _reports.GetTeamAsync(y, m));
        }

        // GET /api/reports/day?date=YYYY-MM-DD
        [HttpGet("day")]
        public async Task<ActionResult<DaySummaryDto>> Day([FromQuery] string date) {
            if (string.IsNullOrWhiteSpace(date) ||
                !DateTime.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var parsed)) {
                throw new InvalidOperationException("Formato de fecha inválido. Usa YYYY-MM-DD.");
            }

            var data = await _reports.GetDayAsync(UserGuid(), parsed);
            return Ok(data);
        }

        private static (int Year, int Month) ParseMonth(string month) {
            if (string.IsNullOrWhiteSpace(month) ||
                !DateTime.TryParseExact(month + "-01", "yyyy-MM-dd", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var parsed)) {
                throw new InvalidOperationException("Formato de mes inválido. Usa YYYY-MM.");
            }
            return (parsed.Year, parsed.Month);
        }

        private Guid UserGuid() {
            var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (claim == null)
                throw new UnauthorizedAccessException("No se encontró el UserId en los claims");

            return Guid.Parse(claim.Value);
        }
    }
}
