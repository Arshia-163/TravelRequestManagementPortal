using TravelManagement.Services.Shared.Enums;

namespace TravelManagement.Data.Entities;


public class Booking
{
    public int Id { get; set; }

    public int TravelRequestId { get; set; }

    public TravelRequest TravelRequest { get; set; } = null!;

    public int BookedByUserId { get; set; }

    public ApplicationUser BookedByUser { get; set; } = null!;

   
    public BookingType Type { get; set; } = BookingType.Flight;

   
    public string Provider { get; set; } = string.Empty;

    public string BookingReference { get; set; } = string.Empty;

    public decimal ActualCost { get; set; }

  
    public BookingStatus Status { get; set; } = BookingStatus.Pending;

    public DateTime BookingDate { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
