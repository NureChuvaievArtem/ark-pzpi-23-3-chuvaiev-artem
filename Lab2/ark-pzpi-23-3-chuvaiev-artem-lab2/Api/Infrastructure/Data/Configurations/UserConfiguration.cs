using Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .UseIdentityColumn();

        builder.Property(u => u.EmailAddress)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.SerialNfcData)
            .HasMaxLength(255);
    }
}

