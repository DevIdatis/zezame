namespace Zezame.Api.Models.DTOs {
    public record CreateInvitationDto(string Email, string Role);
    public record AcceptInvitationDto(string Token, string FullName, string Password);
    public record InvitationDto(
        Guid Id,
        string Email,
        string Role,
        string Status,
        DateTime ExpiresAt,
        DateTime CreatedAt
    );
}
