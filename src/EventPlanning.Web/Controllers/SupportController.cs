using Microsoft.AspNetCore.Mvc;

namespace EventPlanning.Web.Controllers;

public class SupportController : Controller
{
    [HttpGet("support")]
    public IActionResult Index()
    {
        return View("HelpCenter");
    }

    [HttpGet("support/guidelines")]
    public IActionResult Guidelines()
    {
        return View();
    }

    [HttpGet("support/contact")]
    public IActionResult Contact()
    {
        return View("ContactUs");
    }

    [HttpPost("support/contact")]
    [ValidateAntiForgeryToken]
    public IActionResult Contact(object model)
    {
        TempData["SuccessMessage"] = "Your message has been sent. We'll get back to you soon!";
        return RedirectToAction(nameof(Contact));
    }

    [HttpGet("support/privacy")]
    public IActionResult Privacy()
    {
        return View("PrivacyPolicy");
    }
}
