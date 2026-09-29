using TravelManagement.Services.Shared.Enums;

namespace TravelManagement.Services.Shared.DTOs;

public sealed record BookingDto(int Id, int TravelRequestId, BookingType Type, string Provider,
    string BookingReference, decimal ActualCost, BookingStatus Status, DateTime BookingDate,
    string BookedByName, string? Notes);
