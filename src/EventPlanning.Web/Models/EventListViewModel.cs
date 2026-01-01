using EventPlanning.Application.DTOs.Event;
using EventPlanning.Application.Models;
using EventPlanning.Domain.Enums;

namespace EventPlanning.Web.Models;

public class EventListViewModel
{
    public PagedResult<EventDto> Events { get; set; } = new([], 0, 1, 10);
    
    // Filter State
    public string? SearchTerm { get; set; }
    public EventType? Type { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string ViewType { get; set; } = "upcoming";
    public string? SortOrder { get; set; }
}
