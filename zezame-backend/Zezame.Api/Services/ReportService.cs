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
