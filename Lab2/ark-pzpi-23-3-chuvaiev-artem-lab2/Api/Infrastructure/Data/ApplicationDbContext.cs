using Api.Infrastructure.Data.Configurations;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<DeliveryStatus> DeliveryStatuses { get; set; }

    public DbSet<AppLog> AppLogs { get; set; }

    public DbSet<Package> Packages { get; set; }
    
    public DbSet<PackageCategory> PackageCategories { get; set; }

    public DbSet<Role> Roles { get; set; }

    public DbSet<User> Users { get; set; }

    public DbSet<UserRole> UserRoles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new DeliveryStatusConfiguration());
        modelBuilder.ApplyConfiguration(new LogConfiguration());
        modelBuilder.ApplyConfiguration(new PackageCategoryConfiguration());
        modelBuilder.ApplyConfiguration(new PackageConfiguration());
        modelBuilder.ApplyConfiguration(new RoleConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new UserRolesConfiguration());
    }
}