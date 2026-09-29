using TravelManagement.Services.Shared.Enums;

namespace TravelManagement.Data.Entities;

public class TripExtension
{
    public int Id { get; set; }

    public int TravelRequestId { get; set; }

    public TravelRequest TravelRequest { get; set; } = null!;

    public DateTime NewEndDate { get; set; }

    public decimal AdditionalEstimatedCost { get; set; }

    public decimal TotalEstimatedCost { get; set; }

    public string Reason { get; set; } = string.Empty;

    public ExtensionStatus Status { get; set; } = ExtensionStatus.PendingManager;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}
