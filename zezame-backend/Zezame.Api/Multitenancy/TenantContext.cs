using Zezame.Domain.Interfaces;

namespace Zezame.Api.Multitenancy {
    /// <summary>
    /// Contenedor scoped (una instancia por petición) con la organización actual.
    /// Lo rellena TenantResolutionMiddleware tras la autenticación.
    /// </summary>
    public class TenantContext : ITenantContext {
        public Guid? CurrentOrganizationId { get; private set; }
        public bool IsPlatformAdmin { get; private set; }

        public void SetTenant(Guid? organizationId, bool isPlatformAdmin) {
            CurrentOrganizationId = organizationId;
            IsPlatformAdmin = isPlatformAdmin;
        }
    }
}
