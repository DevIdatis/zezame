namespace Zezame.Api.Models {
    public class TimeEntryDtos {
        public record StartEntryDto(string? Description);
        public record StopEntryDto(Guid Id);
        public record EntryVm(
            Guid Id,
            DateTime StartTime,
            DateTime? EndTime,
            int? DurationMin,
            string? Description
        );
    }
}
