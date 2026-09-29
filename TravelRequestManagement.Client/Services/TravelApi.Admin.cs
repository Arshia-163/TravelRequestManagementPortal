using TravelManagement.Services.Shared.DTOs;

namespace TravelRequestManagement.Client.Services;


public sealed partial class TravelApi
{
    public Task<DashboardDto> AdminDashboardAsync() =>
        Get<DashboardDto>("api/admin/dashboard");

    public Task<TravelRequestDto> AdminRequestHistoryAsync(int id) =>
        Get<TravelRequestDto>($"api/admin/requests/{id}");

    public Task<IReadOnlyList<TravelRequestDto>> AdminAllRequestsAsync() =>
        GetList<TravelRequestDto>("api/admin/requests");



    public Task<IReadOnlyList<UserDto>> UsersAsync() =>
        GetList<UserDto>("api/admin/users");

    public Task<UserDto> CreateUserAsync(UserInput input) =>
        Post<UserDto>("api/admin/users", input);

    public Task<UserDto> UpdateUserAsync(int id, UserInput input) =>
        Put<UserDto>($"api/admin/users/{id}", input);

    public Task<UserDto> ActivateUserAsync(int id) =>
        Post<UserDto>($"api/admin/users/{id}/activate", null);

    public Task<UserDto> DeactivateUserAsync(int id) =>
        Post<UserDto>($"api/admin/users/{id}/deactivate", null);

    public Task DeleteUserAsync(int id) =>
        Delete($"api/admin/users/{id}");

    // -- departments --

    public Task<IReadOnlyList<DepartmentDto>> DepartmentsAsync() =>
        GetList<DepartmentDto>("api/admin/departments");

    public Task<DepartmentDto> CreateDepartmentAsync(DepartmentInput input) =>
        Post<DepartmentDto>("api/admin/departments", input);

    public Task<DepartmentDto> UpdateDepartmentAsync(int id, DepartmentInput input) =>
        Put<DepartmentDto>($"api/admin/departments/{id}", input);

    public Task DeleteDepartmentAsync(int id) =>
        Delete($"api/admin/departments/{id}");
}
