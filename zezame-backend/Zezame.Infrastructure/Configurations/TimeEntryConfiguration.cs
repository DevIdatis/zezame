using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zezame.Domain.Entities;

namespace Zezame.Infrastructure.Configurations {
    public class TimeEntryConfiguration : IEntityTypeConfiguration<TimeEntry> {
        public void Configure(EntityTypeBuilder<TimeEntry> entity) {
            entity.ToTable("time_entries");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                  .HasDefaultValueSql("uuid_generate_v4()");

            entity.Property(e => e.StartTime)
                  .IsRequired();

            entity.Property(e => e.CreatedAt)
                  .HasDefaultValueSql("NOW()");

            entity.Property(e => e.UpdatedAt)
                  .HasDefaultValueSql("NOW()");

            entity.HasOne(e => e.User)
                  .WithMany(u => u.TimeEntries)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.UserId, e.StartTime })
                  .HasDatabaseName("idx_time_entries_user_date");
        }
    }
}
