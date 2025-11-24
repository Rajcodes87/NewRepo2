using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Volo.Abp.Application.Services;
using Volo.Abp.Caching;
using System.Threading.Tasks;

namespace Pawchums.Services;

/// <summary>
/// Service for generating and validating location sharing tokens
/// </summary>
public class LocationTokenService : ApplicationService
{
    private readonly IDistributedCache<LocationTokenCacheItem> _cache;
    private readonly ILogger<LocationTokenService> _logger;

    public LocationTokenService(
        IDistributedCache<LocationTokenCacheItem> cache,
        ILogger<LocationTokenService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Generate a secure token for location sharing
    /// </summary>
    public async Task<string> GenerateTokenAsync(Guid requestRescueId, Guid rescuerId)
    {
        var token = GenerateSecureToken();

        var cacheItem = new LocationTokenCacheItem
        {
            RequestRescueId = requestRescueId,
            RescuerId = rescuerId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7) // Token valid for 7 days
        };

        // Store in cache with expiration
        await _cache.SetAsync(
            token,
            cacheItem,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(7)
            });

        _logger.LogInformation("Generated location token for rescuer {RescuerId} and request {RequestId}",
            rescuerId, requestRescueId);

        return token;
    }

    /// <summary>
    /// Validate a location sharing token
    /// </summary>
    public async Task<LocationTokenCacheItem?> ValidateTokenAsync(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return null;
        }

        var cacheItem = await _cache.GetAsync(token);

        if (cacheItem == null)
        {
            _logger.LogWarning("Invalid or expired location token: {Token}", token);
            return null;
        }

        if (cacheItem.ExpiresAt < DateTime.UtcNow)
        {
            _logger.LogWarning("Expired location token: {Token}", token);
            await _cache.RemoveAsync(token);
            return null;
        }

        return cacheItem;
    }

    /// <summary>
    /// Invalidate a token after use
    /// </summary>
    public async Task InvalidateTokenAsync(string token)
    {
        await _cache.RemoveAsync(token);
        _logger.LogInformation("Invalidated location token: {Token}", token);
    }

    private string GenerateSecureToken()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[32];
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
    }
}

/// <summary>
/// Cache item for location tokens
/// </summary>
public class LocationTokenCacheItem
{
    public Guid RequestRescueId { get; set; }
    public Guid RescuerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}