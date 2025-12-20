using EventPlanning.Application.DTOs;
using EventPlanning.Application.Interfaces;
using EventPlanning.Domain.Entities;
using EventPlanning.Domain.Interfaces;
using FluentValidation;

namespace EventPlanning.Application.Services;

public class GuestService(
    IGuestRepository guestRepository,
    IEventRepository eventRepository,
    IValidator<CreateGuestDto> validator,
    IValidator<AddGuestManuallyDto> manualAddValidator) : IGuestService
{
    public async Task AddGuestAsync(string userId, CreateGuestDto dto, CancellationToken cancellationToken = default)
    {
        var validationResult = await validator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid) throw new ValidationException(validationResult.Errors);

        var eventEntity = await eventRepository.GetByIdAsync(dto.EventId, cancellationToken);

        if (eventEntity == null) throw new KeyNotFoundException("Event not found");
        if (eventEntity.OrganizerId != userId) throw new UnauthorizedAccessException("Not your event");

        var guest = new Guest
        {
            Id = Guid.NewGuid().ToString(),
            EventId = dto.EventId,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber
        };

        await guestRepository.AddAsync(guest, cancellationToken);
    }

    public async Task RemoveGuestAsync(string userId, string guestId, CancellationToken cancellationToken = default)
    {
        var guest = await guestRepository.GetByIdAsync(guestId, cancellationToken);
        if (guest == null) return;

        if (guest.Event?.OrganizerId != userId)
            throw new UnauthorizedAccessException("Not your event");

        await guestRepository.DeleteAsync(guest, cancellationToken);
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

        if (eventEntity.Venue != null && eventEntity.Guests.Count >= eventEntity.Venue.Capacity)
            throw new InvalidOperationException("Venue is fully booked.");

        var guest = new Guest
        {
            Id = Guid.NewGuid().ToString(),
            EventId = dto.EventId,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber
        };

        await guestRepository.AddAsync(guest, cancellationToken);
    }
}