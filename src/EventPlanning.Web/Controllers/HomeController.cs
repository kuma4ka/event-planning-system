using EventPlanning.Application.Interfaces;
using EventPlanning.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace EventPlanning.Web.Controllers;

public class HomeController(IEventService eventService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var events = await eventService.GetAllEventsAsync(cancellationToken);
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
            foreach (var error in ex.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
            return View(model);
        }
    }
}