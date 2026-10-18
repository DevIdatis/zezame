namespace Zezame.Domain.Interfaces {
    /// <summary>
    /// Organización (tenant) de la petición en curso. Lo rellena la resolución de
    /// tenant a partir del JWT y lo consume el Global Query Filter de EF Core para
    /// aislar los datos entre organizaciones.
    /// </summary>
    public interface ITenantContext {
        Guid? CurrentOrganizationId { get; }
        bool IsPlatformAdmin { get; }

        void SetTenant(Guid? organizationId, bool isPlatformAdmin);
    }
}
