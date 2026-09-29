using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using TravelManagement.Data.Entities;
using TravelManagement.Data.Repositories.Interfaces;
using TravelManagement.Services.Interfaces;
using TravelManagement.Services.Shared.Constants;
using TravelManagement.Services.Shared.DTOs;

namespace TravelManagement.Services.Implementations;

public sealed class UserService(
    IUserRepository users,
    IDepartmentRepository departments,
    UserManager<ApplicationUser> userManager) : IUserService
{
    public async Task<IReadOnlyList<UserDto>> GetUsersAsync() =>
        await MapAllAsync(await users.GetAllOrderedAsync());

    public async Task<IReadOnlyList<UserDto>> GetRecentlyJoinedAsync(int count) =>
        await MapAllAsync(await users.GetRecentlyJoinedAsync(count));

    private async Task<IReadOnlyList<UserDto>> MapAllAsync(IReadOnlyList<ApplicationUser> people)
    {
        var result = new List<UserDto>(people.Count);
        foreach (var person in people)
            result.Add(await MapUserAsync(person));
        return result;
    }

    public async Task<UserDto> CreateUserAsync(UserInput input)
    {
       
        if (!input.Roles.Contains(RoleNames.Employee))
            input.Roles = [.. input.Roles, RoleNames.Employee];

        ValidateUser(input);
        if (await userManager.FindByEmailAsync(input.Email.Trim()) is not null)
            throw new InvalidOperationException("An account with this email already exists.");
        await EnsureDepartmentExistsAsync(input.DepartmentId);

        var person = new ApplicationUser
        {
            UserName = input.Email.Trim(),
            Email = input.Email.Trim(),
            FirstName = input.FirstName.Trim(),
            LastName = input.LastName.Trim(),
            PhoneNumber = input.Phone.Trim(),
            Gender = input.Gender,
            DepartmentId = input.DepartmentId,
            EmailConfirmed = true,
            EmployeeCode = await GenerateEmployeeCodeAsync()
        };

        try
        {
            var created = await userManager.CreateAsync(person, string.IsNullOrWhiteSpace(input.Password) ? "Welcome@123" : input.Password);
            if (!created.Succeeded) throw new InvalidOperationException(string.Join("; ", created.Errors.Select(x => x.Description)));
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException(DescribeSaveFailure(ex));
        }

        await SetRolesAsync(person, input.Roles);
        return await MapUserAsync(person);
    }

    public async Task<UserDto> UpdateUserAsync(int id, UserInput input)
    {
        ValidateUser(input);
        await EnsureDepartmentExistsAsync(input.DepartmentId);

        var person = await userManager.FindByIdAsync(id.ToString()) ?? throw new InvalidOperationException("User was not found.");
        var duplicate = await userManager.FindByEmailAsync(input.Email.Trim());
        if (duplicate is not null && duplicate.Id != id)
            throw new InvalidOperationException("An account with this email already exists.");

        person.UserName = person.Email = input.Email.Trim();
        person.FirstName = input.FirstName.Trim();
        person.LastName = input.LastName.Trim();
        person.PhoneNumber = input.Phone.Trim();
        person.Gender = input.Gender;
    
        person.DepartmentId = input.DepartmentId;

        try
        {
            var updated = await userManager.UpdateAsync(person);
            if (!updated.Succeeded) throw new InvalidOperationException(string.Join("; ", updated.Errors.Select(x => x.Description)));
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException(DescribeSaveFailure(ex));
        }

        await SetRolesAsync(person, input.Roles);
        return await MapUserAsync(person);
    }

    public async Task<UserDto> SetActiveAsync(int currentUserId, int targetUserId, bool active)
    {
        var person = await userManager.FindByIdAsync(targetUserId.ToString()) ?? throw new InvalidOperationException("User was not found.");

        if (!active)
        {
            if (targetUserId == currentUserId)
                throw new InvalidOperationException("You cannot deactivate your own account.");

            if (await userManager.IsInRoleAsync(person, RoleNames.TravelAdmin))
            {
                var admins = await userManager.GetUsersInRoleAsync(RoleNames.TravelAdmin);
                var otherActiveAdmins = admins.Any(a => a.Id != targetUserId && !IsLockedOut(a));
                if (!otherActiveAdmins)
                    throw new InvalidOperationException("Cannot deactivate the last active Travel Admin.");
            }
        }

        await userManager.SetLockoutEnabledAsync(person, true);
        await userManager.SetLockoutEndDateAsync(person, active ? null : DateTimeOffset.MaxValue);
        return await MapUserAsync(person);
    }

    public async Task DeleteUserAsync(int currentUserId, int targetUserId)
    {
        if (targetUserId == currentUserId)
            throw new InvalidOperationException("You cannot delete your own account.");

        var person = await userManager.FindByIdAsync(targetUserId.ToString())
            ?? throw new InvalidOperationException("User was not found.");

        if (await users.HasWorkflowReferencesAsync(targetUserId))
            throw new InvalidOperationException("This user cannot be deleted because their travel, audit, or booking history must be retained. Deactivate the account instead.");

        if (await departments.IsManagerOfAnyAsync(targetUserId) || await departments.IsDepartmentHeadOfAnyAsync(targetUserId))
            throw new InvalidOperationException("This user cannot be deleted while assigned as a department Manager or Department Head. Reassign those departments first.");

        if (await userManager.IsInRoleAsync(person, RoleNames.TravelAdmin))
        {
            var administrators = await userManager.GetUsersInRoleAsync(RoleNames.TravelAdmin);
            if (!administrators.Any(admin => admin.Id != targetUserId && !IsLockedOut(admin)))
                throw new InvalidOperationException("Cannot delete the last active Travel Admin.");
        }

        try
        {
            var deleted = await userManager.DeleteAsync(person);
            if (!deleted.Succeeded)
                throw new InvalidOperationException(string.Join("; ", deleted.Errors.Select(error => error.Description)));
        }
        catch (DbUpdateException ex)
        {
           
            throw new InvalidOperationException(DescribeSaveFailure(ex));
        }
    }

    private static bool IsLockedOut(ApplicationUser person) =>
        person.LockoutEnd is not null && person.LockoutEnd > DateTimeOffset.UtcNow;

    private static string DescribeSaveFailure(DbUpdateException ex) =>
        IsUniqueConstraintViolation(ex)
            ? "Could not save these changes because one of the details (email, phone, or employee code) is already used by another account, possibly from a request submitted at the same moment. Please check the details and try again."
            : "Could not save these changes due to a database error. Please check the details and try again, or contact support if the problem continues.";

    private static bool IsUniqueConstraintViolation(DbUpdateException ex) =>
        ex.InnerException is SqlException { Number: 2601 or 2627 };

    public async Task<CurrentUserDto> GetCurrentUserAsync(int userId)
    {
        var person = await userManager.FindByIdAsync(userId.ToString())
            ?? throw new InvalidOperationException("User was not found.");

        var roles = await userManager.GetRolesAsync(person);
        var departmentName = person.DepartmentId is null
            ? null
            : (await departments.GetAsync(person.DepartmentId.Value))?.Name;
        var isManager = await departments.IsManagerOfAnyAsync(userId);
        var isDepartmentHead = await departments.IsDepartmentHeadOfAnyAsync(userId);

        return new CurrentUserDto(
            person.Id,
            $"{person.FirstName} {person.LastName}".Trim(),
            person.DepartmentId,
            departmentName,
            roles.ToArray(),
            isManager,
            isDepartmentHead);
    }

    private async Task SetRolesAsync(ApplicationUser person, IEnumerable<string> requested)
    {
        var valid = requested.Where(RoleNames.All.Contains).Distinct().ToArray();
        var current = await userManager.GetRolesAsync(person);

      
        if (current.Contains(RoleNames.Manager) && !valid.Contains(RoleNames.Manager)
            && await departments.IsManagerOfAnyAsync(person.Id))
            throw new InvalidOperationException("This user is still assigned as Manager of a department. Reassign that department's Manager first, from the Departments page.");

        if (current.Contains(RoleNames.DepartmentHead) && !valid.Contains(RoleNames.DepartmentHead)
            && await departments.IsDepartmentHeadOfAnyAsync(person.Id))
            throw new InvalidOperationException("This user is still assigned as Department Head of a department. Reassign that department's Department Head first, from the Departments page.");

        var removed = await userManager.RemoveFromRolesAsync(person, current);
        if (!removed.Succeeded) throw new InvalidOperationException("Unable to update user roles.");

        if (valid.Length > 0)
        {
            try
            {
                var added = await userManager.AddToRolesAsync(person, valid);
                if (!added.Succeeded) throw new InvalidOperationException("Unable to update user roles.");
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException(DescribeSaveFailure(ex));
            }
        }
    }

    private async Task EnsureDepartmentExistsAsync(int? id)
    {
        if (id is not null && await departments.GetAsync(id.Value) is null)
            throw new InvalidOperationException("The selected department does not exist.");
    }

    private static void ValidateUser(UserInput input)
    {
        if (string.IsNullOrWhiteSpace(input.FirstName) ||
            string.IsNullOrWhiteSpace(input.LastName) ||
            string.IsNullOrWhiteSpace(input.Email) ||
            string.IsNullOrWhiteSpace(input.Phone))
        {
            throw new InvalidOperationException("Name, email and phone are required.");
        }

        if (!new EmailAddressAttribute().IsValid(input.Email.Trim()))
            throw new InvalidOperationException("A valid email address is required.");

        if (!System.Text.RegularExpressions.Regex.IsMatch(input.Phone.Trim(), @"^\d{10}$"))
            throw new InvalidOperationException("Phone must be exactly 10 digits.");

        if (!Enum.IsDefined(input.Gender))
            throw new InvalidOperationException("Gender must be Male, Female or Other.");

        var requestedRoles = input.Roles?.Where(RoleNames.All.Contains).Distinct().ToArray() ?? [];
        if (requestedRoles.Length == 0)
            throw new InvalidOperationException("Select at least one role for the employee.");
    }

    private async Task<string> GenerateEmployeeCodeAsync()
    {
        var next = await users.CountWithEmployeeCodeAsync() + 1;
        return next.ToString();
    }

    private async Task<UserDto> MapUserAsync(ApplicationUser x)
    {
        var roles = (await userManager.GetRolesAsync(x)).ToArray();

        return new UserDto(
            x.Id,
            $"{x.FirstName} {x.LastName}".Trim(),
            x.Email ?? "",
            x.PhoneNumber,
            x.Gender,
            x.DepartmentId,
            roles,
            x.EmployeeCode,
            x.CreatedAt,
            !IsLockedOut(x));
    }
}
