namespace TravelManagement.Services.Shared.DTOs;

public sealed record CurrentUserDto(
    int Id,
    string Name,
    int? DepartmentId,
    string? DepartmentName,
    IReadOnlyList<string> Roles,
    bool ManagesDepartments,
    bool HeadsDepartments
);