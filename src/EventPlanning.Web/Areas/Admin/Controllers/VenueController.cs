using EventPlanning.Application.DTOs.Venue;
using EventPlanning.Application.Interfaces;
using EventPlanning.Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EventPlanning.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class VenueController(
    IVenueService venueService,
    UserManager<User> userManager) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var venues = await venueService.GetVenuesAsync(cancellationToken);
        return View(venues);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateVenueDto model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(model);

        var adminId = userManager.GetUserId(User);

        try
        {
            await venueService.CreateVenueAsync(adminId!, model, cancellationToken);
            return RedirectToAction(nameof(Index));
        }
        catch (ValidationException ex)
        {
            foreach (var error in ex.Errors)
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var venueDto = await venueService.GetVenueByIdAsync(id, cancellationToken);
        
        if (venueDto == null) return NotFound();

        return View(venueDto);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(UpdateVenueDto model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(model);

        try
        {
            await venueService.UpdateVenueAsync(model, cancellationToken);
            return RedirectToAction(nameof(Index));
        }
        catch (ValidationException ex)
        {
            foreach (var error in ex.Errors)
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            return View(model);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await venueService.DeleteVenueAsync(id, cancellationToken);
        return RedirectToAction(nameof(Index));
    }
}