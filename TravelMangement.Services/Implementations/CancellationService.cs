
using TravelManagement.Data.Repositories.Interfaces;
using TravelManagement.Services.Interfaces;
using TravelManagement.Services.Shared.Enums;

namespace TravelManagement.Services.Implementations;

public sealed class CancellationService : ICancellationService
{
    private readonly ITravelRequestRepository requests;
    private readonly IAuditLogService auditLog;

    public CancellationService(
        ITravelRequestRepository requests,
        IAuditLogService auditLog)
    {
        this.requests = requests;
        this.auditLog = auditLog;
    }

    public async Task CancelAsync(
        int userId,
        int requestId,
        string reason)
    {
        RequestWorkflowRules.RequireReason(reason);

        var item = await RequestWorkflowRules.GetOwnedAsync(
            requests,
            requestId,
            userId);

        
        var isFinal = item.Status is TravelRequestStatus.Rejected
            or TravelRequestStatus.Cancelled
            or TravelRequestStatus.Completed;

        var isPastApprovedTravel =
            item.Status == TravelRequestStatus.Approved &&
            item.EndDate.Date < DateTime.UtcNow.Date;

        if (isFinal || isPastApprovedTravel)
        {
            throw new InvalidOperationException(
                "This request cannot be cancelled.");
        }

       
        var pendingExtensions = item.TripExtensions
            .Where(e => e.Status is ExtensionStatus.PendingManager
                or ExtensionStatus.PendingDeptHead)
            .ToList();

        foreach (var extension in pendingExtensions)
        {
            extension.Status = ExtensionStatus.Cancelled;
            extension.UpdatedAt = DateTime.UtcNow;
        }

        item.Status = TravelRequestStatus.Cancelled;
        item.UpdatedAt = DateTime.UtcNow;

        auditLog.Record(
            item,
            userId,
            AuditActionType.Cancelled,
            reason);

        await requests.SaveAsync();
    }
}
