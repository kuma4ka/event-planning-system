using EventPlanning.Application.DTOs.Auth;
using EventPlanning.Domain.Entities;
using EventPlanning.Domain.Enums;
using EventPlanning.Domain.Interfaces;
using EventPlanning.Infrastructure.Identity;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EventPlanning.Web.Controllers;

public class AccountController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IUserRepository userRepository,
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
    public async Task<IActionResult> Register(RegisterUserDto model)
    {
        var validationResult = await registerValidator.ValidateAsync(model);
        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            return View(model);
        }

        var appUser = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            PhoneNumber = model.PhoneNumber
        };

        var result = await userManager.CreateAsync(appUser, model.Password);

        if (result.Succeeded)
        {
            try 
            {
                var domainUser = new User(
                    appUser.Id,
                    model.FirstName,
                    model.LastName,
                    UserRole.User,
                    model.Email,
                    model.Email,
                    model.PhoneNumber,
                    model.CountryCode
                );

                await userRepository.AddAsync(domainUser);

                var userId = await userManager.GetUserIdAsync(appUser);
                var code = await userManager.GenerateEmailConfirmationTokenAsync(appUser);
                
                var callbackUrl = Url.Action(
                    "ConfirmEmail",
                    "Account",
                    new { userId, code },
                    protocol: Request.Scheme);

                if (string.IsNullOrEmpty(callbackUrl))
                {
                    logger.LogError("Error generating confirmation email link for {Email}", model.Email);
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
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating domain user profile for {Email}", model.Email);
                await userManager.DeleteAsync(appUser);
                ModelState.AddModelError(string.Empty, "An error occurred while creating your profile. Please try again.");
                return View(model);
            }
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        logger.LogWarning("Registration failed for {Email}: {Errors}", model.Email, string.Join(", ", result.Errors.Select(e => e.Description)));

        return View(model);
    }

    [HttpGet]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Home");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
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

        var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);

        if (result.Succeeded)
        {
            return LocalRedirect(returnUrl);
        }

        if (result.IsLockedOut)
        {
            logger.LogWarning("User {Email} locked out.", model.Email);
            ModelState.AddModelError(string.Empty, "Account is locked out.");
        }
        else
        {
            logger.LogWarning("Failed login attempt for {Email}.", model.Email);
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