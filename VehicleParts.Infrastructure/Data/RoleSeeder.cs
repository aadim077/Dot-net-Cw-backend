using Microsoft.AspNetCore.Identity;
using VehicleParts.Domain.Enums;

namespace VehicleParts.Infrastructure.Data;

public static class RoleSeeder
{
    public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        var roles = new[] { UserRoles.Admin, UserRoles.Staff, UserRoles.Customer };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }
}
