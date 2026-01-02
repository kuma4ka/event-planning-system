using EventPlanning.Application.DTOs.Event;
using EventPlanning.Application.Models;
using EventPlanning.Domain.Enums;

namespace EventPlanning.Web.Models;

public class EventListViewModel
{
    public PagedResult<EventDto> Events { get; init; } = new([], 0, 1, 10);
    
    public string? SearchTerm { get; init; }
    public EventType? Type { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public string ViewType { get; init; } = "upcoming";
    public SortOrder? SortOrder { get; init; }
}
