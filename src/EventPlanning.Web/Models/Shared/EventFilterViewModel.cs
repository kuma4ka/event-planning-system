namespace EventPlanning.Web.Models.Shared;

using EventPlanning.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

public class EventFilterViewModel
{
    public string? SearchTerm { get; set; }
    public EventType? Type { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public IEnumerable<SelectListItem> TypeOptions { get; set; } = new List<SelectListItem>();
    public string MinDate { get; set; } = DateTime.Now.ToString("yyyy-MM-dd");
    public bool HasFilters { get; set; }

    // Additional fields for MyEvents context
    public string ControllerName { get; set; } = "Home";
    public string ActionName { get; set; } = "Index";
    public string? ViewType { get; set; } // "upcoming" or "past"
    public string? SortOrder { get; set; }
    public bool ShowSearch { get; set; } = false;
    public bool IsHeroOverlay { get; set; } = false;
}
