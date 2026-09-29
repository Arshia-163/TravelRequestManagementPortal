using System.ComponentModel.DataAnnotations;
using TravelManagement.Services.Shared.Enums;

namespace TravelManagement.Services.Shared.DTOs;

/// <summary>
/// Input for adding one booking record (Flight, Hotel, Train or Cab) to an
/// Approved travel request from the Travel Admin's "Manage bookings" page.
/// A single travel request can have several of these.
/// </summary>
public sealed class BookingCreateInput
{
    public BookingType Type { get; set; } = BookingType.Flight;

    [Required, StringLength(200)]
    public string Provider { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string BookingReference { get; set; } = string.Empty;

    public decimal ActualCost { get; set; }

    public BookingStatus Status { get; set; } = BookingStatus.Pending;

    public string? Notes { get; set; }
}
