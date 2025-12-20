using EventPlanning.Domain.Entities;
using EventPlanning.Domain.Enums;
using EventPlanning.Domain.Models;

public interface IEventRepository
{
    Task<Event?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task AddAsync(Event eventEntity, CancellationToken cancellationToken = default);
    Task UpdateAsync(Event eventEntity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Event eventEntity, CancellationToken cancellationToken = default);
    
    Task<PagedList<Event>> GetFilteredAsync(
        string userId, 
        string? searchTerm, 
        DateTime? from, 
        DateTime? to, 
        EventType? type, 
        int pageNumber, 
        int pageSize, 
        CancellationToken cancellationToken = default);
}