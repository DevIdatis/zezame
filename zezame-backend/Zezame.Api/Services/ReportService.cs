using Zezame.Api.Models.DTOs;
using Zezame.Domain.Entities;
using Zezame.Domain.Interfaces;

namespace Zezame.Api.Services {
    public class ReportService {
        private readonly ITimeEntryRepository _entries;

        public ReportService(ITimeEntryRepository entries) => _entries = entries;

        // Agregación por día de un mes (para el heatmap del calendario)
        public async Task<IEnumerable<CalendarDayDto>> GetCalendarAsync(Guid userId, int year, int month) {
            var start = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
            var end = start.AddMonths(1).AddSeconds(-1);

            var entries = await _entries.GetByUserAsync(userId, start, end);
            var work = entries.Where(e => e.Type == EntryType.Work);

            var byDay = work
                .GroupBy(e => e.StartTime.Day)
                .ToDictionary(
                    g => g.Key,
                    g => (Minutes: g.Sum(e => e.DurationMin ?? 0), Count: g.Count())
                );

            var daysInMonth = DateTime.DaysInMonth(year, month);
            var result = new List<CalendarDayDto>(daysInMonth);
            for (var d = 1; d <= daysInMonth; d++) {
                if (byDay.TryGetValue(d, out var v))
                    result.Add(new CalendarDayDto(d, Math.Round(v.Minutes / 60.0, 1), v.Count));
                else
                    result.Add(new CalendarDayDto(d, 0, 0));
            }

            return result;
        }

        // ===== Resumen del mes (4 stats con variación vs mes anterior) =====
        public async Task<SummaryDto> GetSummaryAsync(Guid userId, int year, int month) {
            var start = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
            var cur = await AggregateAsync(userId, start, start.AddMonths(1));
            var prev = await AggregateAsync(userId, start.AddMonths(-1), start);

            return new SummaryDto(
                Math.Round(cur.TotalHours, 1), Change(cur.TotalHours, prev.TotalHours),
                Math.Round(cur.AvgDaily, 1), Change(cur.AvgDaily, prev.AvgDaily),
                cur.Productivity, Change(cur.Productivity, prev.Productivity),
                Math.Round(cur.FocusHours, 1), Change(cur.FocusHours, prev.FocusHours)
            );
        }

        private async Task<(double TotalHours, double FocusHours, double AvgDaily, int Productivity)>
            AggregateAsync(Guid userId, DateTime start, DateTime endExclusive) {
            var entries = await _entries.GetByUserAsync(userId, start, endExclusive.AddSeconds(-1));
            var workMin = entries.Where(e => e.Type == EntryType.Work).Sum(e => e.DurationMin ?? 0);
            var breakMin = entries.Where(e => e.Type == EntryType.Break).Sum(e => e.DurationMin ?? 0);
            var daysWorked = entries.Where(e => e.Type == EntryType.Work)
                .Select(e => e.StartTime.Date).Distinct().Count();

            var focusHours = workMin / 60.0;
            var totalHours = (workMin + breakMin) / 60.0;
            var avgDaily = daysWorked > 0 ? focusHours / daysWorked : 0;
            var productivity = (workMin + breakMin) > 0
                ? (int)Math.Round((double)workMin / (workMin + breakMin) * 100)
                : 0;

            return (totalHours, focusHours, avgDaily, productivity);
        }

        private static double Change(double cur, double prev) {
            if (prev <= 0) return cur > 0 ? 100 : 0;
            return Math.Round((cur - prev) / prev * 100, 1);
        }

        // ===== Horas por proyecto (tarta) =====
        public async Task<IEnumerable<ProjectBreakdownDto>> GetByProjectAsync(Guid userId, int year, int month) {
            var start = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
            var entries = await _entries.GetByUserAsync(userId, start, start.AddMonths(1).AddSeconds(-1));
            var work = entries.Where(e => e.Type == EntryType.Work).ToList();
            var totalMin = work.Sum(e => e.DurationMin ?? 0);

            var groups = work
                .GroupBy(e => e.Project?.Name ?? "Otros")
                .Select(g => {
                    var min = g.Sum(e => e.DurationMin ?? 0);
                    return new ProjectBreakdownDto(
                        g.Key,
                        Math.Round(min / 60.0, 1),
                        totalMin > 0 ? (int)Math.Round((double)min / totalMin * 100) : 0);
                })
                .OrderByDescending(x => x.Hours)
                .ToList();

            return groups;
        }

        // ===== Horas por semana del mes (barras) =====
        public async Task<IEnumerable<WeeklyPointDto>> GetWeeklyAsync(Guid userId, int year, int month) {
            var start = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
            var entries = await _entries.GetByUserAsync(userId, start, start.AddMonths(1).AddSeconds(-1));
            var work = entries.Where(e => e.Type == EntryType.Work);

            var buckets = new Dictionary<int, int>();
            foreach (var e in work) {
                var week = (e.StartTime.Day - 1) / 7 + 1; // 1..5
                buckets[week] = buckets.GetValueOrDefault(week) + (e.DurationMin ?? 0);
            }

            var daysInMonth = DateTime.DaysInMonth(year, month);
            var weekCount = (daysInMonth - 1) / 7 + 1;
            var result = new List<WeeklyPointDto>();
            for (var w = 1; w <= weekCount; w++) {
                var min = buckets.GetValueOrDefault(w);
                result.Add(new WeeklyPointDto($"Sem {w}", Math.Round(min / 60.0, 1), 40));
            }
            return result;
        }

        // ===== Ranking del equipo (todos los usuarios) =====
        public async Task<IEnumerable<TeamMemberDto>> GetTeamAsync(int year, int month) {
            var start = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
            var entries = await _entries.GetAllInRangeAsync(start, start.AddMonths(1).AddSeconds(-1));

            return entries
                .Where(e => e.User != null)
                .GroupBy(e => new { e.UserId, e.User.FullName })
                .Select(g => {
                    var workMin = g.Where(e => e.Type == EntryType.Work).Sum(e => e.DurationMin ?? 0);
                    var breakMin = g.Where(e => e.Type == EntryType.Break).Sum(e => e.DurationMin ?? 0);
                    var productivity = (workMin + breakMin) > 0
                        ? (int)Math.Round((double)workMin / (workMin + breakMin) * 100)
                        : 0;
                    var firstName = g.Key.FullName.Split(' ')[0];
                    return new TeamMemberDto(g.Key.FullName, firstName, Math.Round(workMin / 60.0, 1), productivity);
                })
                .OrderByDescending(m => m.Hours)
                .ToList();
        }

        // Detalle de un día concreto
        public async Task<DaySummaryDto> GetDayAsync(Guid userId, DateTime date) {
            var start = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
            var end = start.AddDays(1).AddSeconds(-1);

            var entries = (await _entries.GetByUserAsync(userId, start, end))
                .OrderBy(e => e.StartTime)
                .ToList();

            var work = entries.Where(e => e.Type == EntryType.Work).ToList();
            var totalMinutes = work.Sum(e => e.DurationMin ?? 0);

            var dtos = entries
                .Select(e => new DayEntryDto(
                    e.Id,
                    e.Description,
                    e.Project?.Name,
                    e.Activity?.Name,
                    e.StartTime,
                    e.EndTime,
                    e.DurationMin,
                    e.Type.ToString()))
                .ToList();

            return new DaySummaryDto(start, totalMinutes, work.Count, dtos);
        }
    }
}
