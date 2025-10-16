using Compliance_Tracker.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Compliance_Tracker.ViewComponents;

public class UserAvatarViewComponent : ViewComponent
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserAvatarViewComponent(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return Content("");
        }

        var user = await _userManager.GetUserAsync(UserClaimsPrincipal);
        if (user == null)
        {
            return Content("");
        }

        var model = new
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            AvatarColor = user.AvatarColor,
            Initials = GetInitials(user.FirstName, user.LastName)
        };

        return View(model);
    }

    private string GetInitials(string? firstName, string? lastName)
    {
        var first = !string.IsNullOrEmpty(firstName) ? firstName[0].ToString().ToUpper() : "";
        var last = !string.IsNullOrEmpty(lastName) ? lastName[0].ToString().ToUpper() : "";
        return first + last;
    }
}
