using EventPlanning.Application.DTOs;
using EventPlanning.Application.DTOs.Venue;

namespace EventPlanning.Application.Interfaces;

public interface IVenueService
{
    Task<List<VenueDto>> GetVenuesAsync(CancellationToken cancellationToken = default);
    Task<VenueDto?> GetVenueByIdAsync(int id, CancellationToken cancellationToken = default);
    Task CreateVenueAsync(string adminId, CreateVenueDto dto, CancellationToken cancellationToken = default);
    Task UpdateVenueAsync(UpdateVenueDto dto, CancellationToken cancellationToken = default);
    Task DeleteVenueAsync(int id, CancellationToken cancellationToken = default);
}