using TravelManagement.Services.Shared.Enums;

namespace TravelManagement.Services.Shared.DTOs;

public sealed record AuditLogDto(
    AuditActionType Action,
    string PerformedBy,
    DateTime Timestamp,
    string? Comment,
    int PerformedByUserId = 0
);