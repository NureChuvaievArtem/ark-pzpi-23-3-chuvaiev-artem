using System.Globalization;
using System.Text;
using System.Text.Json;
using Api.Infrastructure.Data;
using Api.Infrastructure.Repository;
using Api.Models;
using Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public class AdminService : IAdminService
{
    private readonly ApplicationDbContext _context;
    private readonly IGenericRepository<User> _userRepository;
    private readonly IGenericRepository<Package> _packageRepository;
    private readonly IGenericRepository<Role> _roleRepository;
    private readonly IGenericRepository<DeliveryStatus> _deliveryStatusRepository;
    private readonly IGenericRepository<PackageCategory> _packageCategoryRepository;
    private readonly string _backupDirectory;

    public AdminService(
        ApplicationDbContext context,
        IGenericRepository<User> userRepository,
        IGenericRepository<Package> packageRepository,
        IGenericRepository<Role> roleRepository,
        IGenericRepository<DeliveryStatus> deliveryStatusRepository,
        IGenericRepository<PackageCategory> packageCategoryRepository)
    {
        _context = context;
        _userRepository = userRepository;
        _packageRepository = packageRepository;
        _roleRepository = roleRepository;
        _deliveryStatusRepository = deliveryStatusRepository;
        _packageCategoryRepository = packageCategoryRepository;
        _backupDirectory = Path.Combine(Directory.GetCurrentDirectory(), "backups");
        
        // Create backup directory if it doesn't exist
        Directory.CreateDirectory(_backupDirectory);
    }

    public async Task<ExportResult> ExportDataAsync(string format)
    {
        try
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var fileName = $"export_{timestamp}.{format.ToLower()}";
            var exportDirectory = Path.Combine(Directory.GetCurrentDirectory(), "exports");
            Directory.CreateDirectory(exportDirectory);
            var filePath = Path.Combine(exportDirectory, fileName);

            var packages = await _packageRepository.GetAllAsync(includeProperties: "User,Category,DeliveryStatus");
            var recordCount = packages.Count();

            if (format.ToLower() == "json")
            {
                var json = JsonSerializer.Serialize(packages, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                await File.WriteAllTextAsync(filePath, json);
            }
            else if (format.ToLower() == "csv")
            {
                var csv = new StringBuilder();
                csv.AppendLine("Id,Height,Width,Depth,PostBoxId,UserId,UserEmail,Category,DeliveryStatus,CreatedAt");

                foreach (var package in packages)
                {
                    csv.AppendLine($"{package.Id},{package.Height},{package.Width},{package.Depth}," +
                                   $"{package.PostBoxId},{package.UserId},{package.User?.EmailAddress ?? ""}," +
                                   $"{package.Category?.Name ?? ""},{package.DeliveryStatus?.Name ?? ""}," +
                                   $"{package.CreatedOn:O}");
                }

                await File.WriteAllTextAsync(filePath, csv.ToString());
            }
            else
            {
                throw new ArgumentException($"Unsupported format: {format}");
            }

            return new ExportResult
            {
                Success = true,
                FileName = fileName,
                FilePath = filePath,
                Format = format.ToUpper(),
                RecordCount = recordCount
            };
        }
        catch (Exception ex)
        {
            return new ExportResult
            {
                Success = false,
                Format = format
            };
        }
    }

    public async Task<DataStatistics> GetDataStatisticsAsync()
    {
        var users = await _userRepository.GetAllAsync();
        var packages = await _packageRepository.GetAllAsync();
        var roles = await _roleRepository.GetAllAsync();
        var deliveryStatuses = await _deliveryStatusRepository.GetAllAsync();
        var packageCategories = await _packageCategoryRepository.GetAllAsync();


        return new DataStatistics
        {
            TotalUsers = users.Count(),
            TotalPackages = packages.Count(),
            TotalRoles = roles.Count(),
            TotalDeliveryStatuses = deliveryStatuses.Count(),
            TotalPackageCategories = packageCategories.Count(),
            DatabaseSizeBytes = 0, // Would require database-specific query
        };
    }

    public async Task<CleanupResult> CleanupOldDataAsync(int daysOld)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
            
            var oldPackages = await _packageRepository.GetAllAsync(
                filter: p => p.CreatedOn < cutoffDate
            );

            var deletedCount = 0;
            foreach (var package in oldPackages)
            {
                await _packageRepository.DeleteAsync(package.Id);
                deletedCount++;
            }

            await _packageRepository.SaveAsync();

            return new CleanupResult
            {
                Success = true,
                PackagesDeleted = deletedCount,
                LogsDeleted = 0,
                Message = $"Cleaned up {deletedCount} old packages"
            };
        }
        catch (Exception ex)
        {
            return new CleanupResult
            {
                Success = false,
                Message = $"Cleanup failed: {ex.Message}"
            };
        }
    }
}

