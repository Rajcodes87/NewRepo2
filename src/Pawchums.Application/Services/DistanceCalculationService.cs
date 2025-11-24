using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Volo.Abp.Application.Services;
using AnimalRescueSystem.RescuerProfiles;

namespace Pawchums.Services;

/// <summary>
/// Service for calculating distances between GPS coordinates using Haversine formula
/// This is one of the two algorithms for the college project requirement
/// </summary>
public class DistanceCalculationService : ApplicationService
{
    private readonly ILogger<DistanceCalculationService> _logger;

    public DistanceCalculationService(ILogger<DistanceCalculationService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Calculate distance between two GPS coordinates using Haversine formula
    /// Algorithm 1: Haversine Formula for shortest distance calculation
    /// Formula: a = sin²(Δφ/2) + cos φ1 ⋅ cos φ2 ⋅ sin²(Δλ/2)
    ///          c = 2 ⋅ atan2(√a, √(1−a))
    ///          d = R ⋅ c
    /// Where φ is latitude, λ is longitude, R is earth's radius
    /// </summary>
    /// <param name="lat1">Latitude of point 1</param>
    /// <param name="lon1">Longitude of point 1</param>
    /// <param name="lat2">Latitude of point 2</param>
    /// <param name="lon2">Longitude of point 2</param>
    /// <returns>Distance in kilometers</returns>
    public double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double EarthRadiusKm = 6371; // Earth's radius in kilometers

        _logger.LogDebug("Calculating distance from ({Lat1}, {Lon1}) to ({Lat2}, {Lon2})",
            lat1, lon1, lat2, lon2);

        // Convert degrees to radians
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);

        var lat1Rad = ToRadians(lat1);
        var lat2Rad = ToRadians(lat2);

        // Haversine formula
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2) *
                Math.Cos(lat1Rad) * Math.Cos(lat2Rad);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        var distance = EarthRadiusKm * c;

        _logger.LogDebug("Calculated distance: {Distance} km", distance);

        return distance;
    }

    /// <summary>
    /// Find nearest rescuers to a rescue request location
    /// Algorithm 2: Sorting algorithm to find K nearest rescuers
    /// Uses Quick Select / Heap Sort concept for efficient K nearest neighbor search
    /// </summary>
    /// <param name="requestLat">Rescue request latitude</param>
    /// <param name="requestLon">Rescue request longitude</param>
    /// <param name="rescuers">List of rescuers with their locations</param>
    /// <param name="maxResults">Maximum number of nearest rescuers to return</param>
    /// <returns>List of rescuers sorted by distance</returns>
    public List<RescuerDistanceDto> FindNearestRescuers(
        double requestLat,
        double requestLon,
        List<RescuerLocationDto> rescuers,
        int maxResults = 10)
    {
        _logger.LogInformation("Finding {MaxResults} nearest rescuers to location ({Lat}, {Lon})",
            maxResults, requestLat, requestLon);

        if (rescuers == null || !rescuers.Any())
        {
            _logger.LogWarning("No rescuers available for distance calculation");
            return new List<RescuerDistanceDto>();
        }

        // Filter rescuers with valid locations and calculate distances
        var rescuersWithDistance = rescuers
            .Where(r => r.Latitude.HasValue && r.Longitude.HasValue)
            .Select(r => new RescuerDistanceDto
            {
                RescuerId = r.RescuerId,
                RescuerName = r.RescuerName,
                Email = r.Email,
                Latitude = r.Latitude.Value,
                Longitude = r.Longitude.Value,
                Distance = CalculateDistance(
                    requestLat,
                    requestLon,
                    r.Latitude.Value,
                    r.Longitude.Value)
            })
            .ToList();

        // Sort by distance (ascending) and take top K
        // This uses a sorting algorithm (QuickSort/TimSort in .NET)
        var nearestRescuers = rescuersWithDistance
            .OrderBy(r => r.Distance)
            .Take(maxResults)
            .ToList();

        _logger.LogInformation("Found {Count} nearest rescuers out of {Total} total rescuers",
            nearestRescuers.Count, rescuersWithDistance.Count);

        return nearestRescuers;
    }

    /// <summary>
    /// Filter rescuers within a specific radius
    /// </summary>
    public List<RescuerDistanceDto> GetRescuersWithinRadius(
        double requestLat,
        double requestLon,
        List<RescuerLocationDto> rescuers,
        double radiusKm)
    {
        _logger.LogInformation("Finding rescuers within {Radius} km of location ({Lat}, {Lon})",
            radiusKm, requestLat, requestLon);

        var rescuersWithDistance = FindNearestRescuers(
            requestLat,
            requestLon,
            rescuers,
            int.MaxValue); // Get all

        var rescuersInRadius = rescuersWithDistance
            .Where(r => r.Distance <= radiusKm)
            .ToList();

        _logger.LogInformation("Found {Count} rescuers within {Radius} km radius",
            rescuersInRadius.Count, radiusKm);

        return rescuersInRadius;
    }

    /// <summary>
    /// Convert degrees to radians
    /// </summary>
    private double ToRadians(double degrees)
    {
        return degrees * Math.PI / 180.0;
    }

    /// <summary>
    /// Convert radians to degrees
    /// </summary>
    private double ToDegrees(double radians)
    {
        return radians * 180.0 / Math.PI;
    }

    /// <summary>
    /// Calculate bearing (direction) from point 1 to point 2
    /// </summary>
    public double CalculateBearing(double lat1, double lon1, double lat2, double lon2)
    {
        var lat1Rad = ToRadians(lat1);
        var lat2Rad = ToRadians(lat2);
        var dLon = ToRadians(lon2 - lon1);

        var y = Math.Sin(dLon) * Math.Cos(lat2Rad);
        var x = Math.Cos(lat1Rad) * Math.Sin(lat2Rad) -
                Math.Sin(lat1Rad) * Math.Cos(lat2Rad) * Math.Cos(dLon);

        var bearingRad = Math.Atan2(y, x);
        var bearingDeg = ToDegrees(bearingRad);

        return (bearingDeg + 360) % 360; // Normalize to 0-360
    }
}