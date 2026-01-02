using EventPlanning.Application.DTOs.Auth;
using EventPlanning.Infrastructure.Identity;
using EventPlanning.Application.Interfaces;
using EventPlanning.Application.Utils;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EventPlanning.Web.Controllers;

public class AccountController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IIdentityService identityService,
    IValidator<RegisterUserDto> registerValidator,
    IValidator<LoginUserDto> loginValidator,
    IEmailSender<ApplicationUser> emailSender,
    ILogger<AccountController> logger) : Controller
{
    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Home");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("register-limit")]
    public async Task<IActionResult> Register(RegisterUserDto model)
    {
        var validationResult = await registerValidator.ValidateAsync(model);
        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            return View(model);
        }

        try
        {
            var (succeeded, errors, userId, code) = await identityService.RegisterUserAsync(model);

            if (succeeded && userId.HasValue && !string.IsNullOrEmpty(code))
            {
                var appUser = await userManager.FindByIdAsync(userId.Value.ToString());
                
                var callbackUrl = Url.Action(
                    "ConfirmEmail",
                    "Account",
                    new { userId = userId.Value, code },
                    protocol: Request.Scheme);

                if (string.IsNullOrEmpty(callbackUrl) || appUser == null)
                {
                    logger.LogError("Error generating confirmation mechanism for {Email}", LogRedactor.RedactEmail(model.Email));
                    ModelState.AddModelError(string.Empty, "Error generating confirmation email.");
                    return View(model);
                }

                await emailSender.SendConfirmationLinkAsync(appUser, model.Email, callbackUrl);

                if (userManager.Options.SignIn.RequireConfirmedAccount)
                {
                    ViewBag.Email = model.Email;
                    return View("RegisterConfirmation");
                }
                else
                {
                    await signInManager.SignInAsync(appUser, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }
            }
            
            foreach (var error in errors)
                ModelState.AddModelError(string.Empty, error);

            logger.LogWarning("Registration failed for {Email}: {Errors}", LogRedactor.RedactEmail(model.Email), string.Join(", ", errors));
            return View(model);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error during registration for {Email}", LogRedactor.RedactEmail(model.Email));
            ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
            return View(model);
        }
    }

    [HttpGet]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Home");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("login-limit")]
    public async Task<IActionResult> Login(LoginUserDto model, string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        var validationResult = await loginValidator.ValidateAsync(model);
        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            return View(model);
        }

        var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, true);

        if (result.Succeeded)
        {
            return LocalRedirect(returnUrl);
        }

        if (result.IsLockedOut)
        {
            logger.LogWarning("User {Email} locked out.", LogRedactor.RedactEmail(model.Email));
            ModelState.AddModelError(string.Empty, "Account is locked out.");
        }
        else
        {
            logger.LogWarning("Failed login attempt for {Email}.", LogRedactor.RedactEmail(model.Email));
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        }

        return View(model);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    [HttpGet("ConfirmEmail")]
    public async Task<IActionResult> ConfirmEmail(string? userId, string? code)
    {
        if (userId == null || code == null)
        {
            return RedirectToAction("Index", "Home");
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{userId}'.");
        }

        var result = await userManager.ConfirmEmailAsync(user, code);
        if (!result.Succeeded)
        {
            logger.LogError("Error confirming email for user {UserId}", userId);
            TempData["ErrorMessage"] = "Error confirming your email.";
            return RedirectToAction("Index", "Home");
        }

        TempData["SuccessMessage"] = "Thank you for confirming your email.";
        return View();
    }
}