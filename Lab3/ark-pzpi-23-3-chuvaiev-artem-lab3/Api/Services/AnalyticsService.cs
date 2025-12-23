using Api.Infrastructure.Repository;
using Api.Models;
using Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

/// <summary>
/// Mathematical data processing service
/// Implements statistical analysis, machine learning predictions, and optimization algorithms
/// </summary>
public class AnalyticsService : IAnalyticsService
{
    private readonly IGenericRepository<Package> _packageRepository;
    private readonly IGenericRepository<User> _userRepository;

    public AnalyticsService(
        IGenericRepository<Package> packageRepository,
        IGenericRepository<User> userRepository)
    {
        _packageRepository = packageRepository;
        _userRepository = userRepository;
    }

    /// <summary>
    /// Statistical Analysis using descriptive statistics
    /// Calculates: Mean, Median, Standard Deviation, Variance
    /// Mathematical justification: Provides data distribution insights for system optimization
    /// </summary>
    public async Task<PackageStatistics> GetPackageStatisticsAsync()
    {
        var packages = await _packageRepository.GetAllAsync(
            includeProperties: "Category,DeliveryStatus,User"
        );

        if (!packages.Any())
        {
            return new PackageStatistics
            {
                TotalPackages = 0,
                PackagesByCategory = new Dictionary<string, int>(),
                PackagesByStatus = new Dictionary<string, int>()
            };
        }

        // Calculate volumes
        var volumes = packages.Select(p => (double)(p.Height * p.Width * p.Depth)).ToList();
        
        // Calculate mean (average)
        var mean = volumes.Average();
        
        // Calculate median
        var sortedVolumes = volumes.OrderBy(v => v).ToList();
        var median = sortedVolumes.Count % 2 == 0
            ? (sortedVolumes[sortedVolumes.Count / 2 - 1] + sortedVolumes[sortedVolumes.Count / 2]) / 2.0
            : sortedVolumes[sortedVolumes.Count / 2];
        
        // Calculate variance and standard deviation
        var variance = volumes.Sum(v => Math.Pow(v - mean, 2)) / volumes.Count;
        var standardDeviation = Math.Sqrt(variance);

        // Calculate average delivery time (assuming CreatedAt is pickup time)
        var deliveryTimes = packages
            .Select(p => (p.LastModifiedOn - p.CreatedOn).TotalHours)
            .ToList();
        
        var avgDeliveryTime = deliveryTimes.Any() ? deliveryTimes.Average() : 0;

        return new PackageStatistics
        {
            TotalPackages = packages.Count(),
            AverageVolume = mean,
            MedianVolume = median,
            StandardDeviation = standardDeviation,
            Variance = variance,
            PackagesByCategory = packages.GroupBy(p => p.Category.Name)
                .ToDictionary(g => g.Key, g => g.Count()),
            PackagesByStatus = packages.GroupBy(p => p.DeliveryStatus.Name)
                .ToDictionary(g => g.Key, g => g.Count()),
            AverageDeliveryTimeHours = avgDeliveryTime
        };
    }

