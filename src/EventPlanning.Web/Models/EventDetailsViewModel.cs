using EventPlanning.Application.DTOs.Event;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EventPlanning.Web.Models;

public class EventDetailsViewModel
{
    public required EventDetailsDto Event { get; init; }
    public string? GoogleMapsApiKey { get; init; }
    public required IEnumerable<SelectListItem> Countries { get; init; }
}
