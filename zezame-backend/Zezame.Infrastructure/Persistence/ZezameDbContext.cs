using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Zezame.Domain.Base;
using Zezame.Domain.Entities;
using Zezame.Domain.Interfaces;

namespace Zezame.Infrastructure.Persistence {
    public class ZezameDbContext : DbContext {
        private readonly ITenantContext _tenant;
        public DbSet<Organization> Organizations => Set<Organization>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Project> Projects => Set<Project>();
        public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();
        public DbSet<Activity> Activities => Set<Activity>();
        public DbSet<TimeEntry> TimeEntries => Set<TimeEntry>();

        public ZezameDbContext(DbContextOptions<ZezameDbContext> options, ITenantContext tenant)
            : base(options) { 
            _tenant = tenant;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            // EXTENSION uuid-ossp
            modelBuilder.HasPostgresExtension("uuid-ossp");

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ZezameDbContext).Assembly);

            ConfigurePostgreSQL(modelBuilder);
        }

        private void ConfigurePostgreSQL(ModelBuilder modelBuilder) {
            foreach (var entity in modelBuilder.Model.GetEntityTypes()) {
                entity.SetTableName(entity.GetTableName()?.ToLowerInvariant());

                foreach (var property in entity.GetProperties()) {
                    property.SetColumnName(property.GetColumnName().ToLowerInvariant());
                }
            }

            // Filtro global de borrado lógico (soft-delete): ninguna consulta
            // devuelve registros con IsDeleted = true salvo que se ignore el filtro
            // explícitamente. Todas las entidades son consistentes (comparten IsDeleted),
            // así que los Include() entre ellas no generan avisos de EF.
            // Soft-delete global. Organization, User y ProjectMember NO se filtran por
            // tenant: el login/registro buscan usuarios por email ANTES de autenticar,
            // y la organización se resuelve por su clave.
            modelBuilder.Entity<Organization>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<User>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<ProjectMember>().HasQueryFilter(e => !e.IsDeleted);

            // Aislamiento multi-tenant (fail-closed): sin organización resuelta,
            // CurrentOrganizationId es null y estas consultas no devuelven nada.
            modelBuilder.Entity<Project>().HasQueryFilter(
                e => !e.IsDeleted && e.OrganizationId == _tenant.CurrentOrganizationId);
            modelBuilder.Entity<Activity>().HasQueryFilter(
                e => !e.IsDeleted && e.OrganizationId == _tenant.CurrentOrganizationId);
            modelBuilder.Entity<TimeEntry>().HasQueryFilter(
                e => !e.IsDeleted && e.OrganizationId == _tenant.CurrentOrganizationId);

            // Índices únicos
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // ===== Relaciones de organización (tenant) =====
            // Restrict en todas: una organización no se borra físicamente mientras
            // tenga datos (se usa borrado lógico). El aislamiento efectivo llega en Fase 3.
            modelBuilder.Entity<User>()
                .HasOne(u => u.Organization)
                .WithMany(o => o.Users)
                .HasForeignKey(u => u.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict); // OrganizationId es nullable (Platform Admin)

            modelBuilder.Entity<Project>()
                .HasOne(p => p.Organization)
                .WithMany(o => o.Projects)
                .HasForeignKey(p => p.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Activity>()
                .HasOne<Organization>()
                .WithMany()
                .HasForeignKey(a => a.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TimeEntry>()
                .HasOne<Organization>()
                .WithMany()
                .HasForeignKey(te => te.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>().HasIndex(u => u.OrganizationId);
            modelBuilder.Entity<Project>().HasIndex(p => p.OrganizationId);
            modelBuilder.Entity<Activity>().HasIndex(a => a.OrganizationId);
            modelBuilder.Entity<TimeEntry>().HasIndex(te => te.OrganizationId);

            // Relación User - Project (Owner)
            modelBuilder.Entity<Project>()
                .HasOne(p => p.Owner)
                .WithMany(u => u.OwnedProjects)
                .HasForeignKey(p => p.OwnerId)
                .OnDelete(DeleteBehavior.Restrict); // No borrar usuario si tiene proyectos

            // Relación many-to-many User - Project (Members)
            modelBuilder.Entity<ProjectMember>()
                .HasIndex(pm => new { pm.UserId, pm.ProjectId })
                .IsUnique();

            // Relación Activity - Project
            modelBuilder.Entity<Activity>()
                .HasOne(a => a.Project)
                .WithMany(p => p.Activities)
                .HasForeignKey(a => a.ProjectId)
                .OnDelete(DeleteBehavior.Cascade); // Borrar actividades si se borra proyecto

            // Relación Activity - User (AssignedTo)
            modelBuilder.Entity<Activity>()
                .HasOne(a => a.AssignedTo)
                .WithMany()
                .HasForeignKey(a => a.AssignedToId)
                .OnDelete(DeleteBehavior.SetNull);

            // Relación TimeEntry - User
            modelBuilder.Entity<TimeEntry>()
                .HasOne(te => te.User)
                .WithMany(u => u.TimeEntries)
                .HasForeignKey(te => te.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación TimeEntry - Project
            modelBuilder.Entity<TimeEntry>()
                .HasOne(te => te.Project)
                .WithMany(p => p.TimeEntries)
                .HasForeignKey(te => te.ProjectId)
                .OnDelete(DeleteBehavior.SetNull);

            // Relación TimeEntry - Activity
            modelBuilder.Entity<TimeEntry>()
                .HasOne(te => te.Activity)
                .WithMany(a => a.TimeEntries)
                .HasForeignKey(te => te.ActivityId)
                .OnDelete(DeleteBehavior.SetNull);

            // Índices para consultas frecuentes (performance)
            modelBuilder.Entity<TimeEntry>()
                .HasIndex(te => new { te.UserId, te.StartTime });

            modelBuilder.Entity<TimeEntry>()
                .HasIndex(te => new { te.UserId, te.Type, te.EndTime })
                .HasFilter("\"endtime\" IS NULL"); // Solo entradas activas

            modelBuilder.Entity<Activity>()
                .HasIndex(a => new { a.ProjectId, a.Status });

            modelBuilder.Entity<ProjectMember>()
                .HasIndex(pm => pm.ProjectId);
        }

        public override int SaveChanges() {
            foreach (var entry in ChangeTracker.Entries<TimeEntry>()) {
                if (entry.State == EntityState.Added)
                    entry.Entity.CreatedAt = DateTime.UtcNow;

                if (entry.State == EntityState.Modified)
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
            }

            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) {
            var entries = ChangeTracker.Entries<BaseEntity>();
            foreach (var entry in entries) {
                if (entry.State == EntityState.Modified) {
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                }
            }
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
