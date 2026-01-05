using EventPlanning.Application.DTOs.Event;
using EventPlanning.Application.Models;
using EventPlanning.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EventPlanning.Web.Models;

public class HomeIndexViewModel
{
    public PagedResult<EventDto> Events { get; init; } = new([], 0, 1, 10);

    public string? SearchTerm { get; init; }
    public EventType? Type { get; init; }
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }
    public SortOrder? SortOrder { get; init; }

    public IEnumerable<SelectListItem> TypeOptions { get; init; } = new List<SelectListItem>();
    public string MinDate { get; init; } = string.Empty;
    public bool HasFilters { get; init; }
}