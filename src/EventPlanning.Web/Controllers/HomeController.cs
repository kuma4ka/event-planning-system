using EventPlanning.Application.DTOs.Event;
using EventPlanning.Application.Interfaces;
using EventPlanning.Application.Models;
using EventPlanning.Domain.Enums;
using EventPlanning.Web.Extensions;
using EventPlanning.Web.Models;
using FluentValidation;
using EventPlanning.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EventPlanning.Web.Controllers;

[Route("")]
[Route("home")]
public class HomeController(
    IEventService eventService,
    UserManager<ApplicationUser> userManager) : Controller
{
    [HttpGet("")]
    [HttpGet("index")]
    [AllowAnonymous]
    public async Task<IActionResult> Index(
        string? searchTerm,
        EventType? type,
        DateTime? from,
        DateTime? to,
        SortOrder? sortOrder,
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        var userIdString = userManager.GetUserId(User);
        var userId = string.IsNullOrEmpty(userIdString) ? Guid.Empty : Guid.Parse(userIdString);
        var now = DateTime.Now;

        var searchDto = new EventSearchDto
        {
            SearchTerm = searchTerm,
            Type = type,
            FromDate = CalculateEffectiveFromDate(from, now),
            ToDate = to?.Date.AddDays(1).AddTicks(-1),
            PageNumber = page,
            PageSize = 9
        };

        var sortString = sortOrder switch
        {
            SortOrder.DateAsc => "date_asc",
            SortOrder.DateDesc => "date_desc",
            SortOrder.NameAsc => "name_asc",
            SortOrder.NameDesc => "name_desc",
            SortOrder.Newest => "newest",
            _ => "date_asc"
        };

        PagedResult<EventDto> result;
        try
        {
            result = await eventService.GetEventsAsync(userId, null, searchDto, sortString, cancellationToken);
        }
        catch (ValidationException ex)
        {
            foreach (var error in ex.Errors) ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            result = new PagedResult<EventDto>([], 0, 1, 9);
        }

        var viewModel = new HomeIndexViewModel
        {
            Events = result,
            SearchTerm = searchTerm,
            Type = type,
            From = from,
            To = to,
            SortOrder = sortOrder,
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
    private static DateTime CalculateEffectiveFromDate(DateTime? from, DateTime now)
    {
        if (!from.HasValue) return now;
        return from.Value.Date < now.Date ? now : from.Value.Date;
    }
}