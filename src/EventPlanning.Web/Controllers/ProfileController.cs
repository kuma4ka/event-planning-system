using EventPlanning.Application.DTOs;
using EventPlanning.Domain.Interfaces;
using EventPlanning.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EventPlanning.Web.Controllers;

[Authorize]
public class ProfileController(
    UserManager<User> userManager,
    IEventRepository eventRepository,
    ILogger<ProfileController> logger) : Controller
{
    private const string TabProfile = "profile";
    private const string TabSecurity = "security";

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var model = await BuildProfileModelAsync(user, cancellationToken);

        ViewBag.PasswordModel = new ChangePasswordDto();
        ViewBag.ActiveTab = TabProfile;

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateInfo(EditProfileDto model, CancellationToken cancellationToken)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        if (!ModelState.IsValid)
        {
            await EnrichModelWithStatsAsync(model, user.Id, cancellationToken);
            model.Email = user.Email;

            ViewBag.ActiveTab = TabProfile;
            ViewBag.PasswordModel = new ChangePasswordDto();
            return View("Index", model);
        }

        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.PhoneNumber = model.PhoneNumber;

        var result = await userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
            logger.LogInformation("User {UserId} updated their profile information.", user.Id);
            TempData["SuccessMessage"] = "Profile details updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        AddErrorsToModelState(result);

        await EnrichModelWithStatsAsync(model, user.Id, cancellationToken);
        model.Email = user.Email;

        ViewBag.ActiveTab = TabProfile;
        ViewBag.PasswordModel = new ChangePasswordDto();
        return View("Index", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto passwordModel,
        CancellationToken cancellationToken)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        if (!ModelState.IsValid)
        {
            var profileModel = await BuildProfileModelAsync(user, cancellationToken);

            ViewBag.ActiveTab = TabSecurity;
            ViewBag.PasswordModel = passwordModel;
            return View("Index", profileModel);
        }

        var result =
            await userManager.ChangePasswordAsync(user, passwordModel.CurrentPassword, passwordModel.NewPassword);

        if (result.Succeeded)
        {
            logger.LogInformation("User {UserId} changed their password.", user.Id);

            await userManager.UpdateSecurityStampAsync(user);

            TempData["SuccessMessage"] = "Password changed successfully!";
            return RedirectToAction(nameof(Index));
        }

        AddErrorsToModelState(result, "PasswordError");

        var model = await BuildProfileModelAsync(user, cancellationToken);

        ViewBag.ActiveTab = TabSecurity;
        ViewBag.PasswordModel = passwordModel;
        return View("Index", model);
    }

    #region Private Helpers

    private async Task<EditProfileDto> BuildProfileModelAsync(User user, CancellationToken token)
    {
        var model = new EditProfileDto
        {
            FirstName = user.FirstName ?? string.Empty,
            LastName = user.LastName ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            Email = user.Email
        };

        await EnrichModelWithStatsAsync(model, user.Id, token);
        return model;
    }

    private async Task EnrichModelWithStatsAsync(EditProfileDto model, string userId, CancellationToken token)
    {
        var organizedEvents = await eventRepository.GetFilteredAsync(
            userId, null, null, null, null, null, null, 1, 1, token);

        model.OrganizedCount = organizedEvents.TotalCount;

        model.JoinedCount = await eventRepository.CountJoinedEventsAsync(userId, token);
    }

    private void AddErrorsToModelState(IdentityResult result, string? keyPrefix = null)
    {
        foreach (var error in result.Errors)
        {
            var key = string.IsNullOrEmpty(keyPrefix) ? string.Empty : keyPrefix;
            ModelState.AddModelError(key, error.Description);
        }
    }

    #endregion
}