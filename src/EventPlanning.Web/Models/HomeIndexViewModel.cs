using EventPlanning.Application.DTOs.Event;
using EventPlanning.Application.Models;
using EventPlanning.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EventPlanning.Web.Models;

public class HomeIndexViewModel
{
    public PagedResult<EventDto> Events { get; set; } = new(new List<EventDto>(), 0, 1, 10);

    public string? SearchTerm { get; set; }
    public EventType? Type { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }

    public IEnumerable<SelectListItem> TypeOptions { get; set; } = new List<SelectListItem>();
    public string MinDate { get; set; } = string.Empty;
    public bool HasFilters { get; set; }
}