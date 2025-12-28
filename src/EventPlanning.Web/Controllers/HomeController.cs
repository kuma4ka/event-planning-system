using EventPlanning.Application.DTOs.Event;
using EventPlanning.Application.Interfaces;
using EventPlanning.Application.Models;
using EventPlanning.Domain.Entities;
using EventPlanning.Domain.Enums;
using EventPlanning.Web.Extensions;
using EventPlanning.Web.Models;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EventPlanning.Web.Controllers;

[Route("")]
[Route("home")]
public class HomeController(
    IEventService eventService,
    UserManager<User> userManager) : Controller
{
    [HttpGet("")]
    [HttpGet("index")]
    [AllowAnonymous]
    public async Task<IActionResult> Index(
        string? searchTerm,
        EventType? type,
        DateTime? from,
        DateTime? to,
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        var userId = userManager.GetUserId(User) ?? string.Empty;
        var now = DateTime.UtcNow;

        var effectiveFromDate = (from.HasValue && from.Value > now) ? from.Value : now;

        DateTime? adjustedToDate = to.HasValue 
            ? to.Value.Date.AddDays(1).AddTicks(-1) 
            : null;

        var searchDto = new EventSearchDto
        {
            SearchTerm = searchTerm,
            Type = type,
            FromDate = effectiveFromDate,
            ToDate = adjustedToDate,
            PageNumber = page,
            PageSize = 9
        };

        PagedResult<EventDto> result;

        try
        {
            result = await eventService.GetEventsAsync(userId, null, searchDto, null, cancellationToken);
        }
        catch (ValidationException ex)
        {
            foreach (var error in ex.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }

            result = new PagedResult<EventDto>(new List<EventDto>(), 0, 1, 9);
        }

        var viewModel = new HomeIndexViewModel
        {
            Events = result,
            
            SearchTerm = searchTerm,
            Type = type,
            From = from,
            To = to,

            TypeOptions = type.ToSelectList("All Categories"),
            MinDate = now.ToString("yyyy-MM-dd"),
            HasFilters = !string.IsNullOrEmpty(searchTerm) || type.HasValue || from.HasValue || to.HasValue
        };

        return View(viewModel);
    }

    [HttpGet("privacy")]
    [AllowAnonymous]
    public IActionResult Privacy()
    {
        return View();
    }
    
    [HttpGet("error")]
    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }
}