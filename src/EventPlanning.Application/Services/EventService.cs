using EventPlanning.Application.DTOs;
using EventPlanning.Application.Interfaces;
using EventPlanning.Domain.Entities;
using EventPlanning.Domain.Interfaces;
using FluentValidation;

namespace EventPlanning.Application.Services;

public class EventService(
    IEventRepository eventRepository,
    IValidator<CreateEventDto> validator) : IEventService
{
    public async Task<List<EventDto>> GetAllEventsAsync(CancellationToken cancellationToken = default)
    {
        var events = await eventRepository.GetAllAsync(cancellationToken);

        return events.Select(e => new EventDto(
            e.Id,
            e.Name,
            e.Description ?? string.Empty,
            e.Date,
            e.Type,
            e.OrganizerId,
            e.Venue?.Name ?? "TBD",
            e.Guests.Count
        )).ToList();
    }

    public async Task<EventDto?> GetEventByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var eventEntity = await eventRepository.GetByIdAsync(id, cancellationToken);
        if (eventEntity == null) return null;

        return new EventDto(
            eventEntity.Id,
            eventEntity.Name,
            eventEntity.Description ?? string.Empty,
            eventEntity.Date,
            eventEntity.Type,
            eventEntity.OrganizerId,
            eventEntity.Venue?.Name ?? "TBD",
            eventEntity.Guests.Count
        );
    }

    public async Task CreateEventAsync(CreateEventDto dto, CancellationToken cancellationToken = default)
    {
        var validationResult = await validator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var newEvent = new Event
        {
            Name = dto.Name,
            Description = dto.Description,
            Date = dto.Date,
            Type = dto.Type,
            VenueId = dto.VenueId,
            OrganizerId = dto.OrganizerId,
            CreatedAt = DateTime.UtcNow
        };

        await eventRepository.AddAsync(newEvent, cancellationToken);
    }
    
    public async Task<List<EventDto>> GetEventsByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        var events = await eventRepository.GetByOrganizerAsync(userId, cancellationToken);

        return events.Select(e => new EventDto(
            e.Id,
            e.Name,
            e.Description ?? string.Empty,
            e.Date,
            e.Type,
            e.OrganizerId,
            e.Venue?.Name ?? "TBD",
            e.Guests.Count
        )).ToList();
    }
}