
using TravelManagement.Data.Entities;
using TravelManagement.Data.Repositories.Interfaces;
using TravelManagement.Services.Interfaces;
using TravelManagement.Services.Shared.DTOs;
using TravelManagement.Services.Shared.Enums;

namespace TravelManagement.Services.Implementations;

public sealed class TripExtensionService : ITripExtensionService
{
    private readonly ITravelRequestRepository requests;
    private readonly IAuditLogService auditLog;

    public TripExtensionService(
        ITravelRequestRepository requests,
        IAuditLogService auditLog)
    {
        this.requests = requests;
        this.auditLog = auditLog;
    }

    public async Task RequestExtensionAsync(
        int userId,
        int requestId,
        ExtensionInput input)
    {
        var item = await RequestWorkflowRules.GetOwnedAsync(
            requests,
            requestId,
            userId);

        if (item.Status is TravelRequestStatus.Cancelled)
        {
            throw new InvalidOperationException(
                "This request has been cancelled and cannot be extended.");
        }

        if (item.Status is TravelRequestStatus.Rejected)
        {
            throw new InvalidOperationException(
                "This request was rejected and cannot be extended.");
        }

        if (item.Status is TravelRequestStatus.Completed ||
            (item.Status is TravelRequestStatus.Approved &&
             item.EndDate.Date < DateTime.UtcNow.Date))
        {
            throw new InvalidOperationException(
                "This trip has already ended and cannot be extended.");
        }

        if (item.Status != TravelRequestStatus.Approved)
        {
            throw new InvalidOperationException(
                "Only approved travel can be extended.");
        }

        if (input.NewEndDate.Date <= item.EndDate ||
            input.AdditionalEstimatedCost <= 0 ||
            string.IsNullOrWhiteSpace(input.Reason))
        {
            throw new InvalidOperationException(
                "This extension is not valid.");
        }

        if (item.Department.ManagerId == userId &&
            (item.Department.DepartmentHeadId is null ||
             item.Department.DepartmentHeadId == userId))
        {
            throw new InvalidOperationException(
                "A separate Department Head must be assigned before this extension can proceed.");
        }

        if (item.Department.ManagerId is null)
        {
            throw new InvalidOperationException(
                "A Manager must be assigned before this extension can proceed.");
        }

      
        var totalCost =
            item.EstimatedCost + input.AdditionalEstimatedCost;

        item.TripExtensions.Add(
            new TripExtension
            {
                NewEndDate = input.NewEndDate.Date,
                AdditionalEstimatedCost = input.AdditionalEstimatedCost,
                TotalEstimatedCost = totalCost,
                Reason = input.Reason.Trim(),

                Status = item.Department.ManagerId == userId
                    ? ExtensionStatus.PendingDeptHead
                    : ExtensionStatus.PendingManager
            });

        item.Status = TravelRequestStatus.ExtensionRequested;
        item.UpdatedAt = DateTime.UtcNow;

        auditLog.Record(
            item,
            userId,
            AuditActionType.ExtensionRequested,
            input.Reason);

        await requests.SaveAsync();
    }
}
