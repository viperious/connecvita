using System.Text.Json.Serialization;

namespace Connecvita.Infrastructure.Wearables.Oura.Models;

public record OuraTokenResponse(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("refresh_token")] string RefreshToken,
    [property: JsonPropertyName("expires_in")] int ExpiresIn,
    [property: JsonPropertyName("token_type")] string TokenType
);

public record OuraSleepData(
    [property: JsonPropertyName("day")] string Day,
    [property: JsonPropertyName("score")] int? Score,
    [property: JsonPropertyName("average_hrv")] double? AverageHrv,
    [property: JsonPropertyName("average_heart_rate")] double? AverageHeartRate,
    [property: JsonPropertyName("average_breath")] double? AverageBreath,
    [property: JsonPropertyName("lowest_heart_rate")] double? LowestHeartRate,
    [property: JsonPropertyName("temperature_delta")] double? TemperatureDelta
);

public record OuraReadinessData(
    [property: JsonPropertyName("day")] string Day,
    [property: JsonPropertyName("score")] int? Score,
    [property: JsonPropertyName("temperature_deviation")] double? TemperatureDeviation
);

public record OuraActivityData(
    [property: JsonPropertyName("day")] string Day,
    [property: JsonPropertyName("score")] int? Score,
    [property: JsonPropertyName("active_calories")] int? ActiveCalories,
    [property: JsonPropertyName("steps")] int? Steps
);

public record OuraSleepDetailData(
    [property: JsonPropertyName("day")] string Day,
    [property: JsonPropertyName("average_hrv")] double? AverageHrv,
    [property: JsonPropertyName("lowest_heart_rate")] double? LowestHeartRate,
    [property: JsonPropertyName("average_heart_rate")] double? AverageHeartRate,
    [property: JsonPropertyName("time_in_bed")] int? TimeInBed,
    [property: JsonPropertyName("total_sleep_duration")] int? TotalSleepDuration,
    [property: JsonPropertyName("efficiency")] int? Efficiency
);

public record OuraSpO2Data(
    [property: JsonPropertyName("day")] string Day,
    [property: JsonPropertyName("spo2_percentage")] OuraSpO2Percentage? SpO2Percentage
);

public record OuraSpO2Percentage(
    [property: JsonPropertyName("average")] double? Average
);

public record OuraStressData(
    [property: JsonPropertyName("day")] string Day,
    [property: JsonPropertyName("stress_high")] int? StressHigh,
    [property: JsonPropertyName("recovery_high")] int? RecoveryHigh,
    [property: JsonPropertyName("day_summary")] string? DaySummary
);

public record OuraDataResponse<T>(
    [property: JsonPropertyName("data")] List<T> Data
);