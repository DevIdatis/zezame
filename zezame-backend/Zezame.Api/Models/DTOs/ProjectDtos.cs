namespace Zezame.Api.Models.DTOs {
    public record CreateProjectDto(string Name, string? Description, string? Color);
    public record ProjectDto(
        Guid Id,
        string Name,
        string Color,
        string? Description,
        Guid OwnerId,
        string OwnerName,
        int MemberCount,
        int ActivityCount,
        DateTime CreatedAt
    );
}
