using EventPlanning.Application.DTOs;
using EventPlanning.Application.Interfaces;
using EventPlanning.Application.Models;
using EventPlanning.Domain.Entities;
using EventPlanning.Domain.Enums;
using EventPlanning.Domain.Interfaces;
using FluentValidation;

namespace EventPlanning.Application.Services;

public class EventService(
    IEventRepository eventRepository,
    IValidator<CreateEventDto> createValidator,
    IValidator<UpdateEventDto> updateValidator) : IEventService
{
    public async Task<PagedResult<EventDto>> GetEventsAsync(string userId, EventSearchDto searchDto,
        CancellationToken cancellationToken = default)
    {
        var pagedEvents = await eventRepository.GetFilteredAsync(
            null,
            userId,
            searchDto.SearchTerm,
            searchDto.FromDate,
            searchDto.ToDate,
            searchDto.Type,
            searchDto.PageNumber,
            searchDto.PageSize,
            cancellationToken
        );

        var eventDtos = pagedEvents.Items.Select(e => new EventDto(
            e.Id, e.Name, e.Description ?? string.Empty, e.Date, e.Type, e.OrganizerId, e.Venue?.Name ?? "TBD",
            e.VenueId
        )).ToList();

        return new PagedResult<EventDto>(
            eventDtos,
            pagedEvents.TotalCount,
            pagedEvents.PageNumber,
            pagedEvents.PageSize
        );
    }

    public async Task<EventDto?> GetEventByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var e = await eventRepository.GetByIdAsync(id, cancellationToken);
        if (e == null) return null;

        return new EventDto(
            e.Id,
            e.Name,
            e.Description ?? string.Empty,
            e.Date,
            e.Type,
            e.OrganizerId,
            e.Venue?.Name ?? "TBD",
            e.VenueId
        );
    }

    public async Task<EventDetailsDto?> GetEventDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        var eventEntity = await eventRepository.GetByIdAsync(id, cancellationToken);
        if (eventEntity == null) return null;

        return new EventDetailsDto(
            eventEntity.Id,
            eventEntity.Name,
            eventEntity.Description ?? string.Empty,
            eventEntity.Date,
            eventEntity.Type.ToString(),
            eventEntity.OrganizerId,
            eventEntity.Venue?.Name ?? "Online / TBD",
            eventEntity.Venue?.ImageUrl,
            eventEntity.Venue?.Capacity ?? 0,
            eventEntity.IsPrivate,
            eventEntity.Guests.Select(g => new GuestDto(
                g.Id,
                $"{g.FirstName} {g.LastName}",
                g.Email,
                g.PhoneNumber
            )).ToList()
        );
    }

    public async Task<int> CreateEventAsync(string userId, CreateEventDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await createValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid) throw new ValidationException(validationResult.Errors);

        var eventEntity = new Event
        {
            Name = dto.Name,
            Description = dto.Description,
            Date = dto.Date,
            Type = Enum.Parse<EventType>(dto.Type),
            VenueId = dto.VenueId,
            OrganizerId = userId,
            IsPrivate = dto.IsPrivate,
            CreatedAt = DateTime.UtcNow
        };

        return await eventRepository.AddAsync(eventEntity, cancellationToken);
    }

    public async Task UpdateEventAsync(string userId, UpdateEventDto dto, CancellationToken cancellationToken = default)
    {
        var validationResult = await updateValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid) throw new ValidationException(validationResult.Errors);

        var eventEntity = await eventRepository.GetByIdAsync(dto.Id, cancellationToken);
        if (eventEntity == null) throw new KeyNotFoundException($"Event {dto.Id} not found");

        if (eventEntity.OrganizerId != userId) throw new UnauthorizedAccessException("Not your event");

        eventEntity.Name = dto.Name;
        eventEntity.Description = dto.Description;
        eventEntity.Date = dto.Date;
        eventEntity.Type = dto.Type;
        eventEntity.VenueId = dto.VenueId;

        await eventRepository.UpdateAsync(eventEntity, cancellationToken);
    }

    public async Task DeleteEventAsync(string userId, int eventId, CancellationToken cancellationToken = default)
    {
        var eventEntity = await eventRepository.GetByIdAsync(eventId, cancellationToken);
        if (eventEntity == null) return;

        if (eventEntity.OrganizerId != userId)
            throw new UnauthorizedAccessException("Not your event");

        await eventRepository.DeleteAsync(eventEntity, cancellationToken);
    }

    public async Task JoinEventAsync(int eventId, string userId, CancellationToken cancellationToken = default)
    {
        var eventEntity = await eventRepository.GetByIdAsync(eventId, cancellationToken);
        if (eventEntity == null) throw new KeyNotFoundException($"Event {eventId} not found");

        if (eventEntity.OrganizerId == userId)
            throw new InvalidOperationException("You cannot join your own event as a guest.");

        if (eventEntity.Venue != null && eventEntity.Guests.Count >= eventEntity.Venue.Capacity)
            throw new InvalidOperationException("Sorry, this event is fully booked.");

        if (eventEntity.Guests.Any(g => g.Id == userId))
            throw new InvalidOperationException("You are already registered for this event.");

        await eventRepository.AddGuestAsync(eventId, userId, cancellationToken);
    }

    public async Task LeaveEventAsync(int eventId, string userId, CancellationToken cancellationToken = default)
    {
        await eventRepository.RemoveGuestAsync(eventId, userId, cancellationToken);
    }
}