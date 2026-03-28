using Connecvita.Domain.Enums;

namespace Connecvita.Domain.Entities;

public class WearableMetrics
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UserProfileId { get; private set; }
    public WearablePlatform Platform { get; private set; }
    public int? SleepScore { get; private set; }
    public int? ReadinessScore { get; private set; }
    public int? ActivityScore { get; private set; }
    public double? HeartRate { get; private set; }
    public double? HRV { get; private set; }
    public double? Temperature { get; private set; }
    public string? WorkoutSummary { get; private set; }
    public List<string> Tags { get; private set; } = [];
    public string? SessionData { get; private set; }
    public DateTime RecordedAt { get; private set; }
    public DateTime SyncedAt { get; private set; }

    protected WearableMetrics() { }

    public static WearableMetrics Create(
        Guid userProfileId,
        WearablePlatform platform,
        DateTime recordedAt)
    {
        return new WearableMetrics
        {
            UserProfileId = userProfileId,
            Platform = platform,
            RecordedAt = recordedAt,
            SyncedAt = DateTime.UtcNow
        };
    }

    public void UpdateMetrics(
        int? sleepScore, int? readinessScore, int? activityScore,
        double? heartRate, double? hrv, double? temperature,
        string? workoutSummary, List<string>? tags, string? sessionData)
    {
        SleepScore = sleepScore;
        ReadinessScore = readinessScore;
        ActivityScore = activityScore;
        HeartRate = heartRate;
        HRV = hrv;
        Temperature = temperature;
        WorkoutSummary = workoutSummary;
        Tags = tags ?? [];
        SessionData = sessionData;
        SyncedAt = DateTime.UtcNow;
    }
}