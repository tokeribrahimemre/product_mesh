using Microsoft.AspNetCore.Identity;
using ProductMesh.Auth.Domain.Entities;
using ProductMesh.Auth.Infrastructure.Identity;
using ProductMesh.Auth.Infrastructure.Persistence;

namespace ProductMesh.Auth.API.Extensions;

public static class SeedDataExtensions
{
    public static async Task SeedRolesAndAdminAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

        await context.Database.EnsureCreatedAsync();

        // Seed roles
        string[] roles = [AppRoles.Admin, AppRoles.User];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // Seed admin user
        var adminEmail = "admin@productmesh.com";
        if (await userManager.FindByEmailAsync(adminEmail) is null)
        {
            var admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "System",
                LastName = "Admin",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(admin, "Admin123!");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(admin, AppRoles.Admin);
        }
    }
}
