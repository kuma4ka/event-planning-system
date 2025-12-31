using EventPlanning.Domain.Entities;

namespace EventPlanning.Domain.Interfaces;

public interface IGuestRepository
{
    Task<Guest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Guest guest, CancellationToken cancellationToken = default);
    Task UpdateAsync(Guest guest, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guest guest, CancellationToken cancellationToken = default);
}