using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zezame.Domain.Entities;

namespace Zezame.Infrastructure.Configurations {
    public class OrganizationConfiguration : IEntityTypeConfiguration<Organization> {
        public void Configure(EntityTypeBuilder<Organization> entity) {
            entity.ToTable("organizations");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                  .HasDefaultValueSql("uuid_generate_v4()");

            entity.Property(e => e.Name)
                  .HasMaxLength(150)
                  .IsRequired();

            entity.Property(e => e.CreatedAt)
                  .HasDefaultValueSql("NOW()");
        }
    }
}
