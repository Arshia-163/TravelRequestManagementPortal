
using Microsoft.AspNetCore.Identity;
using TravelManagement.Data.Entities;
using TravelManagement.Data.Repositories.Interfaces;
using TravelManagement.Services.Interfaces;
using TravelManagement.Services.Shared.Constants;
using TravelManagement.Services.Shared.DTOs;
using TravelMangement.Services.Mappers;

namespace TravelManagement.Services.Implementations;

public sealed class AdminService : IAdminService
{
    private readonly IUserRepository users;
    private readonly IDepartmentRepository departments;
    private readonly IUserService userService;
    private readonly UserManager<ApplicationUser> userManager;

    public AdminService(
        IUserRepository users,
        IDepartmentRepository departments,
        IUserService userService,
        UserManager<ApplicationUser> userManager)
    {
        this.users = users;
        this.departments = departments;
        this.userService = userService;
        this.userManager = userManager;
    }

    public async Task<DashboardDto> GetDashboardAsync()
    {
        return new DashboardDto(
            await users.CountAsync(),
            await departments.CountAsync(),
            await departments.CountDistinctManagersAsync(),
            await userService.GetRecentlyJoinedAsync(5));
    }

    public async Task<IReadOnlyList<DepartmentDto>> GetDepartmentsAsync()
    {
        return (await departments.GetAllAsync())
            .Select(MapDepartment)
            .ToList();
    }

    public async Task<DepartmentDto> CreateDepartmentAsync(DepartmentInput input)
    {
        await ValidateDepartmentInputAsync(input, isCreate: true);

        var item = new Department();

        ApplyDepartment(item, input);
        await EnsureDepartmentNameAvailableAsync(item.Name);

        await departments.AddAsync(item);
        await departments.SaveAsync();

        return MapDepartment(item);
    }

    public async Task<DepartmentDto> UpdateDepartmentAsync(
        int id,
        DepartmentInput input)
    {
        var item = await departments.GetAsync(id)
            ?? throw new InvalidOperationException("Department was not found.");


        await ValidateDepartmentInputAsync(input, isCreate: false);

        var name = input.Name?.Trim() ?? string.Empty;

        await EnsureDepartmentNameAvailableAsync(name, item.Id);

        ApplyDepartment(item, input);

        await departments.SaveAsync();

        return MapDepartment(item);
    }

    public async Task DeleteDepartmentAsync(int id)
    {
        var item = await departments.GetAsync(id)
            ?? throw new InvalidOperationException("Department was not found.");

        if (await departments.HasDependenciesAsync(id))
        {
            throw new InvalidOperationException(
                "This department cannot be deleted because employees or travel requests are assigned to it. Reassign them first.");
        }

        departments.Delete(item);
        await departments.SaveAsync();
    }

    private async Task ValidateDepartmentInputAsync(
        DepartmentInput input,
        bool isCreate)
    {
        if (string.IsNullOrWhiteSpace(input.Name) ||
            input.TravelApprovalLimit <= 0)
        {
            throw new InvalidOperationException(
                "Department name and approval limit are required.");
        }


        if (isCreate &&
            (input.ManagerId is null || input.DepartmentHeadId is null))
        {
            throw new InvalidOperationException(
                "A Manager and a Department Head must both be assigned when creating a department.");
        }

        if (input.ManagerId is not null &&
            input.ManagerId == input.DepartmentHeadId)
        {
            throw new InvalidOperationException(
                "Choose two different people: the Manager and Department Head cannot be the same person.");
        }

        if (input.ManagerId is not null)
        {
            var manager = await userManager.FindByIdAsync(
                input.ManagerId.Value.ToString())
                ?? throw new InvalidOperationException("Manager was not found.");

            if (!await userManager.IsInRoleAsync(manager, RoleNames.Manager))
            {
                throw new InvalidOperationException(
                    "Selected Manager must already hold the Manager role. Grant it from Edit user first.");
            }
        }

        if (input.DepartmentHeadId is not null)
        {
            var head = await userManager.FindByIdAsync(
                input.DepartmentHeadId.Value.ToString())
                ?? throw new InvalidOperationException(
                    "Department Head was not found.");

            if (!await userManager.IsInRoleAsync(
                    head,
                    RoleNames.DepartmentHead))
            {
                throw new InvalidOperationException(
                    "Selected Department Head must already hold the DepartmentHead role. Grant it from Edit user first.");
            }
        }
    }

    private async Task EnsureDepartmentNameAvailableAsync(
        string name,
        int? currentDepartmentId = null)
    {
        if (await departments.NameExistsAsync(name, currentDepartmentId))
        {
            throw new InvalidOperationException(
                "A department with this name already exists.");
        }
    }

    private static void ApplyDepartment(
        Department target,
        DepartmentInput source)
    {
        target.Name = source.Name?.Trim() ?? string.Empty;
        target.TravelApprovalLimit = source.TravelApprovalLimit;

        target.ManagerId = source.ManagerId;
        target.DepartmentHeadId = source.DepartmentHeadId;
    }

    private static DepartmentDto MapDepartment(Department x)
    {
        return DepartmentMapper.ToDto(x);
    }
}

