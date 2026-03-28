using Connecvita.Domain.Enums;

namespace Connecvita.Domain.Entities;

public class WearableToken
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string UserId { get; private set; } = string.Empty;
    public WearablePlatform Platform { get; private set; }
    public string AccessToken { get; private set; } = string.Empty;
    public string? RefreshToken { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    protected WearableToken() { }

    public static WearableToken Create(
        string userId,
        WearablePlatform platform,
        string accessToken,
        string? refreshToken,
        DateTime expiresAt)
    {
        return new WearableToken
        {
            UserId = userId,
            Platform = platform,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt
        };
    }

    public void UpdateTokens(string accessToken, string? refreshToken, DateTime expiresAt)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        ExpiresAt = expiresAt;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
}