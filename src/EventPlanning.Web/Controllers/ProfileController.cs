using EventPlanning.Application.Constants;
using EventPlanning.Domain.Constants;
using EventPlanning.Application.DTOs.Profile;
using EventPlanning.Application.Interfaces;
using EventPlanning.Domain.Entities;
using EventPlanning.Infrastructure.Identity;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EventPlanning.Web.Controllers;

[Authorize]
[Route("profile")]
public class ProfileController(
    IProfileService profileService,
    UserManager<ApplicationUser> userManager,
    ILogger<ProfileController> logger) : Controller
{
    private const string TabProfile = "profile";
    private const string TabSecurity = "security";

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);
        if (userId == null) return RedirectToAction("Login", "Account");

        var model = await profileService.GetProfileAsync(userId, cancellationToken);

        LoadSharedViewData();
        
        ViewBag.PasswordModel = new ChangePasswordDto();
        ViewBag.ActiveTab = TabProfile;

        return View(model);
    }

    [HttpPost("update-info")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateInfo(EditProfileDto model, CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);
        if (userId == null) return RedirectToAction("Login", "Account");

        try
        {
            await profileService.UpdateProfileAsync(userId, model, cancellationToken);

            logger.LogInformation("User {UserId} updated their profile information.", userId);
            TempData["SuccessMessage"] = "Profile details updated successfully!";
            
            return RedirectToAction(nameof(Index));
        }
        catch (ValidationException ex)
        {
            foreach (var error in ex.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
        }

        var freshProfileData = await profileService.GetProfileAsync(userId, cancellationToken);
        
        model.Email = freshProfileData.Email;
        model.OrganizedCount = freshProfileData.OrganizedCount;
        model.JoinedCount = freshProfileData.JoinedCount;

        LoadSharedViewData();

        ViewBag.ActiveTab = TabProfile;
        ViewBag.PasswordModel = new ChangePasswordDto();
        
        return View("Index", model);
    }

    [HttpPost("change-password")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto passwordModel, CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User);
        if (userId == null) return RedirectToAction("Login", "Account");

        try
        {
            await profileService.ChangePasswordAsync(userId, passwordModel, cancellationToken);

            logger.LogInformation("User {UserId} changed their password.", userId);
            TempData["SuccessMessage"] = "Password changed successfully!";
            
            return RedirectToAction(nameof(Index));
        }
        catch (ValidationException ex)
        {
            foreach (var error in ex.Errors)
            {
                ModelState.AddModelError("PasswordError", error.ErrorMessage);
            }
        }

        var profileModel = await profileService.GetProfileAsync(userId, cancellationToken);

        LoadSharedViewData();

        ViewBag.ActiveTab = TabSecurity;
        ViewBag.PasswordModel = passwordModel;
        
        return View("Index", profileModel);
    }

    #region Private Helpers

    private void LoadSharedViewData()
    {
        ViewBag.Countries = new SelectList(
            CountryConstants.SupportedCountries, 
            nameof(CountryInfo.Code),
            nameof(CountryInfo.DisplayValue)
        );
    }

    #endregion
}