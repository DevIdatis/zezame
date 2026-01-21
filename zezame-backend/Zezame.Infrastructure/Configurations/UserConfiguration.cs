using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zezame.Domain.Entities;

namespace Zezame.Infrastructure.Configurations {
    public class UserConfiguration : IEntityTypeConfiguration<User> {
        public void Configure(EntityTypeBuilder<User> entity) {
            entity.ToTable("users");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                  .HasDefaultValueSql("uuid_generate_v4()");

            entity.Property(e => e.Email)
                  .HasMaxLength(120)
                  .IsRequired();

            entity.HasIndex(e => e.Email)
                  .IsUnique();

            entity.Property(e => e.PasswordHash)
                  .HasMaxLength(256)
                  .IsRequired();

            entity.Property(e => e.FullName)
                  .HasMaxLength(120)
                  .IsRequired();

            entity.Property(e => e.CreatedAt)
                  .HasDefaultValueSql("NOW()");
        }
    }
}
