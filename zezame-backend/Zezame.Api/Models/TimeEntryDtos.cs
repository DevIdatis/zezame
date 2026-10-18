using Zezame.Domain.Entities;

namespace Zezame.Api.Models {
    public class TimeEntryDtos {
        public record StartEntryDto(
            string? Description,
            Guid? ProjectId = null,
            Guid? ActivityId = null,
            EntryType Type = EntryType.Work
        );
        public record StopEntryDto(Guid Id);
        public record CreateManualEntryDto(
            DateTime StartTime,
            DateTime EndTime,
            string? Description,
            Guid? ProjectId = null,
            Guid? ActivityId = null
        );
        public record EntryVm(
            Guid Id,
            DateTime StartTime,
            DateTime? EndTime,
            int? DurationMin,
            string? Description,
            EntryType Type
        );
    }
}
