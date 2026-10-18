using Zezame.Domain.Base;

namespace Zezame.Domain.Entities {
    /// <summary>
    /// Tenant raíz del SaaS. Cada usuario y cada recurso de negocio pertenece a una
    /// organización. El aislamiento efectivo entre organizaciones se añade en la Fase 3.
    /// </summary>
    public class Organization : BaseEntity {
        public string Name { get; set; } = null!;

        // Relaciones
        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<Project> Projects { get; set; } = new List<Project>();
    }
}
