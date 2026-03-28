using Connecvita.Domain.Enums;

namespace Connecvita.Domain.ValueObjects;

public record PhysicalAttributes
{
    public decimal? HeightCm { get; init; }
    public decimal? WeightKg { get; init; }
    public string? ChestCm { get; init; }
    public string? WaistCm { get; init; }
    public string? HipsCm { get; init; }
    public string? GeneticData { get; init; }
    public BloodType BloodType { get; init; }
}