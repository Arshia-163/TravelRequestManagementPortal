using TravelManagement.Services.Shared.Enums;

namespace TravelManagement.Services.Shared.DTOs;

public sealed record PendingExtensionDto(
    DateTime NewEndDate,
    decimal AdditionalEstimatedCost,
    decimal TotalEstimatedCost,
    string Reason,
    ExtensionStatus Status
);