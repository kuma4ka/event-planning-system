using EventPlanning.Application.DTOs;

namespace EventPlanning.Application.Interfaces;

public interface IVenueService
{
    Task<List<VenueDto>> GetVenuesAsync(CancellationToken cancellationToken = default);
}