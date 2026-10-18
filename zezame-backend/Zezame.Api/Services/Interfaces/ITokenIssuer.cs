using Zezame.Domain.Entities;
using static Zezame.Api.Models.AuthDtos;

namespace Zezame.Api.Services.Interfaces {
    /// <summary>Emite el par de tokens (access + refresh) para un usuario ya validado.</summary>
    public interface ITokenIssuer {
        Task<TokenDto> IssueTokensAsync(User user);
    }
}
