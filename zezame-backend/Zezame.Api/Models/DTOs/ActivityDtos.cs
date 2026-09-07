namespace Zezame.Api.Models.DTOs {
    public record CreateActivityDto(
        string Name,
        string? Description,
        Guid ProjectId,
        string? Emoji = null,
        Guid? AssignedToId = null,
        int? EstimatedMinutes = null
    );
    public record ActivityDto(
        Guid Id,
        string Name,
        string? Description,
        string? Emoji,
        string Status,
        Guid ProjectId,
        string ProjectName,
        string? AssignedToName,
        int? EstimatedMinutes,
        int? ActualMinutes,
        DateTime CreatedAt
    );
}
