using Api.Models;

namespace Api.Services.Interfaces;

/// <summary>
/// Administrative service for system management
/// Provides backup, restore, export, import functionality
/// </summary>
public interface IAdminService
{
    // Export and Import
    Task<ExportResult> ExportDataAsync(string format); // json, csv
    
    // Data Management
    Task<DataStatistics> GetDataStatisticsAsync();
    Task<CleanupResult> CleanupOldDataAsync(int daysOld);
}

public class BackupResult
{
    public bool Success { get; set; }
    public string FileName { get; set; }
    public string FilePath { get; set; }
    public long FileSizeBytes { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Message { get; set; }
}

public class RestoreResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public int RecordsRestored { get; set; }
    public DateTime RestoredAt { get; set; }
}

public class BackupInfo
{
    public string FileName { get; set; }
    public long FileSizeBytes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ExportResult
{
    public bool Success { get; set; }
    public string FileName { get; set; }
    public string FilePath { get; set; }
    public string Format { get; set; }
    public int RecordCount { get; set; }
}

public class ImportResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public int RecordsImported { get; set; }
    public int RecordsFailed { get; set; }
    public List<string> Errors { get; set; }
}

public class SystemConfiguration
{
    public string SystemName { get; set; }
    public string Version { get; set; }
    public string DefaultLanguage { get; set; }
    public string DefaultTimeZone { get; set; }
    public bool MaintenanceMode { get; set; }
    public int MaxPackageRetentionDays { get; set; }
    public bool EnableEmailNotifications { get; set; }
    public Dictionary<string, string> CustomSettings { get; set; }
}

public class DataStatistics
{
    public int TotalUsers { get; set; }
    public int TotalPackages { get; set; }
    public int TotalRoles { get; set; }
    public int TotalDeliveryStatuses { get; set; }
    public int TotalPackageCategories { get; set; }
    public long DatabaseSizeBytes { get; set; }
}

public class CleanupResult
{
    public bool Success { get; set; }
    public int PackagesDeleted { get; set; }
    public int LogsDeleted { get; set; }
    public string Message { get; set; }
}

