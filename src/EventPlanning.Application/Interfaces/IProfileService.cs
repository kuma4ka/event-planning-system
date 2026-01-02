using EventPlanning.Application.DTOs.Profile;

namespace EventPlanning.Application.Interfaces;

public interface IProfileService
{
    Task<EditProfileDto> GetProfileAsync(Guid userId, CancellationToken cancellationToken = default);
    
    Task UpdateProfileAsync(Guid userId, EditProfileDto dto, CancellationToken cancellationToken = default);
    
    Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto, CancellationToken cancellationToken = default);
}