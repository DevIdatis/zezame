using Microsoft.AspNetCore.SignalR;
using System.IdentityModel.Tokens.Jwt;
using System.Text.RegularExpressions;

namespace Zezame.Api.Hubs {
    public class TimeHub : Hub {
        public override async Task OnConnectedAsync() {
            var userId = Context.User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (!string.IsNullOrEmpty(userId))
                await Groups.AddToGroupAsync(Context.ConnectionId, userId);

            await base.OnConnectedAsync();
        }
    }
}
