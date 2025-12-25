using EventPlanning.Application.DTOs.Guest;
using EventPlanning.Application.Interfaces;
using EventPlanning.Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EventPlanning.Web.Controllers;

[Authorize]
[Route("guests")]
public class GuestController(
    IGuestService guestService,
    UserManager<User> userManager) : Controller
{
    [HttpGet("create/{eventId:int}")]
    public IActionResult Create(int eventId)
    {
        return View(new CreateGuestDto(eventId));
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create(CreateGuestDto model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(model);

        var userId = userManager.GetUserId(User);

        try
        {
            await guestService.AddGuestAsync(userId!, model, cancellationToken);
            return RedirectToAction("Details", "Event", new { id = model.EventId });
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

    [HttpPost("add-manually")]
    public async Task<IActionResult> AddManually(AddGuestManuallyDto model, CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);

        try
        {
            await guestService.AddGuestManuallyAsync(userId!, model, cancellationToken);
            TempData["SuccessMessage"] = $"{model.FirstName} has been added to the list.";
        }
        catch (ValidationException ex)
        {
            TempData["ErrorMessage"] = ex.Errors.FirstOrDefault()?.ErrorMessage ?? "Validation failed.";
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction("Details", "Event", new { id = model.EventId });
    }

    [HttpPost("delete")]
    public async Task<IActionResult> Delete(string guestId, int eventId, CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);
        try
        {
            await guestService.RemoveGuestAsync(userId!, guestId, cancellationToken);
            return RedirectToAction("Details", "Event", new { id = eventId });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }
}