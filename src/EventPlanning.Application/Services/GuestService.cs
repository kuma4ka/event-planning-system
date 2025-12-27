using EventPlanning.Application.DTOs.Guest;
using EventPlanning.Application.Interfaces;
using EventPlanning.Domain.Entities;
using EventPlanning.Domain.Interfaces;
using FluentValidation;

namespace EventPlanning.Application.Services;

public class GuestService(
    IGuestRepository guestRepository,
    IEventRepository eventRepository,
    IValidator<CreateGuestDto> createValidator,
    IValidator<AddGuestManuallyDto> manualAddValidator,
    IValidator<UpdateGuestDto> updateValidator
) : IGuestService
{
    public async Task AddGuestAsync(string userId, CreateGuestDto dto, CancellationToken cancellationToken = default)
    {
        var validationResult = await createValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid) throw new ValidationException(validationResult.Errors);

        var eventEntity = await eventRepository.GetByIdAsync(dto.EventId, cancellationToken);

        if (eventEntity == null) throw new KeyNotFoundException("Event not found");
        if (eventEntity.OrganizerId != userId) throw new UnauthorizedAccessException("Not your event");

        var guest = CreateGuestEntity(dto);

        await guestRepository.AddAsync(guest, cancellationToken);
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

        if (eventEntity.Venue != null && eventEntity.Venue.Capacity > 0 &&
            eventEntity.Guests.Count >= eventEntity.Venue.Capacity)
            throw new InvalidOperationException("Venue is fully booked.");

        var guest = CreateGuestEntity(dto);

        await guestRepository.AddAsync(guest, cancellationToken);
    }

    public async Task UpdateGuestAsync(string currentUserId, UpdateGuestDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await updateValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid) throw new ValidationException(validationResult.Errors);

        var guest = await guestRepository.GetByIdAsync(dto.Id, cancellationToken);
        if (guest == null) throw new KeyNotFoundException($"Guest with ID {dto.Id} not found.");

        if (guest.Event == null)
        {
            var eventEntity = await eventRepository.GetByIdAsync(guest.EventId, cancellationToken);
            if (eventEntity == null || eventEntity.OrganizerId != currentUserId)
                throw new UnauthorizedAccessException("Not your event. Only the organizer can update guests.");
        }
        else if (guest.Event.OrganizerId != currentUserId)
        {
            throw new UnauthorizedAccessException("Not your event. Only the organizer can update guests.");
        }

        guest.FirstName = dto.FirstName;
        guest.LastName = dto.LastName;
        guest.Email = dto.Email;

        guest.PhoneNumber = dto.CountryCode + dto.PhoneNumber;

        await guestRepository.UpdateAsync(guest, cancellationToken);
    }

    public async Task RemoveGuestAsync(string userId, string guestId, CancellationToken cancellationToken = default)
    {
        var guest = await guestRepository.GetByIdAsync(guestId, cancellationToken);
        if (guest == null) return;

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
    }

    private static Guest CreateGuestEntity(GuestBaseDto dto)
    {
        return new Guest
        {
            Id = Guid.NewGuid().ToString(),
            EventId = dto.EventId,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            PhoneNumber = dto.CountryCode + dto.PhoneNumber
        };
    }
}