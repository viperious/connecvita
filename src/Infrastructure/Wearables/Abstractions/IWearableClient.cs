using Connecvita.Domain.Entities;
using Connecvita.Domain.Enums;

namespace Connecvita.Infrastructure.Wearables.Abstractions;

public interface IWearableClient
{
    WearablePlatform Platform { get; }
    string GetAuthorizationUrl(string userId, string redirectUri);
    Task<WearableToken> ExchangeCodeForTokenAsync(string code, string userId, string redirectUri);
    Task<WearableToken> RefreshTokenAsync(WearableToken token);
    Task<WearableMetrics> FetchLatestMetricsAsync(WearableToken token, Guid userProfileId);
    Task<List<WearableMetrics>> FetchHistoricalMetricsAsync(WearableToken token, Guid userProfileId, DateTime from);
}