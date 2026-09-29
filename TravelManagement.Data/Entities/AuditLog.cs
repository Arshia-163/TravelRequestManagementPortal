using TravelManagement.Services.Shared.Enums;

namespace TravelManagement.Data.Entities;


public class AuditLog
{
    public int Id { get; set; }

    public int TravelRequestId { get; set; }

    public TravelRequest TravelRequest { get; set; } = null!;

    public int PerformedByUserId { get; set; }

    public ApplicationUser PerformedByUser { get; set; } = null!;

    public AuditActionType Action { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public string? Comment { get; set; }
}
