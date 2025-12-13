using EventPlanning.Domain.Entities;

namespace EventPlanning.Domain.Interfaces;

public interface IVenueRepository
{
    Task<Venue?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<List<Venue>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Venue venue, CancellationToken cancellationToken = default);
    Task UpdateAsync(Venue venue, CancellationToken cancellationToken = default);
    Task DeleteAsync(Venue venue, CancellationToken cancellationToken = default);
}