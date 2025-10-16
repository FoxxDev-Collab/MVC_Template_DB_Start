using Microsoft.AspNetCore.Identity;

namespace Compliance_Tracker.Models;

public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginDate { get; set; }
    public bool IsActive { get; set; } = true;
    public string AvatarColor { get; set; } = "#1e9df1"; // Default primary color
}
