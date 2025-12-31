using EventPlanning.Application.DTOs.Event;
using EventPlanning.Application.DTOs.Guest;
using EventPlanning.Application.Constants;
using EventPlanning.Application.Interfaces;
using EventPlanning.Application.Models;
using EventPlanning.Domain.Entities;
using EventPlanning.Domain.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;

namespace EventPlanning.Application.Services;

public class EventService(
    IEventRepository eventRepository,
    IValidator<CreateEventDto> createValidator,
    IValidator<UpdateEventDto> updateValidator,
    IValidator<EventSearchDto> searchValidator,
    IUserRepository userRepository,
    IHttpContextAccessor httpContextAccessor,
    IMemoryCache cache) : IEventService
{
    private const string EventCacheKeyPrefix = "event_details_";

    public async Task<PagedResult<EventDto>> GetEventsAsync(
        string userId,
        string? organizerIdFilter,
        EventSearchDto searchDto,
        string? sortOrder,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await searchValidator.ValidateAsync(searchDto, cancellationToken);
        if (!validationResult.IsValid) throw new ValidationException(validationResult.Errors);

        var fromDate = searchDto.FromDate;

        var pagedEvents = await eventRepository.GetFilteredAsync(
            organizerIdFilter,
            userId,
            searchDto.SearchTerm,
            fromDate,
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

    public async Task<EventDetailsDto?> GetEventDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var userId = httpContextAccessor.HttpContext?.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        var publicCacheKey = $"{EventCacheKeyPrefix}{id}_public";
        var organizerCacheKey = $"{EventCacheKeyPrefix}{id}_organizer";

        if (cache.TryGetValue(publicCacheKey, out EventDetailsDto? publicCachedEvent) && publicCachedEvent != null)
        {
            if (publicCachedEvent.OrganizerId != userId)
            {
                return publicCachedEvent;
            }
        }

        if (cache.TryGetValue(organizerCacheKey, out EventDetailsDto? organizerCachedEvent) && organizerCachedEvent != null)
        {
            if (organizerCachedEvent.OrganizerId == userId)
            {
                return organizerCachedEvent;
            }
        }

        var eventEntity = await eventRepository.GetDetailsByIdAsync(id, cancellationToken);
        if (eventEntity == null) return null;

        var isOrganizer = !string.IsNullOrEmpty(userId) && eventEntity.OrganizerId == userId;

        var guestsDto = eventEntity.Guests.Select(g =>
        {
            // Mask data if not organizer
            if (!isOrganizer)
            {
                return new GuestDto(
                    g.Id,
                    g.FirstName,
                    g.LastName,
                    "REDACTED", // Masked Email
                    "",   // Masked CountryCode
                    ""   // Masked Phone
                );
            }

            var localNumber = g.PhoneNumber != null && !string.IsNullOrEmpty(g.CountryCode) && g.PhoneNumber.Value.StartsWith(g.CountryCode)
                ? g.PhoneNumber.Value.Substring(g.CountryCode.Length)
                : (g.PhoneNumber?.Value ?? "");

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



        var cacheOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(10))
            .SetAbsoluteExpiration(TimeSpan.FromHours(1));

        if (isOrganizer)
        {
            cache.Set(organizerCacheKey, eventDetails, cacheOptions);
        }
        else
        {
            cache.Set(publicCacheKey, eventDetails, cacheOptions);
        }

        return eventDetails;
    }

    public async Task<Guid> CreateEventAsync(string userId, CreateEventDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await createValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid) throw new ValidationException(validationResult.Errors);

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

        if (eventEntity.OrganizerId != userId) throw new UnauthorizedAccessException("Not your event");

        if (eventEntity.Date < DateTime.UtcNow)
            throw new InvalidOperationException("Cannot edit an event that has already ended.");

        eventEntity.UpdateDetails(
            dto.Name,
            dto.Description,
            dto.Date,
            dto.Type,
            dto.VenueId == Guid.Empty ? null : dto.VenueId
        );

        await eventRepository.UpdateAsync(eventEntity, cancellationToken);

        InvalidateEventCache(dto.Id);
    }

    public async Task DeleteEventAsync(string userId, Guid eventId, CancellationToken cancellationToken = default)
    {
        var eventEntity = await eventRepository.GetByIdAsync(eventId, cancellationToken);
        if (eventEntity == null) return;

        if (eventEntity.OrganizerId != userId)
            throw new UnauthorizedAccessException("Not your event");

        await eventRepository.DeleteAsync(eventEntity, cancellationToken);

        InvalidateEventCache(eventId);
    }

    public async Task JoinEventAsync(Guid eventId, string userId, CancellationToken cancellationToken = default)
    {
        var eventEntity = await eventRepository.GetByIdAsync(eventId, cancellationToken);
        if (eventEntity == null) throw new KeyNotFoundException($"Event {eventId} not found");

        if (eventEntity.Date < DateTime.UtcNow)
            throw new InvalidOperationException("Cannot join an event that has already ended.");

        if (eventEntity.OrganizerId == userId)
            throw new InvalidOperationException("You cannot join your own event as a guest.");

        var isJoined = await eventRepository.IsUserJoinedAsync(eventId, userId, cancellationToken);
        if (isJoined)
        {
            throw new InvalidOperationException("You are already registered for this event.");
        }

        var success = await eventRepository.TryJoinEventAsync(eventId, userId, cancellationToken);
        if (!success)
        {
            throw new InvalidOperationException("Sorry, this event is fully booked or unavailable.");
        }

        InvalidateEventCache(eventId);
    }

    public async Task LeaveEventAsync(Guid eventId, string userId, CancellationToken cancellationToken = default)
    {
        await eventRepository.RemoveGuestAsync(eventId, userId, cancellationToken);

        InvalidateEventCache(eventId);
    }

    public async Task<bool> IsUserJoinedAsync(Guid eventId, string userId, CancellationToken cancellationToken = default)
    {
        return await eventRepository.IsUserJoinedAsync(eventId, userId, cancellationToken);
    }

    private void InvalidateEventCache(Guid eventId)
    {
        cache.Remove($"{EventCacheKeyPrefix}{eventId}_public");
        cache.Remove($"{EventCacheKeyPrefix}{eventId}_organizer");
    }

    private static (string CountryCode, string PhoneNumber) ParsePhoneNumber(string? fullPhoneNumber)
    {
        if (string.IsNullOrEmpty(fullPhoneNumber)) return (CountryConstants.DefaultCode, string.Empty);

        var country = CountryConstants.SupportedCountries
            .OrderByDescending(c => c.Code.Length)
            .FirstOrDefault(c => fullPhoneNumber.StartsWith(c.Code));

        if (country != null)
        {
            var localNumber = fullPhoneNumber.Substring(country.Code.Length);
            return (country.Code, localNumber);
        }

        return (CountryConstants.DefaultCode, fullPhoneNumber);
    }
}