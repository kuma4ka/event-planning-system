using EventPlanning.Application.DTOs.Venue;
using EventPlanning.Application.Interfaces;
using EventPlanning.Application.Models;
using EventPlanning.Domain.Entities;
using EventPlanning.Domain.Interfaces;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace EventPlanning.Application.Services;

public class VenueService(
    IVenueRepository venueRepository,
    IEventRepository eventRepository,
    IImageService imageService,
    IValidator<CreateVenueDto> createValidator,
    IValidator<UpdateVenueDto> updateValidator,
    IUserRepository userRepository,
    Microsoft.Extensions.Logging.ILogger<VenueService> logger
) : IVenueService
{
    public async Task<List<VenueDto>> GetVenuesAsync(CancellationToken cancellationToken = default)
    {
        var venues = await venueRepository.GetAllAsync(cancellationToken);

        return venues.Select(v => new VenueDto(
            v.Id,
            v.Name,
            v.Address,
            v.Capacity,
            v.Description,
            v.ImageUrl
        )).ToList();
    }

    public async Task<PagedResult<VenueDto>> GetVenuesPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await venueRepository.GetPagedAsync(pageNumber, pageSize, cancellationToken);

        var dtos = items.Select(v => new VenueDto(
            v.Id,
            v.Name,
            v.Address,
            v.Capacity,
            v.Description,
            v.ImageUrl
        )).ToList();

        return new PagedResult<VenueDto>(dtos, totalCount, pageNumber, pageSize);
    }

    public async Task<VenueDto?> GetVenueByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var venue = await venueRepository.GetByIdAsync(id, cancellationToken);
        if (venue == null) return null;

        return new VenueDto(
            venue.Id,
            venue.Name,
            venue.Address,
            venue.Capacity,
            venue.Description,
            venue.ImageUrl
        );
    }

    public async Task CreateVenueAsync(Guid adminId, CreateVenueDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await createValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid) throw new ValidationException(validationResult.Errors);

        string? imageUrl = null;

        if (dto.ImageFile != null)
            imageUrl = await imageService.UploadImageAsync(dto.ImageFile, "venues", cancellationToken);

        var user = await userRepository.GetByIdentityIdAsync(adminId.ToString(), cancellationToken);
        if (user == null) throw new InvalidOperationException("User profile not found");

        var venue = new Venue(
            dto.Name,
            dto.Address,
            dto.Capacity,
            user.Id,
            dto.Description,
            imageUrl
        );

        await venueRepository.AddAsync(venue, cancellationToken);
    }

    public async Task UpdateVenueAsync(Guid userId, UpdateVenueDto dto, CancellationToken cancellationToken = default)
    {
        var validationResult = await updateValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid) throw new ValidationException(validationResult.Errors);

        var venue = await venueRepository.GetByIdAsync(dto.Id, cancellationToken);
        if (venue == null) throw new KeyNotFoundException($"Venue {dto.Id} not found");

        var user = await userRepository.GetByIdentityIdAsync(userId.ToString(), cancellationToken);
        if (user == null) throw new InvalidOperationException("User not found");

        if (user.Role != Domain.Enums.UserRole.Admin && venue.OrganizerId != user.Id)
        {
            throw new UnauthorizedAccessException("User is not authorized to update this venue.");
        }

        string? newImageUrl = venue.ImageUrl;

        if (dto.ImageFile != null)
        {
            if (!string.IsNullOrEmpty(venue.ImageUrl)) imageService.DeleteImage(venue.ImageUrl);

            newImageUrl = await imageService.UploadImageAsync(dto.ImageFile, "venues", cancellationToken);
        }

        venue.UpdateDetails(
            dto.Name,
            dto.Address,
            dto.Capacity,
            dto.Description,
            newImageUrl
        );

        await venueRepository.UpdateAsync(venue, cancellationToken);
        logger.LogInformation("Venue {VenueId} updated by {UserId}. Changes: {@Changes}", dto.Id, userId, dto);
    }

    public async Task DeleteVenueAsync(Guid userId, Guid id, CancellationToken cancellationToken = default)
    {
        var venue = await venueRepository.GetByIdAsync(id, cancellationToken);
        if (venue == null) return;
        
        var user = await userRepository.GetByIdentityIdAsync(userId.ToString(), cancellationToken);
        if (user == null) throw new InvalidOperationException("User not found");

        if (user.Role != Domain.Enums.UserRole.Admin && venue.OrganizerId != user.Id)
        {
            throw new UnauthorizedAccessException("User is not authorized to delete this venue.");
        }

        var hasEvents = await eventRepository.HasEventsAtVenueAsync(id, cancellationToken);
        if (hasEvents)
        {
            throw new InvalidOperationException("Cannot delete venue because it is associated with existing events.");
        }

        if (!string.IsNullOrEmpty(venue.ImageUrl)) imageService.DeleteImage(venue.ImageUrl);

        venue.MarkDeleted();
        await venueRepository.UpdateAsync(venue, cancellationToken);
    }
}