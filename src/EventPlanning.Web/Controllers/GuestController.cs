using EventPlanning.Application.DTOs;
using EventPlanning.Application.Interfaces;
using EventPlanning.Infrastructure.Identity;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EventPlanning.Web.Controllers;

[Authorize]
public class GuestController(
    IGuestService guestService,
    UserManager<User> userManager) : Controller
{
    [HttpGet]
    public IActionResult Create(int eventId)
    {
        return View(new CreateGuestDto(eventId, "", "", "", ""));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateGuestDto model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(model);

        var userId = userManager.GetUserId(User);

        try
        {
            await guestService.AddGuestAsync(userId!, model, cancellationToken);
            return RedirectToAction("Details", "Home", new { id = model.EventId });
        }
        catch (ValidationException ex)
        {
            foreach (var error in ex.Errors)
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            return View(model);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int guestId, int eventId, CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);
        try
        {
            await guestService.RemoveGuestAsync(userId!, guestId, cancellationToken);
            return RedirectToAction("Details", "Home", new { id = eventId });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }
}