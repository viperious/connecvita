namespace Connecvita.Domain.ValueObjects;

public record HealthScores
{
    public int? SleepScore { get; init; }
    public int? ReadinessScore { get; init; }
    public int? ActivityScore { get; init; }
    public double? RestingHeartRate { get; init; }
    public double? HRV { get; init; }
    public double? BodyTemperature { get; init; }
    public DateTime LastUpdated { get; init; }
}