using Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Infrastructure.Data.Configurations;

public class DeliveryStatusConfiguration : IEntityTypeConfiguration<DeliveryStatus>
{
    public void Configure(EntityTypeBuilder<DeliveryStatus> builder)
    {
        builder.HasKey(ds => ds.Id);

        builder.Property(ds => ds.Id)
            .UseIdentityColumn();

        builder.Property(ds => ds.Name)
            .IsRequired()
            .HasMaxLength(100);
    }
}
