using EventPlanning.Domain.Entities;

namespace EventPlanning.Domain.Interfaces;

public interface IEventRepository
{
    Task<Event?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<List<Event>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<Event>> GetByOrganizerAsync(string organizerId, CancellationToken cancellationToken = default);
    
    Task AddAsync(Event eventEntity, CancellationToken cancellationToken = default);
    
    Task UpdateAsync(Event eventEntity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Event eventEntity, CancellationToken cancellationToken = default);
}