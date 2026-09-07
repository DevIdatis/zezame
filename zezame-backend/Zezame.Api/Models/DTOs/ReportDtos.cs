namespace Zezame.Api.Models.DTOs {
    public record CalendarDayDto(int Day, double Hours, int Events);

    public record DayEntryDto(
        Guid Id,
        string? Description,
        string? ProjectName,
        string? ActivityName,
        DateTime StartTime,
        DateTime? EndTime,
        int? DurationMin,
        string Type
    );

    public record DaySummaryDto(
        DateTime Date,
        int TotalMinutes,
        int TaskCount,
        IReadOnlyList<DayEntryDto> Entries
    );

    // Resumen del mes (4 stats con variación vs mes anterior)
    public record SummaryDto(
        double TotalHours,
        double TotalChange,
        double AvgDailyHours,
        double AvgChange,
        int Productivity,
        double ProductivityChange,
        double FocusHours,
        double FocusChange
    );

    public record ProjectBreakdownDto(string Name, double Hours, int Percentage);

    public record WeeklyPointDto(string Week, double Hours, int Target);

    public record TeamMemberDto(string Name, string Avatar, double Hours, int Productivity);
}
