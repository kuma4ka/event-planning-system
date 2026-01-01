using EventPlanning.Application.Constants;
using EventPlanning.Domain.ValueObjects;
using EventPlanning.Application.DTOs.Profile;
using EventPlanning.Application.Interfaces;
using EventPlanning.Domain.Interfaces;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace EventPlanning.Application.Services;

public class ProfileService(
    IIdentityService identityService,
    IEventRepository eventRepository,
    IUserRepository userRepository,
    IValidator<EditProfileDto> profileValidator,
    IValidator<ChangePasswordDto> passwordValidator,
    ICountryService countryService,
    ICacheService cacheService,
    ILogger<ProfileService> logger) : IProfileService
{
    public async Task<EditProfileDto> GetProfileAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null) throw new KeyNotFoundException("User not found");

        var organizedEvents = await eventRepository.GetFilteredAsync(
            userId, null, null, null, null, null, null, 1, 1, cancellationToken);

        var joinedCount = await eventRepository.CountJoinedEventsAsync(userId, cancellationToken);

        var (code, number) = countryService.ParsePhoneNumber(user.PhoneNumber);

        return new EditProfileDto
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            CountryCode = code,
            PhoneNumber = number,
            Email = user.Email,
            OrganizedCount = organizedEvents.TotalCount,
            JoinedCount = joinedCount
        };
    }

    public async Task UpdateProfileAsync(string userId, EditProfileDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await profileValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var user = await userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null) throw new KeyNotFoundException("User not found");

        var newFullPhoneNumber = string.IsNullOrEmpty(dto.PhoneNumber)
            ? null
            : $"{dto.CountryCode}{dto.PhoneNumber}";

        if (newFullPhoneNumber != user.PhoneNumber && !string.IsNullOrEmpty(newFullPhoneNumber))
        {
            var isPhoneTaken = await userRepository.IsPhoneNumberTakenAsync(newFullPhoneNumber, userId, cancellationToken);

            if (isPhoneTaken)
            {
                logger.LogWarning("Profile update failed: Phone number {PhoneNumber} taken", newFullPhoneNumber);
                throw new ValidationException([
                    new ValidationFailure("PhoneNumber", "This phone number is already linked to another account.")
                ]);
            }
            
            var (succeeded, errors) = await identityService.UpdatePhoneNumberAsync(userId, newFullPhoneNumber);
            if (!succeeded)
            {
                logger.LogWarning("Identity phone update failed for user {UserId}: {Errors}", userId, string.Join(", ", errors));
                 throw new ValidationException(errors.Select(e => new ValidationFailure(string.Empty, e)));
            }
        }

        user.UpdateProfile(dto.FirstName, dto.LastName);
        
        if (newFullPhoneNumber != user.PhoneNumber)
        {
             user.UpdatePhoneNumber(newFullPhoneNumber);
        }
        
        user.SetCountryCode(dto.CountryCode);

        await userRepository.UpdateAsync(user, cancellationToken);

        var affectedEventIds = await eventRepository.UpdateGuestDetailsAsync(
            user.Email!, 
            user.FirstName, 
            user.LastName, 
            user.CountryCode, 
            user.PhoneNumber, 
            cancellationToken);

        foreach (var eventId in affectedEventIds)
        {
            cacheService.Remove($"{CachedEventService.EventCacheKeyPrefix}{eventId}_public");
            cacheService.Remove($"{CachedEventService.EventCacheKeyPrefix}{eventId}_organizer");
        }
    }

    public async Task ChangePasswordAsync(string userId, ChangePasswordDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await passwordValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var (succeeded, errors) = await identityService.ChangePasswordAsync(userId, dto.CurrentPassword, dto.NewPassword);

        if (!succeeded)
        {
            logger.LogWarning("Password change failed for user {UserId}: {Errors}", userId, string.Join(", ", errors));
             throw new ValidationException(errors.Select(e => new ValidationFailure(string.Empty, e)));
        }
    }

}