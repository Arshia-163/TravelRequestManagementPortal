using TravelManagement.Services.Shared.DTOs;

namespace TravelManagement.Services.Interfaces;

/// <summary>TravelAdmin dashboard and department administration (spec sections 5, 18).</summary>
public interface IAdminService
{
    Task<DashboardDto> GetDashboardAsync();

    Task<IReadOnlyList<DepartmentDto>> GetDepartmentsAsync();

    Task<DepartmentDto> CreateDepartmentAsync(DepartmentInput input);

    Task<DepartmentDto> UpdateDepartmentAsync(int id, DepartmentInput input);

    Task DeleteDepartmentAsync(int id);
}
