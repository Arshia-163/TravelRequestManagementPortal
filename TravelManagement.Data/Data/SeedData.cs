using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TravelManagement.Data.DbContext;
using TravelManagement.Data.Entities;
using TravelManagement.Services.Shared.Constants;
using TravelManagement.Services.Shared.Enums;

namespace TravelManagement.Data.Data;

public static class SeedData
{
    public static async Task InitializeAsync(
        ApplicationDbContext db,
        UserManager<ApplicationUser> users,
        RoleManager<IdentityRole<int>> roles)
    {
        await db.Database.MigrateAsync();

        foreach (var role in RoleNames.All)
        {
            if (!await roles.RoleExistsAsync(role))
            {
                await roles.CreateAsync(new IdentityRole<int>(role));
            }
        }

        var admin = await users.FindByEmailAsync("admin@travel.local");

        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = "admin@travel.local",
                Email = "admin@travel.local",
                FirstName = "Travel",
                LastName = "Admin",
                PhoneNumber = "0000000000",
                Gender = Gender.Other,
                EmailConfirmed = true,
          
                EmployeeCode = "1"
            };

            var result = await users.CreateAsync(admin, "Admin@12345");

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join("; ", result.Errors.Select(x => x.Description)));
            }

            await users.AddToRoleAsync(admin, RoleNames.Employee);
            await users.AddToRoleAsync(admin, RoleNames.TravelAdmin);
        }
        else if (string.IsNullOrWhiteSpace(admin.EmployeeCode))
        {
           
            admin.EmployeeCode = "1";
            await users.UpdateAsync(admin);
        }
    }
}


public sealed class IdentitySeederService(
    ApplicationDbContext db,
    UserManager<ApplicationUser> users,
    RoleManager<IdentityRole<int>> roles)
{
    public Task SeedAsync()
        => SeedData.InitializeAsync(db, users, roles);
}