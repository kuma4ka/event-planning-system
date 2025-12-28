using EventPlanning.Application.DTOs.Event;
using EventPlanning.Application.DTOs.Guest;
using EventPlanning.Application.Constants;
using EventPlanning.Application.Interfaces;
using EventPlanning.Application.Models;
using EventPlanning.Domain.Entities;
using EventPlanning.Domain.Interfaces;
using FluentValidation;

namespace EventPlanning.Application.Services;

public class EventService(
    IEventRepository eventRepository,
    IValidator<CreateEventDto> createValidator,
    IValidator<UpdateEventDto> updateValidator,
    IValidator<EventSearchDto> searchValidator,
    IIdentityService identityService) : IEventService
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
            e.VenueId,
            e.Venue?.ImageUrl
        );
    }

    public async Task<EventDetailsDto?> GetEventDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        var eventEntity = await eventRepository.GetByIdAsync(id, cancellationToken);
        if (eventEntity == null) return null;

        var guestsDto = eventEntity.Guests.Select(g =>
        {
            var (countryCode, localNumber) = ParsePhoneNumber(g.PhoneNumber);

            return new GuestDto(
                g.Id,
                g.FirstName,
                g.LastName,
                g.Email,
                countryCode,
                localNumber
            );
        }).ToList();

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
            eventEntity.Venue?.ImageUrl
        )
        {
            IsOrganizer = false,
            IsJoined = false
        };

        return eventDetails;
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
            Type = dto.Type,
            VenueId = dto.VenueId == 0 ? null : dto.VenueId,
            OrganizerId = userId,
            IsPrivate = false,
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

        if (eventEntity.Date < DateTime.Now)
            throw new InvalidOperationException("Cannot edit an event that has already ended.");

        eventEntity.Name = dto.Name;
        eventEntity.Description = dto.Description;
        eventEntity.Date = dto.Date;
        eventEntity.Type = dto.Type;
        eventEntity.VenueId = dto.VenueId == 0 ? null : dto.VenueId;

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

        if (eventEntity == null)
            throw new KeyNotFoundException($"Event {eventId} not found");

        if (eventEntity.Date < DateTime.Now)
            throw new InvalidOperationException("Cannot join an event that has already ended.");

        if (eventEntity.OrganizerId == userId)
            throw new InvalidOperationException("You cannot join your own event as a guest.");

        if (eventEntity.Venue is { Capacity: > 0 } &&
            eventEntity.Guests.Count >= eventEntity.Venue.Capacity)
            throw new InvalidOperationException("Sorry, this event is fully booked.");

        var user = await identityService.GetUserByIdAsync(userId);
        if (user == null) throw new KeyNotFoundException("User not found.");

        if (eventEntity.Guests.Any(g => g.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("You are already registered for this event.");

        if (!string.IsNullOrEmpty(user.PhoneNumber))
        {
            var isPhoneTaken = eventEntity.Guests.Any(g =>
                !string.IsNullOrEmpty(g.PhoneNumber) &&
                g.PhoneNumber == user.PhoneNumber);

            if (isPhoneTaken)
                throw new InvalidOperationException(
                    "A participant with this phone number is already joined to this event.");
        }

        await eventRepository.AddGuestAsync(eventId, userId, cancellationToken);
    }

    public async Task LeaveEventAsync(int eventId, string userId, CancellationToken cancellationToken = default)
    {
        await eventRepository.RemoveGuestAsync(eventId, userId, cancellationToken);
    }

    public async Task<bool> IsUserJoinedAsync(int eventId, string userId, CancellationToken cancellationToken = default)
    {
        return await eventRepository.IsUserJoinedAsync(eventId, userId, cancellationToken);
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