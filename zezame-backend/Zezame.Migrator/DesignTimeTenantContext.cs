
using Zezame.Domain.Interfaces;

public class DesignTimeTenantContext : ITenantContext
{

    public Guid? CurrentOrganizationId => null;

    public bool IsPlatformAdmin => throw new NotImplementedException();

    public void SetTenant(Guid? organizationId, bool isPlatformAdmin) {
        throw new NotImplementedException();
    }
}