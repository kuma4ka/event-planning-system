namespace EventPlanning.Web.Models.Shared;

using Domain.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

public class EventFilterViewModel
{
    public string? SearchTerm { get; init; }
    public EventType? Type { get; init; }
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }
    public IEnumerable<SelectListItem> TypeOptions { get; init; } = new List<SelectListItem>();
    public string MinDate { get; init; } = DateTime.Now.ToString("yyyy-MM-dd");
    public bool HasFilters { get; init; }

    public string ControllerName { get; init; } = "Home";
    public string ActionName { get; init; } = "Index";
    public string? ViewType { get; init; }
    public SortOrder? SortOrder { get; init; }
    public bool ShowSearch { get; init; }
    public bool IsHeroOverlay { get; init; }
}
