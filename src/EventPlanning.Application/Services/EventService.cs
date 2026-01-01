using EventPlanning.Application.DTOs.Event;
using EventPlanning.Application.DTOs.Guest;
using EventPlanning.Application.Constants;
using EventPlanning.Application.Interfaces;
using EventPlanning.Application.Models;
using EventPlanning.Domain.Entities;
using EventPlanning.Domain.Interfaces;
using EventPlanning.Domain.ValueObjects;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace EventPlanning.Application.Services;

public class EventService(
    IEventRepository eventRepository,
    IValidator<CreateEventDto> createValidator,
    IValidator<UpdateEventDto> updateValidator,
    IValidator<EventSearchDto> searchValidator,
    IUserRepository userRepository,
    ICountryService countryService,
    ILogger<EventService> logger) : IEventService
{
    public async Task<PagedResult<EventDto>> GetEventsAsync(
        string userId,
        string? organizerIdFilter,
        EventSearchDto searchDto,
        string? sortOrder,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await searchValidator.ValidateAsync(searchDto, cancellationToken);
        if (!validationResult.IsValid) throw new ValidationException(validationResult.Errors);

        var pagedEvents = await eventRepository.GetFilteredAsync(
            organizerIdFilter,
            userId,
            searchDto.SearchTerm,
            searchDto.FromDate,
            searchDto.ToDate,
            searchDto.Type,
            sortOrder,
            searchDto.PageNumber,
            searchDto.PageSize,
            cancellationToken
        );

        var eventDtos = pagedEvents.Items.Select(e => new EventDto(
            e.Id,
            e.Name,
            e.Description ?? string.Empty,
            e.Date,
            e.Type,
            e.OrganizerId,
            e.Venue?.Name ?? "TBD",
            e.VenueId,
            e.Venue?.ImageUrl
        )).ToList();

        return new PagedResult<EventDto>(
            eventDtos, pagedEvents.TotalCount, pagedEvents.PageNumber, pagedEvents.PageSize
        );
    }

    public async Task<EventDto?> GetEventByIdAsync(Guid id, CancellationToken cancellationToken = default)
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
            e.VenueId,
            e.Venue?.ImageUrl
        );
    }

    public async Task<EventDetailsDto?> GetEventDetailsAsync(Guid id, string? userId, CancellationToken cancellationToken = default)
    {
        var eventEntity = await eventRepository.GetDetailsByIdAsync(id, cancellationToken);
        if (eventEntity == null) return null;

        var isOrganizer = !string.IsNullOrEmpty(userId) && eventEntity.OrganizerId == userId;

        var guestsDto = eventEntity.Guests.Select(g =>
        {
            if (!isOrganizer)
            {
                return new GuestDto(
                    g.Id,
                    g.FirstName,
                    g.LastName,
                    "REDACTED",
                    "",
                    ""
                );
            }

            var (_, localNumber) = countryService.ParsePhoneNumber(g.PhoneNumber?.Value);

            return new GuestDto(
                g.Id,
                g.FirstName,
                g.LastName,
                g.Email,
                g.CountryCode,
                localNumber
            );
        }).ToList();

        var isJoined = !string.IsNullOrEmpty(userId) && await eventRepository.IsUserJoinedAsync(eventEntity.Id, userId, cancellationToken);

        var eventDetails = new EventDetailsDto(
            eventEntity.Venue?.Capacity ?? 0,
            eventEntity.IsPrivate,
            guestsDto,
            eventEntity.Id,
            eventEntity.Name,
            eventEntity.Description ?? string.Empty,
            eventEntity.Date,
            eventEntity.Type,
            eventEntity.OrganizerId,
            eventEntity.Venue?.Name ?? "TBD",
            eventEntity.VenueId,
            eventEntity.Venue?.ImageUrl,
            eventEntity.Venue?.Address
        )
        {
            IsOrganizer = isOrganizer,
            IsJoined = isJoined
        };
        var organizer = await userRepository.GetByIdAsync(eventEntity.OrganizerId, cancellationToken);
        if (organizer != null)
        {
            eventDetails = eventDetails with
            {
                OrganizerName = $"{organizer.FirstName} {organizer.LastName}",
                OrganizerEmail = organizer.Email ?? ""
            };
        }

        return eventDetails;
    }

    public async Task<Guid> CreateEventAsync(string userId, CreateEventDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await createValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            logger.LogWarning("Event creation failed validation: {Errors}", string.Join(", ", validationResult.Errors));
            throw new ValidationException(validationResult.Errors);
        }

        var eventEntity = new Event(
            dto.Name,
            dto.Description,
            dto.Date,
            dto.Type,
            userId,
            dto.VenueId == Guid.Empty ? null : dto.VenueId
        );

        return await eventRepository.AddAsync(eventEntity, cancellationToken);
    }

    public async Task UpdateEventAsync(string userId, UpdateEventDto dto, CancellationToken cancellationToken = default)
    {
        var validationResult = await updateValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid) throw new ValidationException(validationResult.Errors);

        var eventEntity = await eventRepository.GetByIdAsync(dto.Id, cancellationToken);
        if (eventEntity == null) throw new KeyNotFoundException($"Event {dto.Id} not found");

        if (eventEntity.OrganizerId != userId)
        {
             logger.LogWarning("Unauthorized event update attempt by {UserId} on event {EventId}", userId, dto.Id);
             throw new UnauthorizedAccessException("Not your event");
        }



        eventEntity.UpdateDetails(
            dto.Name,
            dto.Description,
            dto.Date,
            dto.Type,
            dto.VenueId == Guid.Empty ? null : dto.VenueId
        );

        await eventRepository.UpdateAsync(eventEntity, cancellationToken);
    }

    public async Task DeleteEventAsync(string userId, Guid eventId, CancellationToken cancellationToken = default)
    {
        var eventEntity = await eventRepository.GetByIdAsync(eventId, cancellationToken);
        if (eventEntity == null) return;

        if (eventEntity.OrganizerId != userId)
        {
            logger.LogWarning("Unauthorized event delete attempt by {UserId} on event {EventId}", userId, eventId);
            throw new UnauthorizedAccessException("Not your event");
        }

        await eventRepository.DeleteAsync(eventEntity, cancellationToken);
    }


}