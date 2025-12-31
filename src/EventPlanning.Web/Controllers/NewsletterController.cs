using EventPlanning.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventPlanning.Web.Controllers;

[Route("newsletter")]
public class NewsletterController(INewsletterService newsletterService) : Controller
{
    [HttpPost("subscribe")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Subscribe([FromForm] string email, CancellationToken cancellationToken)
    {
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
