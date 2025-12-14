using FluentValidation;
using EventPlanning.Application.Interfaces;
using EventPlanning.Application.DTOs;
using EventPlanning.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EventPlanning.Web.Controllers;

[Authorize]
public class HomeController(
    IEventService eventService,
    UserManager<User> userManager) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);

        if (userId == null) return RedirectToAction("Login", "Account");

        var events = await eventService.GetEventsByUserIdAsync(userId, cancellationToken);

        return View(events);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateEventDto model, CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);
        
        var modelWithUser = model with { OrganizerId = userId! };
        
        ModelState.Remove(nameof(model.OrganizerId));

        if (!ModelState.IsValid)
            return View(model);

        try 
        {
            await eventService.CreateEventAsync(modelWithUser, cancellationToken);
            return RedirectToAction(nameof(Index));
        }
        catch (ValidationException ex)
        {
            foreach (var error in ex.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
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

        var updateModel = new UpdateEventDto(
            eventDto.Id,
            eventDto.Name,
            eventDto.Description,
            eventDto.Date,
            eventDto.Type,
            null
        );

        return View(updateModel);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(UpdateEventDto model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(model);

        var userId = userManager.GetUserId(User);

        try
        {
            await eventService.UpdateEventAsync(userId!, model, cancellationToken);
            return RedirectToAction(nameof(Index));
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
}