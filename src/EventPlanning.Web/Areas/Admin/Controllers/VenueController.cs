using EventPlanning.Application.DTOs.Venue;
using EventPlanning.Application.Interfaces;
using EventPlanning.Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EventPlanning.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Route("[area]/[controller]")]
[Authorize(Roles = "Admin")]
public class VenueController(
    IVenueService venueService,
    UserManager<User> userManager,
    ILogger<VenueController> logger) : Controller
{
    [HttpGet("")]
    public async Task<IActionResult> Index(int page = 1, CancellationToken cancellationToken = default)
    {
        var venues = await venueService.GetVenuesPagedAsync(page, 9, cancellationToken);
        return View(venues);
    }

    [HttpGet("create")]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
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

    [HttpGet("edit/{id:guid}")]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var venueDto = await venueService.GetVenueByIdAsync(id, cancellationToken);

        if (venueDto == null) return NotFound();

        var updateModel = new UpdateVenueDto(
            venueDto.Id,
            venueDto.Name,
            venueDto.Address,
            venueDto.Capacity,
            venueDto.Description,
            venueDto.ImageUrl,
            null
        );

        return View(updateModel);
    }

    [HttpPost("edit/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, UpdateVenueDto model, CancellationToken cancellationToken)
    {
        if (id != model.Id)
        {
            return BadRequest("ID mismatch");
        }

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

    [HttpPost("delete/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await venueService.DeleteVenueAsync(id, cancellationToken);

        return RedirectToAction(nameof(Index));
    }
}