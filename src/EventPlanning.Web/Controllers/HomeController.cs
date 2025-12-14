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
        var modelWithUser = model with { OrganizerId = "test-user-1" };

        ModelState.Remove(nameof(model.OrganizerId));

        if (!ModelState.IsValid)
            return View(model);

        try
        {
            await eventService.CreateEventAsync(modelWithUser, cancellationToken);
            return RedirectToAction(nameof(Index));
        }
        catch (FluentValidation.ValidationException ex)
        {
            foreach (var error in ex.Errors) ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            return View(model);
        }
    }
}