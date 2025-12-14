using EventPlanning.Application.DTOs;
using EventPlanning.Application.Interfaces;
using EventPlanning.Domain.Interfaces;

namespace EventPlanning.Application.Services;

public class VenueService(IVenueRepository venueRepository) : IVenueService
{
    public async Task<List<VenueDto>> GetVenuesAsync(CancellationToken cancellationToken = default)
    {
        var venues = await venueRepository.GetAllAsync(cancellationToken);
        
        return venues.Select(v => new VenueDto(v.Id, v.Name)).ToList();
    }
}