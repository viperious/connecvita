using Connecvita.Domain.Enums;
using Connecvita.Domain.ValueObjects;

namespace Connecvita.Domain.Entities;

public class UserProfile
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string UserId { get; private set; } = string.Empty;

    // Basic info
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public DateTime DateOfBirth { get; private set; }
    public string? Bio { get; private set; }
    public string? Location { get; private set; }

    // Physical
    public PhysicalAttributes? PhysicalAttributes { get; private set; }

    // Health
    public HealthScores? LatestHealthScores { get; private set; }

    // Matching
    public MatchingContext MatchingContext { get; private set; }
    public string? MatchingPreferences { get; private set; }

    // Media
    public List<string> PhotoUrls { get; private set; } = [];
    public string? VoiceProfileUrl { get; private set; }

    // Wearables
    public List<WearablePlatform> ConnectedPlatforms { get; private set; } = [];
    public List<WearableMetrics> WearableHistory { get; private set; } = [];

    // Metadata
    public bool IsComplete { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    protected UserProfile() { }

    public static UserProfile Create(string userId, string firstName, string lastName, DateTime dateOfBirth)
    {
        return new UserProfile
        {
            UserId = userId,
            FirstName = firstName,
            LastName = lastName,
            DateOfBirth = dateOfBirth
        };
    }

    public void UpdateBio(string? bio, string? location)
    {
        Bio = bio;
        Location = location;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePhysicalAttributes(PhysicalAttributes attributes)
    {
        PhysicalAttributes = attributes;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateHealthScores(HealthScores scores)
    {
        LatestHealthScores = scores;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateMatchingPreferences(MatchingContext context, string? preferences)
    {
        MatchingContext = context;
        MatchingPreferences = preferences;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddPhoto(string url)
    {
        PhotoUrls.Add(url);
        UpdatedAt = DateTime.UtcNow;
    }

    public void ConnectPlatform(WearablePlatform platform)
    {
        if (!ConnectedPlatforms.Contains(platform))
            ConnectedPlatforms.Add(platform);
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkComplete()
    {
        IsComplete = true;
        UpdatedAt = DateTime.UtcNow;
    }
}