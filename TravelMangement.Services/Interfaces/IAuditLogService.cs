using TravelManagement.Data.Entities;
using TravelManagement.Services.Shared.DTOs;
using TravelManagement.Services.Shared.Enums;

namespace TravelManagement.Services.Interfaces;

/// <summary>
/// Owns construction of audit trail entries (spec section 15). Other workflow
/// services call Record(...) while they still hold the TravelRequest in memory
/// so the new entry is included in the same SaveChanges as the rest of that
/// operation; GetHistoryAsync is for standalone history lookups.
/// </summary>
public interface IAuditLogService
{
    AuditLog Record(TravelRequest request, int performedByUserId, AuditActionType action, string? comment = null);

    Task<IReadOnlyList<AuditLogDto>> GetHistoryAsync(int travelRequestId);
}
