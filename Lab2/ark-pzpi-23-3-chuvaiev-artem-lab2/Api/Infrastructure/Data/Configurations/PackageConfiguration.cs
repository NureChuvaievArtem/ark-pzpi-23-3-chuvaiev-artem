using Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Infrastructure.Data.Configurations;

public class PackageConfiguration : IEntityTypeConfiguration<Package>
{
    public void Configure(EntityTypeBuilder<Package> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .UseIdentityColumn();

        builder.Property(p => p.Height)
            .IsRequired();

        builder.Property(p => p.Width)
            .IsRequired();

        builder.Property(p => p.Depth)
            .IsRequired();

        builder.Property(p => p.PostBoxId)
            .IsRequired();

        builder.HasOne(p => p.Category)
            .WithMany()
            .HasForeignKey("CategoryId")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.DeliveryStatus)
            .WithMany()
            .HasForeignKey("DeliveryStatusId")
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(p => p.User)
            .WithMany(u => u.Packages)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

