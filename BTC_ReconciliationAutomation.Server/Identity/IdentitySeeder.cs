using Microsoft.AspNetCore.Identity;

namespace BTC_ReconciliationAutomation.Server.Identity;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        const string email = "admin@btc.com";
        const string password = "Admin@1234";

        if (await userManager.FindByEmailAsync(email) == null)
        {
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = "System",
                LastName = "Admin",
                EmailConfirmed = true
            };

            await userManager.CreateAsync(user, password);
        }
    }
}