    /// <summary>
    /// Linear Regression for delivery time prediction
    /// Mathematical model: y = β₀ + β₁x₁ + β₂x₂ + ... + βₙxₙ
    /// Where: y = delivery time, x = factors (volume, category weight, etc.)
    /// Justification: Predicts delivery times for resource planning and customer expectations
    /// </summary>
    public async Task<DeliveryTimePrediction> PredictDeliveryTimeAsync(int packageId)
    {
        var package = await _packageRepository.GetByIdAsync(
            packageId,
            includeProperties: "Category,DeliveryStatus"
        );

        if (package == null)
        {
            throw new ArgumentException($"Package with ID {packageId} not found");
        }

        var allPackages = await _packageRepository.GetAllAsync(
            includeProperties: "Category,DeliveryStatus"
        );

        // Simple linear regression based on volume
        var volume = package.Height * package.Width * package.Depth;
        
        // Calculate regression coefficients from historical data
        var historicalData = allPackages
            .Where(p => p.Id != packageId)
            .Select(p => new
            {
                Volume = (double)(p.Height * p.Width * p.Depth),
                DeliveryTime = (p.LastModifiedOn - p.CreatedOn).TotalHours
            })
            .ToList();

        double predictedTime;
        double confidence;
        var factors = new Dictionary<string, double>();

        if (historicalData.Count > 1)
        {
            // Linear regression: y = a + bx
            var n = historicalData.Count;
            var sumX = historicalData.Sum(d => d.Volume);
            var sumY = historicalData.Sum(d => d.DeliveryTime);
            var sumXY = historicalData.Sum(d => d.Volume * d.DeliveryTime);
            var sumX2 = historicalData.Sum(d => d.Volume * d.Volume);

            var denominator = n * sumX2 - sumX * sumX;
            
            // Check for division by zero (happens when all volumes are the same)
            if (Math.Abs(denominator) < 0.0001)
            {
                predictedTime = historicalData.Average(d => d.DeliveryTime);
                confidence = 50;
                factors["fallback"] = 1.0;
            }
            else
            {
                var b = (n * sumXY - sumX * sumY) / denominator;
                var a = (sumY - b * sumX) / n;

                predictedTime = a + b * volume;
                
                var categoryFactor = package.Category?.Name switch
                {
                    "Express" => 0.5,  // 50% faster
                    "Standard" => 1.0,
                    "Economy" => 1.5,  // 50% slower
                    _ => 1.0
                };
                
                predictedTime *= categoryFactor;
                
                // Calculate R² for confidence
                var yMean = historicalData.Average(d => d.DeliveryTime);
                var ssTot = historicalData.Sum(d => Math.Pow(d.DeliveryTime - yMean, 2));
                
                // Check for division by zero (happens when all delivery times are the same)
                if (ssTot < 0.0001)
                {
                    confidence = 50;
                    factors["volumeCoefficient"] = b;
                    factors["baseTime"] = a;
                    factors["categoryFactor"] = categoryFactor;
                    factors["rSquared"] = 0;
                }
                else
                {
                    var ssRes = historicalData.Sum(d => Math.Pow(d.DeliveryTime - (a + b * d.Volume), 2));
                    var rSquared = 1 - (ssRes / ssTot);
                    
                    // Ensure rSquared is valid (not infinity or NaN)
                    if (double.IsInfinity(rSquared) || double.IsNaN(rSquared))
                    {
                        rSquared = 0;
                    }
                    
                    confidence = Math.Max(0, Math.Min(100, rSquared * 100));
                    
                    factors["volumeCoefficient"] = b;
                    factors["baseTime"] = a;
                    factors["categoryFactor"] = categoryFactor;
                    factors["rSquared"] = rSquared;
                }
            }
        }
        else
        {
            // Fallback: simple heuristic
            predictedTime = 24 + (volume / 10000.0); // Base 24 hours + volume factor
            confidence = 50;
            factors["fallback"] = 1.0;
        }

        // Ensure predictedTime is valid (not infinity or NaN)
        if (double.IsInfinity(predictedTime) || double.IsNaN(predictedTime))
        {
            predictedTime = 24; // Fallback to 24 hours
            confidence = 50;
            factors["fallback"] = 1.0;
        }

        return new DeliveryTimePrediction
        {
            PackageId = packageId,
            PredictedDeliveryTimeHours = Math.Max(1, Math.Min(predictedTime, 8760)), // Clamp between 1 hour and 1 year
            ConfidenceScore = Math.Max(0, Math.Min(100, confidence)),
            Method = "Linear Regression",
            Factors = factors
        };
    }

    /// <summary>
    /// Time Series Analysis for user activity patterns
    /// Mathematical method: Trend analysis using moving averages
    /// Justification: Identifies usage patterns for capacity planning
    /// </summary>
    public async Task<UserActivityAnalysis> AnalyzeUserActivityAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId, includeProperties: "Packages");
        
        if (user == null)
        {
            throw new ArgumentException($"User with ID {userId} not found");
        }

        var packages = user.Packages.ToList();

        if (!packages.Any())
        {
            return new UserActivityAnalysis
            {
                UserId = userId,
                TotalPackages = 0,
                PackagesByHour = new Dictionary<int, int>(),
                PackagesByDayOfWeek = new Dictionary<string, int>(),
                ActivityTrend = 0,
                PeakActivityTime = "N/A"
            };
        }

        // Group by hour of day
        var byHour = packages
            .GroupBy(p => p.CreatedOn.Hour)
            .ToDictionary(g => g.Key, g => g.Count());

        // Group by day of week
        var byDayOfWeek = packages
            .GroupBy(p => p.CreatedOn.DayOfWeek.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        // Calculate trend using simple moving average comparison
        var orderedPackages = packages.OrderBy(p => p.CreatedOn).ToList();
        var trend = 0.0;
        
        if (orderedPackages.Count >= 4)
        {
            var midpoint = orderedPackages.Count / 2;
            var firstHalf = orderedPackages.Take(midpoint).Count();
            var secondHalf = orderedPackages.Skip(midpoint).Count();
            trend = ((double)secondHalf - firstHalf) / firstHalf * 100;
        }

        var peakHour = byHour.Any() 
            ? byHour.OrderByDescending(kvp => kvp.Value).First().Key 
            : 0;

        return new UserActivityAnalysis
        {
            UserId = userId,
            TotalPackages = packages.Count,
            PackagesByHour = byHour,
            PackagesByDayOfWeek = byDayOfWeek,
            ActivityTrend = trend,
            PeakActivityTime = $"{peakHour:D2}:00 - {(peakHour + 1):D2}:00"
        };
    }
}

