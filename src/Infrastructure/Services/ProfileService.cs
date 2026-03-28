using Connecvita.Application.DTOs.Profile;
using Connecvita.Application.Interfaces;
using Connecvita.Domain.Entities;
using Connecvita.Domain.ValueObjects;
using Connecvita.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Connecvita.Infrastructure.Services;

public class ProfileService : IProfileService
{
    private readonly AppDbContext _context;

    public ProfileService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<UserProfileDto?> GetByUserIdAsync(string userId)
    {
        var profile = await _context.UserProfiles
            .FirstOrDefaultAsync(x => x.UserId == userId);

        return profile is null ? null : MapToDto(profile);
    }

    public async Task<UserProfileDto> CreateAsync(string userId, CreateProfileDto dto)
    {
        // Check if profile already exists — update instead of insert
        var existing = await _context.UserProfiles
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (existing is not null)
        {
            existing.UpdateBio(dto.Bio, dto.Location);
            existing.UpdateMatchingPreferences(dto.MatchingContext, null);
            existing.MarkComplete();
            await _context.SaveChangesAsync();
            return MapToDto(existing);
        }

        var profile = UserProfile.Create(
            userId,
            dto.FirstName,
            dto.LastName,
            dto.DateOfBirth);

        profile.UpdateBio(dto.Bio, dto.Location);
        profile.UpdateMatchingPreferences(dto.MatchingContext, null);
        profile.MarkComplete();

        _context.UserProfiles.Add(profile);
        await _context.SaveChangesAsync();

        return MapToDto(profile);
    }

    public async Task<UserProfileDto> UpdateAsync(string userId, UpdateProfileDto dto)
    {
        var profile = await _context.UserProfiles
            .FirstOrDefaultAsync(x => x.UserId == userId)
            ?? throw new InvalidOperationException("Profile not found.");

        // Update bio and location if provided
        profile.UpdateBio(
            dto.Bio ?? profile.Bio,
            dto.Location ?? profile.Location
        );

        if (dto.MatchingContext.HasValue)
            profile.UpdateMatchingPreferences(dto.MatchingContext.Value, dto.MatchingPreferences);

        if (dto.HeightCm.HasValue || dto.WeightKg.HasValue || dto.BloodType.HasValue)
        {
            var physical = new PhysicalAttributes
            {
                HeightCm = dto.HeightCm,
                WeightKg = dto.WeightKg,
                BloodType = dto.BloodType ?? Domain.Enums.BloodType.Unknown
            };
            profile.UpdatePhysicalAttributes(physical);
        }

        await _context.SaveChangesAsync();
        return MapToDto(profile);
    }

    public async Task AddPhotoAsync(string userId, string photoUrl)
    {
        var profile = await _context.UserProfiles
            .FirstOrDefaultAsync(x => x.UserId == userId)
            ?? throw new InvalidOperationException("Profile not found.");

        profile.AddPhoto(photoUrl);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> IsProfileCompleteAsync(string userId)
    {
        var profile = await _context.UserProfiles
            .FirstOrDefaultAsync(x => x.UserId == userId);

        return profile?.IsComplete ?? false;
    }

    private static UserProfileDto MapToDto(UserProfile profile) => new()
    {
        Id = profile.Id,
        UserId = profile.UserId,
        FirstName = profile.FirstName,
        LastName = profile.LastName,
        DateOfBirth = profile.DateOfBirth,
        Bio = profile.Bio,
        Location = profile.Location,
        MatchingContext = profile.MatchingContext,
        MatchingPreferences = profile.MatchingPreferences,
        PhotoUrls = profile.PhotoUrls,
        IsComplete = profile.IsComplete,
        SleepScore = profile.LatestHealthScores?.SleepScore,
        ReadinessScore = profile.LatestHealthScores?.ReadinessScore,
        ActivityScore = profile.LatestHealthScores?.ActivityScore,
        HeightCm = profile.PhysicalAttributes?.HeightCm,
        WeightCm = profile.PhysicalAttributes?.WeightKg,
        BloodType = profile.PhysicalAttributes?.BloodType,
        ConnectedPlatforms = profile.ConnectedPlatforms
    };
}