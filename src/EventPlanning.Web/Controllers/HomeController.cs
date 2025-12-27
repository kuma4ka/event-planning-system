using EventPlanning.Application.DTOs;
using EventPlanning.Application.DTOs.Event;
using EventPlanning.Application.Interfaces;
using EventPlanning.Domain.Entities;
using EventPlanning.Domain.Enums;
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

        var effectiveFromDate = (from.HasValue && from.Value > DateTime.UtcNow) ? from.Value : DateTime.UtcNow;

        var searchDto = new EventSearchDto
        {
            SearchTerm = searchTerm,
            Type = type,
            FromDate = effectiveFromDate,
            ToDate = to,
            PageNumber = page,
            PageSize = 9
        };

        var result = await eventService.GetEventsAsync(userId, null, searchDto, null, cancellationToken);

        ViewBag.CurrentSearch = searchTerm;
        ViewBag.CurrentType = type;
        ViewBag.CurrentFrom = from?.ToString("yyyy-MM-dd"); 
        ViewBag.CurrentTo = to?.ToString("yyyy-MM-dd");

        return View(result);
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