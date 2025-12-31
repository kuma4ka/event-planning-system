using EventPlanning.Application.DTOs.Venue;
using EventPlanning.Application.Models;

namespace EventPlanning.Application.Interfaces;

public interface IVenueService
{
    Task<List<VenueDto>> GetVenuesAsync(CancellationToken cancellationToken = default);
    Task<PagedResult<VenueDto>> GetVenuesPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<VenueDto?> GetVenueByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task CreateVenueAsync(string adminId, CreateVenueDto dto, CancellationToken cancellationToken = default);
    Task UpdateVenueAsync(UpdateVenueDto dto, CancellationToken cancellationToken = default);
    Task DeleteVenueAsync(Guid id, CancellationToken cancellationToken = default);
}