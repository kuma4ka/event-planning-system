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
    IImageService imageService,
    IValidator<CreateVenueDto> createValidator,
    IValidator<UpdateVenueDto> updateValidator,
    ILogger<VenueService> logger
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

    public async Task CreateVenueAsync(string adminId, CreateVenueDto dto,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await createValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid) throw new ValidationException(validationResult.Errors);

        string? imageUrl = null;

        if (dto.ImageFile != null)
            imageUrl = await imageService.UploadImageAsync(dto.ImageFile, "venues", cancellationToken);

        var venue = new Venue(
            dto.Name,
            dto.Address,
            dto.Capacity,
            adminId,
            dto.Description,
            imageUrl
        );

        await venueRepository.AddAsync(venue, cancellationToken);
    }

    public async Task UpdateVenueAsync(UpdateVenueDto dto, CancellationToken cancellationToken = default)
    {
        var validationResult = await updateValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid) throw new ValidationException(validationResult.Errors);

        var venue = await venueRepository.GetByIdAsync(dto.Id, cancellationToken);
        if (venue == null) throw new KeyNotFoundException($"Venue {dto.Id} not found");

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
            newImageUrl // Pass the new (or existing) ImageUrl
        );

        await venueRepository.UpdateAsync(venue, cancellationToken);
    }

    public async Task DeleteVenueAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var venue = await venueRepository.GetByIdAsync(id, cancellationToken);
        if (venue == null) return;

        if (!string.IsNullOrEmpty(venue.ImageUrl)) imageService.DeleteImage(venue.ImageUrl);

        await venueRepository.DeleteAsync(venue, cancellationToken);
    }
}