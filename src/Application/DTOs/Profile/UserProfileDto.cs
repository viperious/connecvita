using Connecvita.Domain.Enums;

public record UserProfileDto
{
    public Guid Id { get; init; }
    public string UserId { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public DateTime DateOfBirth { get; init; }
    public string? Bio { get; init; }
    public string? Location { get; init; }
    public MatchingContext MatchingContext { get; init; }
    public string? MatchingPreferences { get; init; }
    public List<string> PhotoUrls { get; init; } = [];
    public bool IsComplete { get; init; }

    // Health scores
    public int? SleepScore { get; init; }
    public int? ReadinessScore { get; init; }
    public int? ActivityScore { get; init; }

    // Physical attributes
    public decimal? HeightCm { get; init; }
    public decimal? WeightCm { get; init; }
    public BloodType? BloodType { get; init; }

    public List<WearablePlatform> ConnectedPlatforms { get; init; } = [];
}