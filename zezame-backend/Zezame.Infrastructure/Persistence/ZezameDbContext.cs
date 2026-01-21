using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Zezame.Domain.Entities;

namespace Zezame.Infrastructure.Persistence {
    public class ZezameDbContext : DbContext {
        public DbSet<User> Users => Set<User>();
        public DbSet<TimeEntry> TimeEntries => Set<TimeEntry>();

        public ZezameDbContext(DbContextOptions<ZezameDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            // EXTENSION uuid-ossp
            modelBuilder.HasPostgresExtension("uuid-ossp");

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ZezameDbContext).Assembly);
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
    }
}
