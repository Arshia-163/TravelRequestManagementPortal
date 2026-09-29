using TravelManagement.Services.Shared.Enums;

namespace TravelManagement.Data.Entities;

public class TravelRequest
{
    public int Id { get; set; }

  
    public int UserId { get; set; }

    public ApplicationUser User { get; set; } = null!;

    public int DepartmentId { get; set; }

    public Department Department { get; set; } = null!;

    public TravelType TravelType { get; set; } = TravelType.Domestic;

   
    public string Source { get; set; } = string.Empty;

   
    public string Destination { get; set; } = string.Empty;

    public string BusinessJustification { get; set; } = string.Empty;

    
    public bool AccommodationRequired { get; set; }

    public string? HotelName { get; set; }

    public string? HotelCity { get; set; }

    public DateTime? CheckInDate { get; set; }

    public DateTime? CheckOutDate { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public decimal EstimatedCost { get; set; }

    public string Currency { get; set; } = string.Empty;

    public TravelRequestStatus Status { get; set; } = TravelRequestStatus.Draft;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public ICollection<TripExtension> TripExtensions { get; set; } = new List<TripExtension>();

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}
