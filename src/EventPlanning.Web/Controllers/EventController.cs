using EventPlanning.Application.DTOs.Event;
using EventPlanning.Application.Interfaces;
using EventPlanning.Application.Models;
using EventPlanning.Domain.Entities;
using EventPlanning.Domain.Enums;
using EventPlanning.Infrastructure.Identity;
using EventPlanning.Web.Extensions;
using EventPlanning.Web.Models;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EventPlanning.Web.Controllers;

[Authorize]
[Route("events")]
public class EventController(
    IEventService eventService,
    IVenueService venueService,
    UserManager<ApplicationUser> userManager,
    IConfiguration configuration,
    ICountryService countryService,
    ILogger<EventController> logger) : Controller
{
    [HttpGet("details/{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);
        var eventDetails = await eventService.GetEventDetailsAsync(id, userId, cancellationToken);
        if (eventDetails == null) return NotFound();

        ViewBag.GoogleMapsApiKey = configuration["GoogleMaps:ApiKey"];
        ViewBag.Countries = new SelectList(countryService.GetSupportedCountries(), "Code", "DisplayValue");

        return View(eventDetails);
    }

    [HttpGet("create")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var venues = await GetVenuesList(cancellationToken);
        return View(EventFormViewModel.ForCreate(null, venues));
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EventFormViewModel viewModel, CancellationToken cancellationToken)
    {
        if (viewModel.CreateDto == null) return BadRequest();
        var model = viewModel.CreateDto;

        if (!ModelState.IsValid)
        {
            viewModel.Venues = await GetVenuesList(cancellationToken);
            return View(viewModel);
        }

        var userId = userManager.GetUserId(User);

        try
        {
            var eventId = await eventService.CreateEventAsync(userId!, model, cancellationToken);
            logger.LogInformation("Event created: {EventId} by {User}", eventId, User.Identity?.Name);
            return RedirectToAction(nameof(MyEvents));
        }
        catch (ValidationException ex)
        {
            ModelState.AddValidationErrors(ex);
            viewModel.Venues = await GetVenuesList(cancellationToken);
            return View(viewModel);
        }
    }

    [HttpGet("edit/{id:guid}")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);
        var eventDto = await eventService.GetEventByIdAsync(id, cancellationToken);

        if (eventDto == null) return NotFound();
        if (eventDto.OrganizerId != userId) return Forbid();

        var updateModel = new UpdateEventDto(
            eventDto.Id,
            eventDto.Name,
            eventDto.Description,
            eventDto.Date,
            eventDto.Type,
            eventDto.VenueId ?? Guid.Empty
        );

        var venues = await GetVenuesList(cancellationToken);
        return View(EventFormViewModel.ForEdit(updateModel, venues));
    }

    [HttpPost("edit/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, EventFormViewModel viewModel, CancellationToken cancellationToken)
    {
        if (viewModel.UpdateDto == null || id != viewModel.UpdateDto.Id) return BadRequest("ID mismatch");
        var model = viewModel.UpdateDto;

        if (!ModelState.IsValid)
        {
            viewModel.Venues = await GetVenuesList(cancellationToken);
            return View(viewModel);
        }

        var userId = userManager.GetUserId(User);

        try
        {
            await eventService.UpdateEventAsync(userId!, model, cancellationToken);
            logger.LogInformation("Event updated: {EventId} by {User}", id, User.Identity?.Name);
            return RedirectToAction(nameof(MyEvents));
        }
        catch (ValidationException ex)
        {
            ModelState.AddValidationErrors(ex);
            viewModel.Venues = await GetVenuesList(cancellationToken);
            return View(viewModel);
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            viewModel.Venues = await GetVenuesList(cancellationToken);
            return View(viewModel);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("delete/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);
        try
        {
            await eventService.DeleteEventAsync(userId!, id, cancellationToken);
            logger.LogInformation("Event deleted: {EventId} by {User}", id, User.Identity?.Name);
            return RedirectToAction(nameof(MyEvents));
        }
        catch (UnauthorizedAccessException)
        {
            logger.LogWarning("Unauthorized delete attempt: Event {EventId} by {User}", id, User.Identity?.Name);
            return Forbid();
        }
    }

    [HttpGet("my-events")]
    public async Task<IActionResult> MyEvents(
        string? searchTerm,
        EventType? type,
        DateTime? from,
        DateTime? to,
        string? sortOrder,
        string viewType = "upcoming",
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        var userId = userManager.GetUserId(User);
        var now = DateTime.Now;

        var searchFrom = from;
        var searchTo = to;
        var searchSort = sortOrder;

        if (viewType == "past")
        {
            searchTo ??= now;
            searchSort = string.IsNullOrEmpty(sortOrder) ? "date_desc" : sortOrder;
        }
        else
        {
            searchFrom ??= now;
            searchSort = string.IsNullOrEmpty(sortOrder) ? "date_asc" : sortOrder;
        }

        var searchDto = new EventSearchDto
        {
            SearchTerm = searchTerm,
            Type = type,
            FromDate = searchFrom,
            ToDate = searchTo?.Date.AddDays(1).AddTicks(-1),
            PageNumber = page,
            PageSize = 10
        };

        PagedResult<EventDto> result;
        try
        {
            result = await eventService.GetEventsAsync(userId!, userId, searchDto, searchSort, cancellationToken);
        }
        catch (ValidationException ex)
        {
            ModelState.AddValidationErrors(ex);
            result = new PagedResult<EventDto>([], 0, 1, 10);
        }

        var viewModel = new EventListViewModel
        {
            Events = result,
            SearchTerm = searchTerm,
            Type = type,
            FromDate = from,
            ToDate = to,
            ViewType = viewType,
            SortOrder = searchSort
        };

        return View(viewModel);
    }
    
    private async Task<List<SelectListItem>> GetVenuesList(CancellationToken token)
    {
        var venues = await venueService.GetVenuesAsync(token);

        return venues.Select(v => new SelectListItem
        {
            Value = v.Id.ToString(),
            Text = v.Name
        }).ToList();
    }
}