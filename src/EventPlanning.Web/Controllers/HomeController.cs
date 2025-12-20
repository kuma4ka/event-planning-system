using EventPlanning.Application.DTOs;
using EventPlanning.Application.Interfaces;
using EventPlanning.Application.Models;
using EventPlanning.Domain.Enums;
using EventPlanning.Infrastructure.Identity;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EventPlanning.Web.Controllers;

[Authorize]
public class HomeController(
    IEventService eventService,
    IVenueService venueService,
    UserManager<User> userManager) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(
        string? searchTerm,
        EventType? type,
        DateTime? from,
        DateTime? to,
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        var userId = userManager.GetUserId(User);
        if (userId == null) return RedirectToAction("Login", "Account");

        var searchDto = new EventSearchDto
        {
            SearchTerm = searchTerm,
            Type = type,
            FromDate = from,
            ToDate = to,
            PageNumber = page,
            PageSize = 9
        };

        var result = await eventService.GetEventsAsync(userId, searchDto, cancellationToken);

        return View(result);
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        await LoadVenuesToViewBag(cancellationToken);
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateEventDto model, CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);
        var modelWithUser = model with { OrganizerId = userId! };

        ModelState.Remove(nameof(model.OrganizerId));

        if (!ModelState.IsValid)
        {
            await LoadVenuesToViewBag(cancellationToken);
            return View(model);
        }

        try
        {
            await eventService.CreateEventAsync(modelWithUser, cancellationToken);
            return RedirectToAction(nameof(Index));
        }
        catch (ValidationException ex)
        {
            foreach (var error in ex.Errors) ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            await LoadVenuesToViewBag(cancellationToken);
            return View(model);
        }
    }

    [HttpGet]
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
            eventDto.VenueId
        );

        return View(updateModel);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(UpdateEventDto model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await LoadVenuesToViewBag(cancellationToken);
            return View(model);
        }

        var userId = userManager.GetUserId(User);

        try
        {
            await eventService.UpdateEventAsync(userId!, model, cancellationToken);
            return RedirectToAction(nameof(Index));
        }
        catch (ValidationException ex)
        {
            foreach (var error in ex.Errors) ModelState.AddModelError(error.PropertyName, error.ErrorMessage);

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

    [HttpPost]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);
        try
        {
            await eventService.DeleteEventAsync(userId!, id, cancellationToken);
            return RedirectToAction(nameof(Index));
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);

        var eventDetails = await eventService.GetEventDetailsAsync(id, cancellationToken);

        if (eventDetails == null) return NotFound();

        return View(eventDetails);
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

    [HttpPost]
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
}