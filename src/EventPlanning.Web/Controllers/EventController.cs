using EventPlanning.Application.DTOs.Event;
using EventPlanning.Application.Interfaces;
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
    UserManager<User> userManager) : Controller
{
    [HttpGet("details/{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var eventDetails = await eventService.GetEventDetailsAsync(id, cancellationToken);
        if (eventDetails == null) return NotFound();

        var userId = userManager.GetUserId(User);

        eventDetails.IsOrganizer = userId != null && eventDetails.OrganizerId == userId;
        eventDetails.IsJoined = userId != null && await eventService.IsUserJoinedAsync(id, userId, cancellationToken);

        var organizer = await userManager.FindByIdAsync(eventDetails.OrganizerId);
        ViewBag.OrganizerName = organizer != null ? $"{organizer.FirstName} {organizer.LastName}" : "Unknown Organizer";
        ViewBag.OrganizerEmail = organizer?.Email ?? "";

        return View(eventDetails);
    }

    [HttpGet("create")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        await LoadVenuesToViewBag(cancellationToken);
        return View();
    }

    [HttpPost("create")]
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
            await eventService.CreateEventAsync(userId!, model, cancellationToken);
            return RedirectToAction("Index", "Home");
        }
        catch (ValidationException ex)
        {
            foreach (var error in ex.Errors)
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);

            await LoadVenuesToViewBag(cancellationToken);
            return View(model);
        }
    }

    [HttpGet("edit/{id:int}")]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
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
            eventDto.VenueId ?? 0
        );

        return View(updateModel);
    }

    [HttpPost("edit/{id:int}")]
    public async Task<IActionResult> Edit(int id, UpdateEventDto model, CancellationToken cancellationToken)
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

    [HttpPost("delete/{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);
        try
        {
            await eventService.DeleteEventAsync(userId!, id, cancellationToken);
            return RedirectToAction("Index", "Home");
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpPost("join/{id:int}")]
    public async Task<IActionResult> Join(int id, CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);
        if (userId == null) return RedirectToAction("Login", "Account");

        try
        {
            await eventService.JoinEventAsync(id, userId, cancellationToken);
            TempData["SuccessMessage"] = "You have successfully joined the event!";
        }
        catch (Exception ex)
        {
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

        if (viewType == "past")
        {
            to ??= now;

            if (string.IsNullOrEmpty(sortOrder)) sortOrder = "date_desc";
        }
        else
        {
            from ??= now;

            if (string.IsNullOrEmpty(sortOrder)) sortOrder = "date_asc";
        }

        var searchDto = new EventSearchDto
        {
            SearchTerm = searchTerm,
            Type = type,
            FromDate = from,
            ToDate = to,
            PageNumber = page,
            PageSize = 10
        };

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

        var result = await eventService.GetEventsAsync(userId!, userId, searchDto, sortOrder, cancellationToken);

        return View(result);
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