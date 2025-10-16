using Compliance_Tracker.Constants;
using Compliance_Tracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Compliance_Tracker.Controllers;

[AllowAnonymous]
public class SetupController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public SetupController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _signInManager = signInManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        // Check if any users exist
        var usersExist = _userManager.Users.Any();
        if (usersExist)
        {
            return RedirectToAction("Index", "Home");
        }

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(SetupViewModel model)
    {
        // Double check no users exist
        var usersExist = _userManager.Users.Any();
        if (usersExist)
        {
            return RedirectToAction("Index", "Home");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            // Create all roles first
            foreach (var roleName in Roles.AllRoles)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    await _roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Create the admin user
            var user = new ApplicationUser
            {
                UserName = model.Username,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                EmailConfirmed = true,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Assign Admin role
                await _userManager.AddToRoleAsync(user, Roles.Admin);

                // Sign in the user
                await _signInManager.SignInAsync(user, isPersistent: false);

                TempData["SuccessMessage"] = "Initial setup completed successfully! Welcome to Compliance Tracker.";
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"An error occurred during setup: {ex.Message}");
        }

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> CheckSetupRequired()
    {
        var usersExist = _userManager.Users.Any();
        return Json(new { setupRequired = !usersExist });
    }
}
