using TravelManagement.Services.Shared.Enums;

namespace TravelManagement.Services.Shared.DTOs;

public sealed record ExtensionHistoryDto(
    int Id,
    DateTime RequestedAt,
    DateTime NewEndDate,
    decimal AdditionalEstimatedCost,
    decimal TotalEstimatedCost,
    ExtensionStatus Status,
    string? ManagerName,
    string? DepartmentHeadName
);