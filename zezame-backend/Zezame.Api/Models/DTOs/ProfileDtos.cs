namespace Zezame.Api.Models.DTOs {
    public record ProfileDto(
        Guid Id,
        string Email,
        string FullName,
        string Role,
        string? Timezone,
        string? Language
    );

    public record UpdateProfileDto(
        string? FullName,
        string? Email,
        string? Timezone,
        string? Language
    );

    public record ChangePasswordDto(string CurrentPassword, string NewPassword);
}
