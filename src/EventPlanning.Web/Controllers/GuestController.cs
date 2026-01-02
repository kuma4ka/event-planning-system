using EventPlanning.Application.DTOs.Guest;
using EventPlanning.Application.Interfaces;
using EventPlanning.Domain.Entities;
using EventPlanning.Infrastructure.Identity;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EventPlanning.Web.Controllers;

[Authorize]
[Route("guests")]
public class GuestController(
    IGuestService guestService,
    UserManager<ApplicationUser> userManager,
    ILogger<GuestController> logger) : Controller
{


    [HttpPost("add-manually")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddManually(AddGuestManuallyDto model, CancellationToken cancellationToken)
    {
        var userIdString = userManager.GetUserId(User);
        var userId = Guid.Parse(userIdString!);

        try
        {
            await guestService.AddGuestManuallyAsync(userId, model, cancellationToken);
            TempData["SuccessMessage"] = $"{model.FirstName} has been added to the list.";
        }
        catch (ValidationException ex)
        {
            logger.LogWarning("Validation failed when manually adding guest: {Errors}", string.Join(", ", ex.Errors.Select(e => e.ErrorMessage)));
            TempData["ErrorMessage"] = ex.Errors.FirstOrDefault()?.ErrorMessage ?? "Validation failed.";
        }
        catch (UnauthorizedAccessException)
        {
            logger.LogWarning("User {UserId} unauthorized to add guest to event {EventId}", userId, model.EventId);
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning("Invalid operation adding guest: {Message}", ex.Message);
            TempData["ErrorMessage"] = "Could not add guest due to rule violation (e.g. duplicate or capacity).";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error adding guest manually to event {EventId}", model.EventId);
            TempData["ErrorMessage"] = "An unexpected error occurred.";
        }

        return RedirectToAction("Details", "Event", new { id = model.EventId });
    }

    [HttpPost("edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateGuestDto model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Invalid data provided for guest update.";
            return RedirectToAction("Details", "Event", new { id = model.EventId });
        }

        var userIdString = userManager.GetUserId(User);
        var userId = Guid.Parse(userIdString!);

        try
        {
            await guestService.UpdateGuestAsync(userId, model, cancellationToken);
            TempData["SuccessMessage"] = "Guest information updated successfully.";
        }
        catch (ValidationException ex)
        {
            logger.LogWarning("Validation failed when updating guest {GuestId}: {Errors}", model.Id, string.Join(", ", ex.Errors.Select(e => e.ErrorMessage)));
            TempData["ErrorMessage"] = ex.Errors.FirstOrDefault()?.ErrorMessage ?? "Validation failed.";
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating guest {GuestId}", model.Id);
            TempData["ErrorMessage"] = "An unexpected error occurred.";
        }

        return RedirectToAction("Details", "Event", new { id = model.EventId });
    }

    [HttpPost("remove")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(Guid eventId, Guid guestId, CancellationToken cancellationToken)
    {
        var userIdString = userManager.GetUserId(User);
        var userId = Guid.Parse(userIdString!);
        try
        {
            await guestService.RemoveGuestAsync(userId, guestId, cancellationToken);
            TempData["SuccessMessage"] = "Guest removed from the list.";
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error removing guest {GuestId} from event {EventId}", guestId, eventId);
            TempData["ErrorMessage"] = "An unexpected error occurred.";
        }

        return RedirectToAction("Details", "Event", new { id = eventId });
    }
}