using Microsoft.AspNetCore.Identity;

namespace BTC_ReconciliationAutomation.Server.Identity;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}
