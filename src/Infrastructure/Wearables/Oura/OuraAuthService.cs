using Connecvita.Domain.Entities;
using Connecvita.Domain.Enums;
using Connecvita.Infrastructure.Data;
using Connecvita.Infrastructure.Wearables.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Connecvita.Infrastructure.Wearables.Oura;

public class OuraAuthService
{
    private readonly AppDbContext _context;
    private readonly IWearableClient _ouraClient;
    private readonly string _redirectUri;

    public OuraAuthService(
        AppDbContext context,
        OuraClient ouraClient,
        IConfiguration configuration)
    {
        _context = context;
        _ouraClient = ouraClient;
        _redirectUri = configuration["Wearables:Oura:RedirectUri"]!;
    }

    public string GetAuthorizationUrl(string userId)
        => _ouraClient.GetAuthorizationUrl(userId, _redirectUri);

    public async Task HandleCallbackAsync(string code, string userId)
    {
        // Exchange code for token
        var token = await _ouraClient.ExchangeCodeForTokenAsync(code, userId, _redirectUri);

        // Save or update token in DB
        var existing = await _context.WearableTokens
            .FirstOrDefaultAsync(t => t.UserId == userId && t.Platform == WearablePlatform.OuraRing);

        if (existing is not null)
        {
            existing.UpdateTokens(token.AccessToken, token.RefreshToken, token.ExpiresAt);
        }
        else
        {
            _context.WearableTokens.Add(token);
        }

        // Mark Oura as connected on the user's profile
        var profile = await _context.UserProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile is not null)
        {
            profile.ConnectPlatform(WearablePlatform.OuraRing);
        }

        await _context.SaveChangesAsync();

        // Kick off initial historical sync
        var historicalMetrics = await _ouraClient.FetchHistoricalMetricsAsync(
            token, profile!.Id, DateTime.UtcNow.AddDays(-30));

        foreach (var metrics in historicalMetrics)
        {
            var existingMetrics = await _context.WearableMetrics
                .FirstOrDefaultAsync(m =>
                    m.UserProfileId == profile.Id &&
                    m.Platform == WearablePlatform.OuraRing &&
                    m.RecordedAt.Date == metrics.RecordedAt.Date);

            if (existingMetrics is null)
                _context.WearableMetrics.Add(metrics);
        }

        // Update profile with latest health scores
        var latest = historicalMetrics.OrderByDescending(m => m.RecordedAt).FirstOrDefault();
        if (latest is not null)
        {
            var healthScores = new Domain.ValueObjects.HealthScores
            {
                SleepScore = latest.SleepScore,
                ReadinessScore = latest.ReadinessScore,
                ActivityScore = latest.ActivityScore,
                RestingHeartRate = latest.HeartRate,
                HRV = latest.HRV,
                BodyTemperature = latest.Temperature,
                LastUpdated = DateTime.UtcNow
            };
            profile.UpdateHealthScores(healthScores);
        }

        await _context.SaveChangesAsync();
    }

    public async Task ResyncAsync(string userId)
    {
        var token = await _context.WearableTokens
            .FirstOrDefaultAsync(t => t.UserId == userId &&
                t.Platform == WearablePlatform.OuraRing)
            ?? throw new InvalidOperationException("No Oura token found. Please connect first.");

        // Refresh token if expired
        if (token.IsExpired)
        {
            var refreshed = await _ouraClient.RefreshTokenAsync(token);
            token.UpdateTokens(refreshed.AccessToken, refreshed.RefreshToken, refreshed.ExpiresAt);
            await _context.SaveChangesAsync();
        }

        var profile = await _context.UserProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId)
            ?? throw new InvalidOperationException("Profile not found.");

        // Delete existing Oura metrics before re-syncing
        var existing = _context.WearableMetrics
            .Where(m => m.UserProfileId == profile.Id && m.Platform == WearablePlatform.OuraRing);
        _context.WearableMetrics.RemoveRange(existing);
        await _context.SaveChangesAsync();

        // Fetch fresh historical data
        var historicalMetrics = await _ouraClient.FetchHistoricalMetricsAsync(
            token, profile.Id, DateTime.UtcNow.AddDays(-30));

        foreach (var metrics in historicalMetrics)
            _context.WearableMetrics.Add(metrics);

        // Update profile health scores with latest
        var latest = historicalMetrics.OrderByDescending(m => m.RecordedAt).FirstOrDefault();
        if (latest is not null)
        {
            var healthScores = new Domain.ValueObjects.HealthScores
            {
                SleepScore = latest.SleepScore,
                ReadinessScore = latest.ReadinessScore,
                ActivityScore = latest.ActivityScore,
                RestingHeartRate = latest.HeartRate,
                HRV = latest.HRV,
                BodyTemperature = latest.Temperature,
                LastUpdated = DateTime.UtcNow
            };
            profile.UpdateHealthScores(healthScores);
        }

        await _context.SaveChangesAsync();
    }
}