using EventPlanning.Application.DTOs;
using EventPlanning.Application.Interfaces;
using EventPlanning.Domain.Entities;
using EventPlanning.Domain.Interfaces;
using FluentValidation;

namespace EventPlanning.Application.Services;

public class GuestService(
    IGuestRepository guestRepository,
    IEventRepository eventRepository,
    IValidator<CreateGuestDto> validator) : IGuestService
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
            EventId = dto.EventId,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber
        };

        await guestRepository.AddAsync(guest, cancellationToken);
    }

    public async Task RemoveGuestAsync(string userId, int guestId, CancellationToken cancellationToken = default)
    {
        var guest = await guestRepository.GetByIdAsync(guestId, cancellationToken);
        if (guest == null) return;

        if (guest.Event?.OrganizerId != userId)
            throw new UnauthorizedAccessException("Not your event");

        await guestRepository.DeleteAsync(guest, cancellationToken);
    }
}