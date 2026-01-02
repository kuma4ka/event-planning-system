using EventPlanning.Application.Interfaces;
using EventPlanning.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EventPlanning.Web.Controllers;

[Authorize]
[Route("events")]
public class EventParticipationController(
    IEventParticipationService participationService,
    UserManager<ApplicationUser> userManager,
    ILogger<EventParticipationController> logger) : Controller
{
    [HttpPost("join/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Join(Guid id, CancellationToken cancellationToken)
    {
        var paramsUserId = userManager.GetUserId(User);
        if (paramsUserId == null) return RedirectToAction("Login", "Account");
        var userId = Guid.Parse(paramsUserId);

        try
        {
            await participationService.JoinEventAsync(id, userId, cancellationToken);
            logger.LogInformation("User {UserId} joined event {EventId}", userId, id);
            TempData["SuccessMessage"] = "You have successfully joined the event!";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error joining event: {EventId} by {UserId}", id, userId);
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction("Details", "Event", new { id });
    }

    [HttpGet("join/{id:guid}")]
    public IActionResult JoinPrompt(Guid id)
    {
        TempData["InfoMessage"] = "Please confirm your action by clicking the button again.";
        return RedirectToAction("Details", "Event", new { id });
    }

    [HttpPost("leave/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Leave(Guid id, CancellationToken cancellationToken)
    {
        var paramsUserId = userManager.GetUserId(User);
        if (paramsUserId == null) return RedirectToAction("Login", "Account");
        var userId = Guid.Parse(paramsUserId);

        try
        {
            await participationService.LeaveEventAsync(id, userId, cancellationToken);
            logger.LogInformation("User {UserId} left event {EventId}", userId, id);
            TempData["SuccessMessage"] = "You have left the event.";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error leaving event: {EventId} by {UserId}", id, userId);
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction("Details", "Event", new { id });
    }

    [HttpGet("leave/{id:guid}")]
    public IActionResult LeavePrompt(Guid id)
    {
         TempData["InfoMessage"] = "Please confirm your action by clicking the button again.";
         return RedirectToAction("Details", "Event", new { id });
    }
}
