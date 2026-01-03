using EventPlanning.Application.DTOs.Event;
using EventPlanning.Application.DTOs.Guest;
using EventPlanning.Application.Interfaces;
using EventPlanning.Application.Models;
using EventPlanning.Domain.Entities;
using EventPlanning.Domain.Interfaces;
using FluentValidation;
using Mapster;
using Microsoft.Extensions.Logging;

namespace EventPlanning.Application.Services;

public class EventService(
    IEventRepository eventRepository,
    IGuestRepository guestRepository,
    IValidator<CreateEventDto> createValidator,
    IValidator<UpdateEventDto> updateValidator,
    IValidator<EventSearchDto> searchValidator,
    IUserRepository userRepository,
    ICountryService countryService,
    ILogger<EventService> logger) : IEventService
{
    public async Task<PagedResult<EventDto>> GetEventsAsync(
        Guid userId,
        Guid? organizerIdFilter,
        EventSearchDto searchDto,
        string? sortOrder,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await searchValidator.ValidateAsync(searchDto, cancellationToken);
        if (!validationResult.IsValid) throw new ValidationException(validationResult.Errors);

        var user = await userRepository.GetByIdentityIdAsync(userId.ToString(), cancellationToken);
        var domainUserId = user?.Id ?? userId;

        if (organizerIdFilter.HasValue && organizerIdFilter.Value == userId)
        {
            organizerIdFilter = domainUserId;
        }

        var pagedEvents = await eventRepository.GetFilteredAsync(
            organizerIdFilter,
            domainUserId,
            searchDto.SearchTerm,
            searchDto.FromDate,
            searchDto.ToDate,
            searchDto.Type,
            sortOrder,
            searchDto.PageNumber,
            searchDto.PageSize,
            cancellationToken
        );

        var eventDtos = pagedEvents.Items.Adapt<List<EventDto>>();

        return new PagedResult<EventDto>(
            eventDtos, pagedEvents.TotalCount, pagedEvents.PageNumber, pagedEvents.PageSize
        );
    }

    public async Task<EventDto?> GetEventByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var e = await eventRepository.GetByIdAsync(id, cancellationToken);
        if (e == null) return null;

        return e.Adapt<EventDto>();
    }

    public async Task<EventDetailsDto?> GetEventDetailsAsync(Guid id, Guid? userId, CancellationToken cancellationToken = default)
    {
        var eventEntity = await eventRepository.GetDetailsByIdAsync(id, cancellationToken);
        if (eventEntity == null) return null;

        Guid? domainUserId = null;
        if (userId.HasValue)
        {
            var user = await userRepository.GetByIdentityIdAsync(userId.Value.ToString(), cancellationToken);
            domainUserId = user?.Id;
        }

        var isOrganizer = domainUserId.HasValue && eventEntity.OrganizerId == domainUserId.Value;

        var eventDetails = eventEntity.Adapt<EventDetailsDto>();

        var guestsDto = eventEntity.Guests.Select(g => MapGuestDto(g, isOrganizer)).ToList();

        var isJoined = domainUserId.HasValue && await guestRepository.IsUserJoinedAsync(eventEntity.Id, domainUserId.Value, cancellationToken);

        eventDetails = eventDetails with 
        { 
            Guests = guestsDto,
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

    public async Task<Guid> CreateEventAsync(Guid userId, CreateEventDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await createValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            logger.LogWarning("Event creation failed validation: {Errors}", string.Join(", ", validationResult.Errors));
            throw new ValidationException(validationResult.Errors);
        }

        var user = await userRepository.GetByIdentityIdAsync(userId.ToString(), cancellationToken);
        if (user == null) throw new InvalidOperationException("User profile not found");

        var eventEntity = new Event(
            dto.Name,
            dto.Description,
            dto.Date,
            dto.Type,
            user.Id,
            dto.VenueId == Guid.Empty ? null : dto.VenueId
        );

        return await eventRepository.AddAsync(eventEntity, cancellationToken);
    }

    public async Task<EventDto> GetEventForEditAsync(Guid eventId, Guid userId, CancellationToken cancellationToken = default)
    {
        var eventEntity = await eventRepository.GetByIdAsync(eventId, cancellationToken);
        if (eventEntity == null) throw new KeyNotFoundException($"Event {eventId} not found");

        await ValidateOrganizerAccessAsync(eventEntity.OrganizerId, userId, eventId, cancellationToken);

        return eventEntity.Adapt<EventDto>();
    }

    public async Task UpdateEventAsync(Guid userId, UpdateEventDto dto, CancellationToken cancellationToken = default)
    {
        var validationResult = await updateValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid) throw new ValidationException(validationResult.Errors);

        var eventEntity = await eventRepository.GetByIdAsync(dto.Id, cancellationToken);
        if (eventEntity == null) throw new KeyNotFoundException($"Event {dto.Id} not found");

        await ValidateOrganizerAccessAsync(eventEntity.OrganizerId, userId, dto.Id, cancellationToken);

        eventEntity.UpdateDetails(
            dto.Name,
            dto.Description,
            dto.Date,
            dto.Type,
            dto.VenueId == Guid.Empty ? null : dto.VenueId
        );

        await eventRepository.UpdateAsync(eventEntity, cancellationToken);
    }

    public async Task DeleteEventAsync(Guid userId, Guid eventId, CancellationToken cancellationToken = default)
    {
        var eventEntity = await eventRepository.GetByIdAsync(eventId, cancellationToken);
        if (eventEntity == null) return;

        await ValidateOrganizerAccessAsync(eventEntity.OrganizerId, userId, eventId, cancellationToken);

        await eventRepository.DeleteAsync(eventEntity, cancellationToken);
    }

    private async Task ValidateOrganizerAccessAsync(Guid eventOrganizerId, Guid userId, Guid eventId, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdentityIdAsync(userId.ToString(), cancellationToken);
        if (user == null) throw new UnauthorizedAccessException("User not found");

        if (eventOrganizerId != user.Id)
        {
            logger.LogWarning("Unauthorized event access attempt by {UserId} on event {EventId}", userId, eventId);
            throw new UnauthorizedAccessException("Not your event");
        }
    }

    private GuestDto MapGuestDto(Guest g, bool isOrganizer)
    {
        if (isOrganizer)
        {
            var (_, localNumber) = countryService.ParsePhoneNumber(g.PhoneNumber?.Value);
            return new GuestDto
            {
                Id = g.Id,
                FirstName = g.FirstName,
                LastName = g.LastName,
                Email = g.Email.Value,
                CountryCode = g.CountryCode,
                PhoneNumber = localNumber
            };
        }

        return new GuestDto
        {
            Id = g.Id,
            FirstName = g.FirstName,
            LastName = g.LastName,
            Email = "REDACTED",
            CountryCode = "",
            PhoneNumber = ""
        };
    }
}