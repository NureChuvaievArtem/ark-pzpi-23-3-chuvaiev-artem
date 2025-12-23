using Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Infrastructure.Data.Configurations;

public class PackageCategoryConfiguration : IEntityTypeConfiguration<PackageCategory>
{
    public void Configure(EntityTypeBuilder<PackageCategory> builder)
    {
        builder.HasKey(pc => pc.Id);

        builder.Property(pc => pc.Id)
            .UseIdentityColumn();

        builder.Property(pc => pc.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(pc => pc.IsFragile)
            .IsRequired()
            .HasDefaultValue(false);
    }
}

