using EventPlanning.Application.DTOs.Profile;

namespace EventPlanning.Application.Interfaces;

public interface IProfileService
{
    Task<EditProfileDto> GetProfileAsync(string userId, CancellationToken cancellationToken = default);
    
    Task UpdateProfileAsync(string userId, EditProfileDto dto, CancellationToken cancellationToken = default);
    
    Task ChangePasswordAsync(string userId, ChangePasswordDto dto, CancellationToken cancellationToken = default);
}