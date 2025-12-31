using EventPlanning.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.RateLimiting;

namespace EventPlanning.Web.Controllers;

[Route("newsletter")]
public class NewsletterController(INewsletterService newsletterService) : Controller
{
    [HttpPost("subscribe")]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("newsletter-limit")]
    public async Task<IActionResult> Subscribe([FromForm] string email, [FromForm] string? website, CancellationToken cancellationToken)
    {
        // HONEYPOT: If the hidden 'website' field is filled, it's a bot.
        // Return success so they don't know they failed.
        if (!string.IsNullOrEmpty(website))
        {
            return Json(new { success = true, message = "Successfully subscribed!" });
        }

        try
        {
            await newsletterService.SubscribeAsync(email, cancellationToken);
            return Json(new { success = true, message = "Successfully subscribed!" });
        }
        catch (ArgumentException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
        catch (Exception)
        {
            return Json(new { success = false, message = "An error occurred. Please try again later." });
        }
    }
}
