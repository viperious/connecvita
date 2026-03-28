using Connecvita.Domain.Enums;

namespace Connecvita.Application.DTOs.Profile;

public record CreateProfileDto
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public DateTime DateOfBirth { get; init; }
    public string? Bio { get; init; }
    public string? Location { get; init; }
    public MatchingContext MatchingContext { get; init; } = MatchingContext.All;
}