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
    // Список кодів для парсингу. 
    // В реальному проекті це краще винести в конфігурацію або окремий сервіс.
    private readonly string[] _supportedCodes = ["+380", "+1", "+44", "+48", "+49"];

    public async Task<EditProfileDto> GetProfileAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null) throw new KeyNotFoundException("User not found");

        var organizedEvents = await eventRepository.GetFilteredAsync(
            userId, null, null, null, null, null, null, 1, 1, cancellationToken);
        
        var joinedCount = await eventRepository.CountJoinedEventsAsync(userId, cancellationToken);

        // Розділяємо повний номер на код та тіло
        var (code, number) = ParsePhoneNumber(user.PhoneNumber);

        return new EditProfileDto
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            CountryCode = code,       // <--- Заповнюємо код
            PhoneNumber = number,     // <--- Заповнюємо лише номер абонента
            Email = user.Email,
            OrganizedCount = organizedEvents.TotalCount,
            JoinedCount = joinedCount
        };
    }

    public async Task UpdateProfileAsync(string userId, EditProfileDto dto, CancellationToken cancellationToken = default)
    {
        var validationResult = await profileValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid) 
            throw new FluentValidation.ValidationException(validationResult.Errors);

        var user = await userManager.FindByIdAsync(userId);
        if (user == null) throw new KeyNotFoundException("User not found");

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;

        // Об'єднуємо код та номер для збереження в базу.
        // Якщо номер не введено, зберігаємо null (щоб не було "+380" без цифр).
        user.PhoneNumber = string.IsNullOrEmpty(dto.PhoneNumber) 
            ? null 
            : $"{dto.CountryCode}{dto.PhoneNumber}";

        var result = await userManager.UpdateAsync(user);
        
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => new FluentValidation.Results.ValidationFailure(string.Empty, e.Description));
            throw new FluentValidation.ValidationException(errors);
        }
    }

    public async Task ChangePasswordAsync(string userId, ChangePasswordDto dto, CancellationToken cancellationToken = default)
    {
        var validationResult = await passwordValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid) 
            throw new FluentValidation.ValidationException(validationResult.Errors);

        var user = await userManager.FindByIdAsync(userId);
        if (user == null) throw new KeyNotFoundException("User not found");

        var result = await userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => new FluentValidation.Results.ValidationFailure(string.Empty, e.Description));
            throw new FluentValidation.ValidationException(errors);
        }
    }

    // Helper метод для визначення коду країни з повного номера
    private (string Code, string Number) ParsePhoneNumber(string? fullNumber)
    {
        if (string.IsNullOrEmpty(fullNumber)) return ("+380", string.Empty); // Дефолтне значення

        // Сортуємо коди за довжиною (desc), щоб спочатку перевірити довші коди (наприклад +1 vs +123)
        // Хоча в нашому списку конфліктів немає, це гарна практика.
        foreach (var code in _supportedCodes)
        {
            if (fullNumber.StartsWith(code))
            {
                // Повертаємо знайдений код і решту рядка як номер
                return (code, fullNumber.Substring(code.Length));
            }
        }

        // Якщо код не розпізнано (старий формат або інша країна), 
        // лишаємо +380 і повертаємо весь рядок в поле номера, щоб юзер це побачив і виправив.
        return ("+380", fullNumber); 
    }
}