using TravelManagement.Services.Shared.Enums;

namespace TravelManagement.Services.Shared.DTOs;


public sealed record AdminBookingDto(
    int Id,
    int TravelRequestId, 
    string EmployeeName, 
    string DepartmentName,
    string Destination, 
    BookingType Type, 
    string Provider, 
    string BookingReference,
    decimal ActualCost,
    string Currency, 
    BookingStatus Status,
    DateTime BookingDate,
    string BookedByName, 
    string? Notes);
