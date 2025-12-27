using EventPlanning.Application.DTOs.Venue;
using EventPlanning.Application.Interfaces;
using EventPlanning.Domain.Entities;
using EventPlanning.Domain.Interfaces;
using FluentValidation;

namespace EventPlanning.Application.Services;

public class VenueService(
    IVenueRepository venueRepository,
    IImageService imageService,
    IValidator<CreateVenueDto> createValidator,
    IValidator<UpdateVenueDto> updateValidator
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
            v.Description
        )).ToList();
    }

    public async Task<UpdateVenueDto?> GetVenueByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var venue = await venueRepository.GetByIdAsync(id, cancellationToken);
        if (venue == null) return null;

        return new UpdateVenueDto(
            venue.Id,
            venue.Name,
            venue.Address,
            venue.Capacity,
            venue.Description,
            venue.ImageUrl,
            null
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

        var venue = new Venue
        {
            Name = dto.Name,
            Address = dto.Address,
            Capacity = dto.Capacity,
            Description = dto.Description,
            ImageUrl = imageUrl,
            OrganizerId = adminId
        };

        await venueRepository.AddAsync(venue, cancellationToken);
    }

    public async Task UpdateVenueAsync(UpdateVenueDto dto, CancellationToken cancellationToken = default)
    {
        var validationResult = await updateValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid) throw new ValidationException(validationResult.Errors);

        var venue = await venueRepository.GetByIdAsync(dto.Id, cancellationToken);
        if (venue == null) throw new KeyNotFoundException($"Venue {dto.Id} not found");

        if (dto.ImageFile != null)
        {
            if (!string.IsNullOrEmpty(venue.ImageUrl)) imageService.DeleteImage(venue.ImageUrl);

            venue.ImageUrl = await imageService.UploadImageAsync(dto.ImageFile, "venues", cancellationToken);
        }

        venue.Name = dto.Name;
        venue.Address = dto.Address;
        venue.Capacity = dto.Capacity;
        venue.Description = dto.Description;

        await venueRepository.UpdateAsync(venue, cancellationToken);
    }

    public async Task DeleteVenueAsync(int id, CancellationToken cancellationToken = default)
    {
        var venue = await venueRepository.GetByIdAsync(id, cancellationToken);
        if (venue == null) return;

        if (!string.IsNullOrEmpty(venue.ImageUrl)) imageService.DeleteImage(venue.ImageUrl);

        await venueRepository.DeleteAsync(venue, cancellationToken);
    }
}