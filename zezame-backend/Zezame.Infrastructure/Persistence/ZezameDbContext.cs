using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Zezame.Domain.Base;
using Zezame.Domain.Entities;

namespace Zezame.Infrastructure.Persistence {
    public class ZezameDbContext : DbContext {
        public DbSet<User> Users => Set<User>();
        public DbSet<Project> Projects => Set<Project>();
        public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();
        public DbSet<Activity> Activities => Set<Activity>();
        public DbSet<TimeEntry> TimeEntries => Set<TimeEntry>();

        public ZezameDbContext(DbContextOptions<ZezameDbContext> options)
            : base(options) { }

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

            // Índices únicos
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

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
