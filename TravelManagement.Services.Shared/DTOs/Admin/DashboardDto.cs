namespace TravelManagement.Services.Shared.DTOs;

public sealed record DashboardDto(
    int TotalEmployees,
    int TotalDepartments,
    int TotalManagers,
    IReadOnlyList<UserDto> RecentlyJoinedEmployees
);