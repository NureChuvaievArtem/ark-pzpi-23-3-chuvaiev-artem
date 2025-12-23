using Api.ApiResult;
using Api.Infrastructure.Errors;
using Api.Infrastructure.ResultPattern;
using Api.Models.DTOs;
using Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
/// Controller for mathematical data processing and analytics
/// Lab 3: Statistical analysis, predictions, optimization
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    /// <summary>
    /// Get statistical analysis of packages
    /// Mathematical methods: Mean, Median, Standard Deviation, Variance
    /// </summary>
    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics()
    {
        try
        {
            var stats = await _analyticsService.GetPackageStatisticsAsync();
            var result = Result<PackageStatistics>.Success(stats);
            return result.Match(
                successStatusCode: 200,
                failure: ApiResults.ToProblemDetails
            );
        }
        catch (Exception ex)
        {
            var result = Result<PackageStatistics>.Failure(
                Error.InternalServerError("analytics.STATISTICS_ERROR", ex.Message));
            return result.Match(
                successStatusCode: 200,
                failure: ApiResults.ToProblemDetails
            );
        }
    }

    /// <summary>
    /// Predict delivery time using linear regression
    /// </summary>
    [HttpGet("predict-delivery/{packageId}")]
    public async Task<IActionResult> PredictDeliveryTime(int packageId)
    {
        try
        {
            var prediction = await _analyticsService.PredictDeliveryTimeAsync(packageId);
            var result = Result<DeliveryTimePrediction>.Success(prediction);
            return result.Match(
                successStatusCode: 200,
                failure: ApiResults.ToProblemDetails
            );
        }
        catch (ArgumentException ex)
        {
            var result = Result<DeliveryTimePrediction>.Failure(
                Error.NotFound("analytics.PACKAGE_NOT_FOUND", ex.Message));
            return result.Match(
                successStatusCode: 200,
                failure: ApiResults.ToProblemDetails
            );
        }
    }

    /// <summary>
    /// Analyze user activity patterns
    /// </summary>
    [HttpGet("user-activity/{userId}")]
    public async Task<IActionResult> AnalyzeUserActivity(int userId)
    {
        try
        {
            var analysis = await _analyticsService.AnalyzeUserActivityAsync(userId);
            var result = Result<UserActivityAnalysis>.Success(analysis);
            return result.Match(
                successStatusCode: 200,
                failure: ApiResults.ToProblemDetails
            );
        }
        catch (ArgumentException ex)
        {
            var result = Result<UserActivityAnalysis>.Failure(
                Error.NotFound("analytics.USER_NOT_FOUND", ex.Message));
            return result.Match(
                successStatusCode: 200,
                failure: ApiResults.ToProblemDetails
            );
        }
    }
}

