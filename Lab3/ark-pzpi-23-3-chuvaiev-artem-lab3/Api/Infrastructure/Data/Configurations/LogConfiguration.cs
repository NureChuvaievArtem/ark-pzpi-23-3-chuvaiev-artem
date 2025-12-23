using Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Infrastructure.Data.Configurations;

public class LogConfiguration : IEntityTypeConfiguration<AppLog>
{
    public void Configure(EntityTypeBuilder<AppLog> builder)
    {
        builder.HasKey(l => l.Id);

        builder.Property(l => l.Id)
            .UseIdentityColumn();

        builder.Property(l => l.BoxId);

        builder.Property(l => l.Message)
            .IsRequired()
            .HasMaxLength(1000);

        builder.HasIndex(l => l.BoxId);
    }
}

