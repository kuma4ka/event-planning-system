using EventPlanning.Application.DTOs.Guest;
using EventPlanning.Application.Interfaces;
using EventPlanning.Application.Constants;
using EventPlanning.Domain.Entities;
using EventPlanning.Domain.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace EventPlanning.Application.Services;

public class GuestService(
    IGuestRepository guestRepository,
    IEventRepository eventRepository,
    IValidator<AddGuestManuallyDto> manualAddValidator,
    IValidator<UpdateGuestDto> updateValidator,
    IUserRepository userRepository,
    IMemoryCache cache,
    IUnitOfWork unitOfWork) : IGuestService
{



    public async Task AddGuestManuallyAsync(Guid currentUserId, AddGuestManuallyDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await manualAddValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);


        var eventEntity = await eventRepository.GetByIdAsync(dto.EventId, cancellationToken);
        if (eventEntity == null) throw new KeyNotFoundException("Event not found");

        await ValidateOrganizerAccessAsync(eventEntity.OrganizerId, currentUserId, cancellationToken);


        if (eventEntity.Date < DateTime.Now)
            throw new InvalidOperationException("Cannot add guests to an event that has already ended.");

        await unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            await CheckCapacityAsync(eventEntity, dto.EventId, cancellationToken);
            await CheckUniqueEmailAsync(dto.EventId, dto.Email, null, cancellationToken);

            var fullPhoneNumber = dto.CountryCode + dto.PhoneNumber.Replace(" ", "").Replace("-", "");
            if (!string.IsNullOrEmpty(dto.PhoneNumber))
            {
                await CheckUniquePhoneAsync(dto.EventId, fullPhoneNumber, null, cancellationToken);
            }

            var guest = CreateGuestEntity(dto);

            await guestRepository.AddAsync(guest, cancellationToken);
        }, System.Data.IsolationLevel.Serializable, cancellationToken);

        InvalidateEventCache(dto.EventId);
    }

    public async Task UpdateGuestAsync(Guid currentUserId, UpdateGuestDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await updateValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid) throw new ValidationException(validationResult.Errors);

        var guest = await guestRepository.GetByIdAsync(dto.Id, cancellationToken);
        if (guest == null) throw new KeyNotFoundException($"Guest with ID {dto.Id} not found.");

        var eventEntity = guest.Event ?? await eventRepository.GetByIdAsync(guest.EventId, cancellationToken);
        if (eventEntity == null) throw new KeyNotFoundException("Event not found.");

        await ValidateOrganizerAccessAsync(eventEntity.OrganizerId, currentUserId, cancellationToken);


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

    public async Task RemoveGuestAsync(Guid userId, Guid guestId, CancellationToken cancellationToken = default)
    {
        var guest = await guestRepository.GetByIdAsync(guestId, cancellationToken);
        if (guest == null) return;

        Guid eventId = guest.EventId;

        var user = await userRepository.GetByIdentityIdAsync(userId.ToString(), cancellationToken);
        if (user == null) return;

        if (guest.Event == null)
        {
            var eventEntity = await eventRepository.GetByIdAsync(guest.EventId, cancellationToken);
            if (eventEntity == null) return;
            await ValidateOrganizerAccessAsync(eventEntity.OrganizerId, userId, cancellationToken);
        }
        else 
        {
             await ValidateOrganizerAccessAsync(guest.Event.OrganizerId, userId, cancellationToken);
        }


        await guestRepository.DeleteAsync(guest, cancellationToken);

        InvalidateEventCache(eventId);
    }

    private void InvalidateEventCache(Guid eventId)
    {
        cache.Remove(CacheKeyGenerator.GetEventKeyPublic(eventId));
        cache.Remove(CacheKeyGenerator.GetEventKeyOrganizer(eventId));
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
        var guestCount = await guestRepository.CountGuestsAtEventAsync(eventId, cancellationToken);
        eventEntity.CanAddGuest(guestCount);
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

    private async Task ValidateOrganizerAccessAsync(Guid eventOrganizerId, Guid userId, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdentityIdAsync(userId.ToString(), cancellationToken);
        if (user == null) throw new UnauthorizedAccessException("User not found");

        if (eventOrganizerId != user.Id)
        {
            throw new UnauthorizedAccessException("Not your event");
        }
    }
}