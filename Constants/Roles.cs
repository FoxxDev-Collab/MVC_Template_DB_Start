namespace Compliance_Tracker.Constants;

public static class Roles
{
    public const string Admin = "Admin";
    public const string ISSM = "ISSM";
    public const string ISSO = "ISSO";
    public const string SystemAdmin = "System Admin";
    public const string Auditor = "Auditor";

    public static readonly string[] AllRoles = new[]
    {
        Admin,
        ISSM,
        ISSO,
        SystemAdmin,
        Auditor
    };
}
