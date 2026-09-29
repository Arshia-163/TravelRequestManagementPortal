using TravelManagement.Services.Shared.Enums;

namespace TravelManagement.Services.Shared.DTOs;

/// <summary>Input for moving a booking forward to a new status.</summary>
public sealed class BookingStatusUpdateInput
{
    public BookingStatus Status { get; set; }
}
