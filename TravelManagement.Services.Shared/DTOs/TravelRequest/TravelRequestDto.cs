namespace TravelManagement.Services.Shared.DTOs;

public sealed record TravelRequestDto(
    int Id,
    string EmployeeName,
    string DepartmentName,
    TravelManagement.Services.Shared.Enums.TravelType TravelType,
    string Destination,
    string BusinessJustification,
    DateTime StartDate,
    DateTime EndDate,
    decimal EstimatedCost,
    string Currency,
    TravelManagement.Services.Shared.Enums.TravelRequestStatus Status,
    DateTime CreatedAt,
    IReadOnlyList<AuditLogDto> History,
    PendingExtensionDto? PendingExtension = null,
    string Source = "",
    bool AccommodationRequired = false,
    string? HotelName = null,
    string? HotelCity = null,
    DateTime? CheckInDate = null,
    DateTime? CheckOutDate = null,
    IReadOnlyList<ExtensionHistoryDto>? Extensions = null,

    string? DecidedBy = null,
    bool DecidedByCurrentUser = false,
    bool? DecidedApproved = null
);