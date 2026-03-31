using Microsoft.AspNetCore.Identity;
using TravelGallery.Models;

namespace TravelGallery.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider services, IConfiguration config)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        foreach (var role in new[] { "Admin", "User" })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        var adminEmail = config["AdminSeed:Email"] ?? "admin@traveler.cz";
        var adminPassword = config["AdminSeed:Password"] ?? "Admin123!";
        var adminName = config["AdminSeed:DisplayName"] ?? "Admin";

        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                DisplayName = adminName,
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(admin, adminPassword);
            if (result.Succeeded)
                await userManager.AddToRoleAsync(admin, "Admin");
        }
    }
}
