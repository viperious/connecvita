using Connecvita.Infrastructure.Data;
using Connecvita.Infrastructure.Wearables.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Connecvita.Infrastructure.Wearables.Sync;

public class WearableSyncBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<WearableSyncBackgroundService> _logger;
    private readonly TimeSpan _syncInterval = TimeSpan.FromHours(24);

    public WearableSyncBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<WearableSyncBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Wearable sync background service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            await SyncAllUsersAsync(stoppingToken);
            await Task.Delay(_syncInterval, stoppingToken);
        }
    }

    private async Task SyncAllUsersAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting daily wearable sync at {Time}", DateTime.UtcNow);

        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var clients = scope.ServiceProvider.GetServices<IWearableClient>().ToList();

        var tokens = await context.WearableTokens.ToListAsync(cancellationToken);

        foreach (var token in tokens)
        {
            try
            {
                var client = clients.FirstOrDefault(c => c.Platform == token.Platform);
                if (client is null) continue;

                // Refresh token if expired
                if (token.IsExpired)
                {
                    var refreshed = await client.RefreshTokenAsync(token);
                    token.UpdateTokens(refreshed.AccessToken, refreshed.RefreshToken, refreshed.ExpiresAt);
                    await context.SaveChangesAsync(cancellationToken);
                }

                // Find the user's profile
                var profile = await context.UserProfiles
                    .FirstOrDefaultAsync(p => p.UserId == token.UserId, cancellationToken);

                if (profile is null) continue;

                // Fetch and store latest metrics
                var metrics = await client.FetchLatestMetricsAsync(token, profile.Id);
                context.WearableMetrics.Add(metrics);

                // Update profile health scores
                var healthScores = new Domain.ValueObjects.HealthScores
                {
                    SleepScore = metrics.SleepScore,
                    ReadinessScore = metrics.ReadinessScore,
                    ActivityScore = metrics.ActivityScore,
                    RestingHeartRate = metrics.HeartRate,
                    HRV = metrics.HRV,
                    BodyTemperature = metrics.Temperature,
                    LastUpdated = DateTime.UtcNow
                };
                profile.UpdateHealthScores(healthScores);

                await context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Synced {Platform} for user {UserId}", token.Platform, token.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to sync {Platform} for user {UserId}",
                    token.Platform, token.UserId);
            }
        }

        _logger.LogInformation("Daily wearable sync completed at {Time}", DateTime.UtcNow);
    }
}