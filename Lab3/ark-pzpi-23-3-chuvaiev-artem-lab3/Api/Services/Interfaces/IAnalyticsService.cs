using Api.Models;

namespace Api.Services.Interfaces;

/// <summary>
/// Service for mathematical data processing and analytics
/// Implements statistical analysis, predictions, and optimization algorithms
/// </summary>
public interface IAnalyticsService
{
    /// <summary>
    /// Calculates statistical metrics for packages
    /// Uses: Mean, Median, Standard Deviation, Variance
    /// </summary>
    Task<PackageStatistics> GetPackageStatisticsAsync();
    
    /// <summary>
    /// Predicts delivery time using linear regression
    /// Based on package dimensions, category, and historical data
    /// </summary>
    Task<DeliveryTimePrediction> PredictDeliveryTimeAsync(int packageId);
    
    /// <summary>
    /// Analyzes user activity patterns using time series analysis
    /// </summary>
    Task<UserActivityAnalysis> AnalyzeUserActivityAsync(int userId);
}

public class PackageStatistics
{
    public int TotalPackages { get; set; }
    public double AverageVolume { get; set; }
    public double MedianVolume { get; set; }
    public double StandardDeviation { get; set; }
    public double Variance { get; set; }
    public Dictionary<string, int> PackagesByCategory { get; set; }
    public Dictionary<string, int> PackagesByStatus { get; set; }
    public double AverageDeliveryTimeHours { get; set; }
}

public class DeliveryTimePrediction
{
    public int PackageId { get; set; }
    public double PredictedDeliveryTimeHours { get; set; }
    public double ConfidenceScore { get; set; }
    public string Method { get; set; } // "Linear Regression"
    public Dictionary<string, double> Factors { get; set; }
}

public class OptimalPlacementResult
{
    public int SuggestedPostBoxId { get; set; }
    public double SpaceUtilization { get; set; }
    public double EfficiencyScore { get; set; }
    public string Algorithm { get; set; } // "First Fit Decreasing Bin Packing"
    public List<AlternativePlacement> Alternatives { get; set; }
}

public class AlternativePlacement
{
    public int PostBoxId { get; set; }
    public double UtilizationScore { get; set; }
}

public class UserActivityAnalysis
{
    public int UserId { get; set; }
    public int TotalPackages { get; set; }
    public Dictionary<int, int> PackagesByHour { get; set; } // Hour -> Count
    public Dictionary<string, int> PackagesByDayOfWeek { get; set; }
    public double ActivityTrend { get; set; } // Positive = increasing, Negative = decreasing
    public string PeakActivityTime { get; set; }
}

public class DistributionAnalysis
{
    public List<HistogramBin> VolumeDistribution { get; set; }
    public double Skewness { get; set; }
    public double Kurtosis { get; set; }
    public List<int> Outliers { get; set; } // Package IDs
}

public class HistogramBin
{
    public double RangeStart { get; set; }
    public double RangeEnd { get; set; }
    public int Count { get; set; }
    public double Frequency { get; set; }
}

public class PostBoxCapacityData
{
    public int Id { get; set; }
    public int MaxVolume { get; set; }
    public int CurrentUsage { get; set; }
}

