namespace Zezame.Api.Models.DTOs {
    public record CreateProjectDto(
        string Name,
        string? Description,
        string? Color,
        string? Emoji = null,
        DateTime? Deadline = null
    );

    public record UpdateProjectDto(
        string? Name,
        string? Description,
        string? Color,
        string? Emoji,
        DateTime? Deadline,
        string? Status
    );

    public record ProjectDto(
        Guid Id,
        string Name,
        string? Description,
        string Color,
        string? Emoji,
        string Status,
        DateTime? Deadline,
        double Hours,
        int Progress,
        int MemberCount,
        int TasksCompleted,
        int TasksTotal,
        Guid OwnerId,
        string OwnerName,
        DateTime CreatedAt
    );
}
