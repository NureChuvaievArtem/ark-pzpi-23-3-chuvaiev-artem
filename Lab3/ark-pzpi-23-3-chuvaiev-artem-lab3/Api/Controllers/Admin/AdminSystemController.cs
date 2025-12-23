using Api.ApiResult;
using Api.Infrastructure.Errors;
using Api.Infrastructure.ResultPattern;
using Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Admin;

/// <summary>
/// System administration operations
/// Backup, restore, export, import, configuration, statistics
/// </summary>
[ApiController]
[Route("api/admin/system")]
public class AdminSystemController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminSystemController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    #region Export & Import

    [HttpGet("export/{format}")]
    public async Task<IActionResult> ExportData(string format)
    {
        var exportResult = await _adminService.ExportDataAsync(format);
        
        if (!exportResult.Success)
        {
            var result = Result.Failure(Error.InternalServerError("admin.EXPORT_FAILED", "Export failed"));
            return result.MatchNoData(
                successStatusCode: 200,
                failure: ApiResults.ToProblemDetails
            );
        }

        var fileBytes = await System.IO.File.ReadAllBytesAsync(exportResult.FilePath);
        var contentType = format.ToLower() == "json" ? "application/json" : "text/csv";
        return File(fileBytes, contentType, exportResult.FileName);
    }
    #endregion

    #region Statistics & Cleanup

    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics()
    {
        var stats = await _adminService.GetDataStatisticsAsync();
        var result = Result<DataStatistics>.Success(stats);
        
        return result.Match(
            successStatusCode: 200,
            failure: ApiResults.ToProblemDetails
        );
    }

    [HttpDelete("cleanup/{daysOld}")]
    public async Task<IActionResult> CleanupOldData(int daysOld)
    {
        var cleanupResult = await _adminService.CleanupOldDataAsync(daysOld);
        
        if (!cleanupResult.Success)
        {
            var result = Result<CleanupResult>.Failure(
                Error.InternalServerError("admin.CLEANUP_FAILED", cleanupResult.Message)
            );
            return result.Match(
                successStatusCode: 200,
                failure: ApiResults.ToProblemDetails
            );
        }

        var successResult = Result<CleanupResult>.Success(cleanupResult);
        return successResult.Match(
            successStatusCode: 200,
            failure: ApiResults.ToProblemDetails
        );
    }

    #endregion
}

