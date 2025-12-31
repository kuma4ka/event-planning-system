using EventPlanning.Domain.Entities;

namespace EventPlanning.Domain.Interfaces;

public interface IVenueRepository
{
    Task<Venue?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Venue>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<(List<Venue> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task AddAsync(Venue venue, CancellationToken cancellationToken = default);
    Task UpdateAsync(Venue venue, CancellationToken cancellationToken = default);
    Task DeleteAsync(Venue venue, CancellationToken cancellationToken = default);
}