using EventPlanning.Application.DTOs.Event;
using EventPlanning.Application.Interfaces;
using EventPlanning.Application.Models;
using EventPlanning.Domain.Entities;
using EventPlanning.Domain.Enums;
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
    UserManager<User> userManager,
    IConfiguration configuration,
    ILogger<EventController> logger) : Controller
{
    [HttpGet("details/{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var eventDetails = await eventService.GetEventDetailsAsync(id, cancellationToken);
        if (eventDetails == null) return NotFound();

        ViewBag.GoogleMapsApiKey = configuration["GoogleMaps:ApiKey"];

        return View(eventDetails);
    }

    [HttpGet("create")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        await LoadVenuesToViewBag(cancellationToken);
        return View();
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateEventDto model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await LoadVenuesToViewBag(cancellationToken);
            return View(model);
        }

        var userId = userManager.GetUserId(User);

        try
        {
            var eventId = await eventService.CreateEventAsync(userId!, model, cancellationToken);
            logger.LogInformation("Event created: {EventId} by {User}", eventId, User.Identity?.Name);
            return RedirectToAction("MyEvents", "Event");
        }
        catch (ValidationException ex)
        {
            foreach (var error in ex.Errors)
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);

            await LoadVenuesToViewBag(cancellationToken);
            return View(model);
        }
    }

    [HttpGet("edit/{id:guid}")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);
        var eventDto = await eventService.GetEventByIdAsync(id, cancellationToken);

        if (eventDto == null) return NotFound();
        if (eventDto.OrganizerId != userId) return Forbid();

        await LoadVenuesToViewBag(cancellationToken);

        var updateModel = new UpdateEventDto(
            eventDto.Id,
            eventDto.Name,
            eventDto.Description,
            eventDto.Date,
            eventDto.Type,
            eventDto.VenueId ?? Guid.Empty
        );

        return View(updateModel);
    }

    [HttpPost("edit/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, UpdateEventDto model, CancellationToken cancellationToken)
    {
        if (id != model.Id) return BadRequest("ID mismatch");

        if (!ModelState.IsValid)
        {
            await LoadVenuesToViewBag(cancellationToken);
            return View(model);
        }

        var userId = userManager.GetUserId(User);

        try
        {
            await eventService.UpdateEventAsync(userId!, model, cancellationToken);
            logger.LogInformation("Event updated: {EventId} by {User}", id, User.Identity?.Name);
            return RedirectToAction("MyEvents", "Event");
        }
        catch (ValidationException ex)
        {
            foreach (var error in ex.Errors) ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            await LoadVenuesToViewBag(cancellationToken);
            return View(model);
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            await LoadVenuesToViewBag(cancellationToken);
            return View(model);
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

    [HttpPost("delete/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);
        try
        {
            await eventService.DeleteEventAsync(userId!, id, cancellationToken);
            logger.LogInformation("Event deleted: {EventId} by {User}", id, User.Identity?.Name);
            return RedirectToAction("MyEvents", "Event");
        }
        catch (UnauthorizedAccessException)
        {
            logger.LogWarning("Unauthorized delete attempt: Event {EventId} by {User}", id, User.Identity?.Name);
            return Forbid();
        }
    }

    [HttpPost("join/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Join(Guid id, CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);
        if (userId == null) return RedirectToAction("Login", "Account");

        try
        {
            await eventService.JoinEventAsync(id, userId, cancellationToken);
            logger.LogInformation("User {User} joined event {EventId}", User.Identity?.Name, id);
            TempData["SuccessMessage"] = "You have successfully joined the event!";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error joining event: {EventId} by {User}", id, User.Identity?.Name);
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost("leave/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Leave(Guid id, CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);
        if (userId == null) return RedirectToAction("Login", "Account");

        try
        {
            await eventService.LeaveEventAsync(id, userId, cancellationToken);
            logger.LogInformation("User {User} left event {EventId}", User.Identity?.Name, id);
            TempData["SuccessMessage"] = "You have left the event.";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error leaving event: {EventId} by {User}", id, User.Identity?.Name);
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
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

        // Apply view type defaults
        if (viewType == "past")
        {
            to ??= now;
            sortOrder = string.IsNullOrEmpty(sortOrder) ? "date_desc" : sortOrder;
        }
        else
        {
            from ??= now;
            sortOrder = string.IsNullOrEmpty(sortOrder) ? "date_asc" : sortOrder;
        }

        var searchDto = new EventSearchDto
        {
            SearchTerm = searchTerm,
            Type = type,
            FromDate = from,
            ToDate = to?.Date.AddDays(1).AddTicks(-1),
            PageNumber = page,
            PageSize = 10
        };

        SetMyEventsViewBag(searchTerm, type, from, to, sortOrder, viewType, now);

        PagedResult<EventDto> result;
        try
        {
            result = await eventService.GetEventsAsync(userId!, userId, searchDto, sortOrder, cancellationToken);
        }
        catch (ValidationException ex)
        {
            foreach (var error in ex.Errors) ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            result = new PagedResult<EventDto>([], 0, 1, 10);
        }

        return View(result);
    }

    private void SetMyEventsViewBag(string? searchTerm, EventType? type, DateTime? from, DateTime? to, string? sortOrder, string viewType, DateTime now)
    {
        ViewBag.CurrentViewType = viewType;
        ViewBag.CurrentSearch = searchTerm;
        ViewBag.CurrentType = type;

        ViewBag.CurrentFrom = from.HasValue && Math.Abs((from.Value - now).TotalMinutes) < 1 && viewType == "upcoming"
            ? null
            : from?.ToString("yyyy-MM-dd");

        ViewBag.CurrentTo = to.HasValue && Math.Abs((to.Value - now).TotalMinutes) < 1 && viewType == "past"
            ? null
            : to?.ToString("yyyy-MM-dd");

        ViewBag.CurrentSort = sortOrder;
        ViewBag.DateSortParam = sortOrder == "date_desc" ? "date_asc" : "date_desc";
        ViewBag.NameSortParam = sortOrder == "name_asc" ? "name_desc" : "name_asc";
    }

    private async Task LoadVenuesToViewBag(CancellationToken token)
    {
        var venues = await venueService.GetVenuesAsync(token);

        ViewBag.Venues = venues.Select(v => new SelectListItem
        {
            Value = v.Id.ToString(),
            Text = v.Name
        }).ToList();
    }
}