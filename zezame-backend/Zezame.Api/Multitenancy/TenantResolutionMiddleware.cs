using Zezame.Domain.Interfaces;

namespace Zezame.Api.Multitenancy {
    /// <summary>
    /// Resuelve la organización de la petición a partir de los claims del JWT y la
    /// deja en ITenantContext. Debe ejecutarse DESPUÉS de la autenticación.
    /// </summary>
    public class TenantResolutionMiddleware(RequestDelegate next) {
        private readonly RequestDelegate _next = next;

        public async Task InvokeAsync(HttpContext context, ITenantContext tenant) {
            var user = context.User;
            if (user?.Identity?.IsAuthenticated == true) {
                var orgId = Guid.TryParse(user.FindFirst("org_id")?.Value, out var g)
                    ? g
                    : (Guid?)null;
                var isPlatformAdmin = user.FindFirst("is_platform_admin")?.Value == "true";
                tenant.SetTenant(orgId, isPlatformAdmin);
            }

            await _next(context);
        }
    }
}
