using TravelManagement.Services.Shared.Enums;

namespace TravelManagement.Services.Shared.DTOs;

public sealed record UserDto(
    int Id,
    string Name,
    string Email,
    string? Phone,
    Gender Gender,
    int? DepartmentId,
    IReadOnlyList<string> Roles,
    string? EmployeeCode = null,
    DateTime CreatedAt = default,
    bool IsActive = true
);