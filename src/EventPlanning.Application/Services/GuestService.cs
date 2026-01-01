using EventPlanning.Application.DTOs.Guest;
using EventPlanning.Application.Interfaces;
using EventPlanning.Domain.Entities;
using EventPlanning.Domain.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace EventPlanning.Application.Services;

public class GuestService(
    IGuestRepository guestRepository,
    IEventRepository eventRepository,
    IValidator<CreateGuestDto> createValidator,
    IValidator<AddGuestManuallyDto> manualAddValidator,
    IValidator<UpdateGuestDto> updateValidator,
    IMemoryCache cache,
    ILogger<GuestService> logger) : IGuestService
{
    private const string EventCacheKeyPrefix = "event_details_";

    public async Task AddGuestAsync(string userId, CreateGuestDto dto, CancellationToken cancellationToken = default)
    {
        var validationResult = await createValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid) throw new ValidationException(validationResult.Errors);

        var eventEntity = await eventRepository.GetByIdAsync(dto.EventId, cancellationToken);

        if (eventEntity == null) throw new KeyNotFoundException("Event not found");
        if (eventEntity.OrganizerId != userId)
        {
            logger.LogWarning("Unauthorized guest add attempt by {UserId} for event {EventId}", userId, dto.EventId);
            throw new UnauthorizedAccessException("Not your event");
        }

        await CheckUniqueEmailAsync(dto.EventId, dto.Email, null, cancellationToken);

        var guest = CreateGuestEntity(dto);
        await guestRepository.AddAsync(guest, cancellationToken);

        InvalidateEventCache(dto.EventId);
    }

    public async Task AddGuestManuallyAsync(string currentUserId, AddGuestManuallyDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await manualAddValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var eventEntity = await eventRepository.GetByIdAsync(dto.EventId, cancellationToken);
        if (eventEntity == null) throw new KeyNotFoundException("Event not found");

        if (eventEntity.OrganizerId != currentUserId)
            throw new UnauthorizedAccessException("Only the organizer can add guests manually.");

        if (eventEntity.Date < DateTime.Now)
            throw new InvalidOperationException("Cannot add guests to an event that has already ended.");

        await CheckCapacityAsync(eventEntity, dto.EventId, cancellationToken);
        await CheckUniqueEmailAsync(dto.EventId, dto.Email, null, cancellationToken);

        var fullPhoneNumber = dto.CountryCode + dto.PhoneNumber.Replace(" ", "").Replace("-", "");
        if (!string.IsNullOrEmpty(dto.PhoneNumber))
        {
            await CheckUniquePhoneAsync(dto.EventId, fullPhoneNumber, null, cancellationToken);
        }

        var guest = CreateGuestEntity(dto);

        await guestRepository.AddAsync(guest, cancellationToken);

        InvalidateEventCache(dto.EventId);
    }

    public async Task UpdateGuestAsync(string currentUserId, UpdateGuestDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await updateValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid) throw new ValidationException(validationResult.Errors);

        var guest = await guestRepository.GetByIdAsync(dto.Id, cancellationToken);
        if (guest == null) throw new KeyNotFoundException($"Guest with ID {dto.Id} not found.");

        var eventEntity = guest.Event ?? await eventRepository.GetByIdAsync(guest.EventId, cancellationToken);
        if (eventEntity == null) throw new KeyNotFoundException("Event not found.");

        if (eventEntity.OrganizerId != currentUserId)
            throw new UnauthorizedAccessException("Not your event. Only the organizer can update guests.");

        if (guest.UserId != null)
        {
            throw new InvalidOperationException("Cannot edit a guest who is a registered user. They must update their own profile.");
        }

        if (!guest.Email.Value.Equals(dto.Email, StringComparison.OrdinalIgnoreCase))
        {
            await CheckUniqueEmailAsync(guest.EventId, dto.Email, guest.Id, cancellationToken);
        }

        var newFullPhone = dto.CountryCode + dto.PhoneNumber;
        if ((guest.PhoneNumber != null ? guest.PhoneNumber.Value : null) != newFullPhone && !string.IsNullOrEmpty(dto.PhoneNumber))
        {
             await CheckUniquePhoneAsync(guest.EventId, newFullPhone, guest.Id, cancellationToken);
        }

        guest.UpdateDetails(dto.FirstName, dto.LastName, dto.Email, dto.CountryCode, newFullPhone);

        await guestRepository.UpdateAsync(guest, cancellationToken);

        InvalidateEventCache(guest.EventId);
    }

    public async Task RemoveGuestAsync(string userId, Guid guestId, CancellationToken cancellationToken = default)
    {
        var guest = await guestRepository.GetByIdAsync(guestId, cancellationToken);
        if (guest == null) return;

        Guid eventId = guest.EventId;

        if (guest.Event == null)
        {
            var eventEntity = await eventRepository.GetByIdAsync(guest.EventId, cancellationToken);
            if (eventEntity == null || eventEntity.OrganizerId != userId)
                throw new UnauthorizedAccessException("Not your event");
        }
        else if (guest.Event.OrganizerId != userId)
        {
            throw new UnauthorizedAccessException("Not your event");
        }

        await guestRepository.DeleteAsync(guest, cancellationToken);

        InvalidateEventCache(eventId);
    }

    private void InvalidateEventCache(Guid eventId)
    {
        cache.Remove($"{EventCacheKeyPrefix}{eventId}_public");
        cache.Remove($"{EventCacheKeyPrefix}{eventId}_organizer");
    }

    private static Guest CreateGuestEntity(GuestBaseDto dto)
    {
        return new Guest(
            dto.FirstName,
            dto.LastName,
            dto.Email,
            dto.EventId,
            dto.CountryCode,
            dto.CountryCode + dto.PhoneNumber
        );
    }

    private async Task CheckCapacityAsync(Event eventEntity, Guid eventId, CancellationToken cancellationToken)
    {
        if (eventEntity.Venue is { Capacity: > 0 })
        {
            var guestCount = await guestRepository.CountGuestsAtEventAsync(eventId, cancellationToken);
            if (guestCount >= eventEntity.Venue.Capacity)
                throw new InvalidOperationException("Venue is fully booked.");
        }
    }

    private async Task CheckUniqueEmailAsync(Guid eventId, string email, Guid? excludeGuestId, CancellationToken cancellationToken)
    {
        if (await guestRepository.EmailExistsAtEventAsync(eventId, email, excludeGuestId, cancellationToken))
        {
            throw new InvalidOperationException($"Guest with email '{email}' is already registered for this event.");
        }
    }

    private async Task CheckUniquePhoneAsync(Guid eventId, string phoneNumber, Guid? excludeGuestId, CancellationToken cancellationToken)
    {
        bool phoneExists = await guestRepository.PhoneExistsAtEventAsync(eventId, phoneNumber, excludeGuestId, cancellationToken);
        if (phoneExists)
        {
            var displayPhone = phoneNumber.StartsWith("+") ? phoneNumber : $"+{phoneNumber}";
            throw new InvalidOperationException($"Guest with phone number {displayPhone} is already added to this event.");
        }
    }
}