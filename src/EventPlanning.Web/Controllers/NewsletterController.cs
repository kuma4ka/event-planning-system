using EventPlanning.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.RateLimiting;

namespace EventPlanning.Web.Controllers;

[Route("newsletter")]
public class NewsletterController(INewsletterService newsletterService, ILogger<NewsletterController> logger) : Controller
{
    [HttpPost("subscribe")]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("newsletter-limit")]
    public async Task<IActionResult> Subscribe([FromForm] string email, [FromForm] string? website, CancellationToken cancellationToken)
    {
        // Honeypot check
        if (!string.IsNullOrEmpty(website))
        {
            logger.LogWarning("Honeypot triggered for newsletter by {Email}", email);
            return Json(new { success = true, message = "Please check your email to confirm your subscription." });
        }

        try
        {
            await newsletterService.SubscribeAsync(email, cancellationToken);
            return Json(new { success = true, message = "Please check your email to confirm your subscription." });
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning("Invalid newsletter subscription attempt: {Message}", ex.Message);
            return Json(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error subscribing to newsletter: {Email}", email);
            return Json(new { success = false, message = "An error occurred. Please try again later." });
        }
    }

    [HttpGet("confirm")]
    public async Task<IActionResult> Confirm(string email, string token, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
        {
            return BadRequest("Invalid confirmation link.");
        }

        var result = await newsletterService.ConfirmSubscriptionAsync(email, token, cancellationToken);

        if (result)
        {
            return View("SubscriptionConfirmed");
        }
        else
        {
            logger.LogWarning("Failed newsletter confirmation for {Email}", email);
            return BadRequest("Invalid or expired confirmation token.");
        }
    }
}
