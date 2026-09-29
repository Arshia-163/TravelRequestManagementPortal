using TravelManagement.Services.Interfaces;
using TravelManagement.Services.Shared.DTOs;

namespace TravelRequestManagement.Services;


public sealed partial class ServerTravelApi
{
    public Task<DashboardDto> AdminDashboardAsync() =>
        Run(TravelAdminOnly, (sp, _) => sp.GetRequiredService<IAdminService>().GetDashboardAsync());

    public Task<TravelRequestDto> AdminRequestHistoryAsync(int id) =>
        Run(TravelAdminOnly, async (sp, userId) =>
            await sp.GetRequiredService<ITravelRequestService>().GetAsync(id, userId, canViewAll: true)
            ?? throw new KeyNotFoundException("No request was found with that request number."));

    public Task<IReadOnlyList<TravelRequestDto>> AdminAllRequestsAsync() =>
        Run(TravelAdminOnly, (sp, _) => sp.GetRequiredService<ITravelRequestService>().GetAllAsync());

    

    public Task<IReadOnlyList<UserDto>> UsersAsync() =>
        Run(TravelAdminOnly, (sp, _) => sp.GetRequiredService<IUserService>().GetUsersAsync());

    public Task<UserDto> CreateUserAsync(UserInput input) =>
        Run(TravelAdminOnly, (sp, _) => sp.GetRequiredService<IUserService>().CreateUserAsync(input));

    public Task<UserDto> UpdateUserAsync(int id, UserInput input) =>
        Run(TravelAdminOnly, (sp, _) => sp.GetRequiredService<IUserService>().UpdateUserAsync(id, input));

    public Task<UserDto> ActivateUserAsync(int id) =>
        Run(TravelAdminOnly, (sp, userId) => sp.GetRequiredService<IUserService>().SetActiveAsync(userId, id, true));

    public Task<UserDto> DeactivateUserAsync(int id) =>
        Run(TravelAdminOnly, (sp, userId) => sp.GetRequiredService<IUserService>().SetActiveAsync(userId, id, false));

    public Task DeleteUserAsync(int id) =>
        RunVoid(TravelAdminOnly, (sp, userId) => sp.GetRequiredService<IUserService>().DeleteUserAsync(userId, id));

    // -- departments --

    public Task<IReadOnlyList<DepartmentDto>> DepartmentsAsync() =>
        Run(TravelAdminOnly, (sp, _) => sp.GetRequiredService<IAdminService>().GetDepartmentsAsync());

    public Task<DepartmentDto> CreateDepartmentAsync(DepartmentInput input) =>
        Run(TravelAdminOnly, (sp, _) => sp.GetRequiredService<IAdminService>().CreateDepartmentAsync(input));

    public Task<DepartmentDto> UpdateDepartmentAsync(int id, DepartmentInput input) =>
        Run(TravelAdminOnly, (sp, _) => sp.GetRequiredService<IAdminService>().UpdateDepartmentAsync(id, input));

    public Task DeleteDepartmentAsync(int id) =>
        RunVoid(TravelAdminOnly, (sp, _) => sp.GetRequiredService<IAdminService>().DeleteDepartmentAsync(id));
}
