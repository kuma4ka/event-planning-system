using EventPlanning.Application.Constants;
using EventPlanning.Application.DTOs.Profile;
using EventPlanning.Application.Interfaces;
using EventPlanning.Domain.Entities;
using EventPlanning.Domain.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace EventPlanning.Application.Services;

public class ProfileService(
    UserManager<User> userManager,
    IEventRepository eventRepository,
    IValidator<EditProfileDto> profileValidator,
    IValidator<ChangePasswordDto> passwordValidator) : IProfileService
{
    public async Task<EditProfileDto> GetProfileAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null) throw new KeyNotFoundException("User not found");

        var organizedEvents = await eventRepository.GetFilteredAsync(
            userId, null, null, null, null, null, null, 1, 1, cancellationToken);

        var joinedCount = await eventRepository.CountJoinedEventsAsync(userId, cancellationToken);

        var (code, number) = ParsePhoneNumber(user.PhoneNumber);

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

        var user = await userManager.FindByIdAsync(userId);
        if (user == null) throw new KeyNotFoundException("User not found");

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;

        user.PhoneNumber = string.IsNullOrEmpty(dto.PhoneNumber) ? null : $"{dto.CountryCode}{dto.PhoneNumber}";

        var result = await userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e =>
                new FluentValidation.Results.ValidationFailure(string.Empty, e.Description));
            throw new ValidationException(errors);
        }
    }

    public async Task ChangePasswordAsync(string userId, ChangePasswordDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await passwordValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var user = await userManager.FindByIdAsync(userId);
        if (user == null) throw new KeyNotFoundException("User not found");

        var result = await userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e =>
                new FluentValidation.Results.ValidationFailure(string.Empty, e.Description));
            throw new ValidationException(errors);
        }
    }

    private (string Code, string Number) ParsePhoneNumber(string? fullNumber)
    {
        if (string.IsNullOrEmpty(fullNumber))
            return (CountryConstants.DefaultCode, string.Empty);

        var matchedCountry = CountryConstants.SupportedCountries
            .OrderByDescending(c => c.Code.Length)
            .FirstOrDefault(c => fullNumber.StartsWith(c.Code));

        if (matchedCountry != null)
        {
            var numberBody = fullNumber.Substring(matchedCountry.Code.Length);
            return (matchedCountry.Code, numberBody);
        }

        return (CountryConstants.DefaultCode, fullNumber);
    }
}