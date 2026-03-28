using Connecvita.Application.DTOs.Profile;

namespace Connecvita.Application.Interfaces;

public interface IProfileService
{
    Task<UserProfileDto?> GetByUserIdAsync(string userId);
    Task<UserProfileDto> CreateAsync(string userId, CreateProfileDto dto);
    Task<UserProfileDto> UpdateAsync(string userId, UpdateProfileDto dto);
    Task AddPhotoAsync(string userId, string photoUrl);
    Task<bool> IsProfileCompleteAsync(string userId);
}