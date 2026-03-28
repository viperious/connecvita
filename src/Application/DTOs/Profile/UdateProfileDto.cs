using Connecvita.Domain.Enums;

namespace Connecvita.Application.DTOs.Profile;

public record UpdateProfileDto
{
    public string? Bio { get; init; }
    public string? Location { get; init; }
    public MatchingContext? MatchingContext { get; init; }
    public string? MatchingPreferences { get; init; }
    public decimal? HeightCm { get; init; }
    public decimal? WeightKg { get; init; }
    public BloodType? BloodType { get; init; }
}