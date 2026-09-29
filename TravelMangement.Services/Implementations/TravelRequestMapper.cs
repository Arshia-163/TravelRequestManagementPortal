
using TravelManagement.Data.Entities;
using TravelMangement.Services.Mappers;
using TravelManagement.Services.Shared.DTOs;
using TravelManagement.Services.Shared.Enums;

namespace TravelManagement.Services.Implementations;

internal static class TravelRequestMapper
{
    public static TravelRequestDto ToDto(TravelRequest x)
    {
        var history = x.AuditLogs
            .OrderBy(a => a.Timestamp)
            .Select(AuditLogMapper.ToDto)
            .ToList();

        return new TravelRequestDto(
            x.Id,
            $"{x.User?.FirstName} {x.User?.LastName}".Trim(),
            x.Department?.Name ?? "Unassigned",
            x.TravelType,
            x.Destination,
            x.BusinessJustification,
            x.StartDate,
            x.EndDate,
            x.EstimatedCost,
            x.Currency,
            x.Status,
            x.CreatedAt,
            history,
            ToPendingExtensionDto(x),
            x.Source,
            x.AccommodationRequired,
            x.HotelName,
            x.HotelCity,
            x.CheckInDate,
            x.CheckOutDate,
            ToExtensionHistory(x));
    }

    public static TravelRequestDto ToApproverDto(TravelRequest x, int viewerUserId, bool asDepartmentHead)
    {
        var decisionActions = asDepartmentHead
            ? new[] { AuditActionType.DepartmentHeadApproved, AuditActionType.DepartmentHeadRejected }
            : new[] { AuditActionType.ManagerApproved, AuditActionType.ManagerRejected };

        var lastDecision = x.AuditLogs
            .Where(a => decisionActions.Contains(a.Action))
            .OrderByDescending(a => a.Timestamp)
            .FirstOrDefault();

        return ToDto(x) with
        {
            History = [],
            DecidedBy = lastDecision is null
                ? null
                : $"{lastDecision.PerformedByUser.FirstName} {lastDecision.PerformedByUser.LastName}".Trim(),
            DecidedByCurrentUser = lastDecision is not null && lastDecision.PerformedByUserId == viewerUserId,
            DecidedApproved = lastDecision is null
                ? null
                : lastDecision.Action is AuditActionType.ManagerApproved or AuditActionType.DepartmentHeadApproved
        };
    }

    private static PendingExtensionDto? ToPendingExtensionDto(
        TravelRequest x)
    {
        if (x.Status != TravelRequestStatus.ExtensionRequested)
        {
            return null;
        }

        var pending = x.TripExtensions
            .OrderByDescending(e => e.CreatedAt)
            .FirstOrDefault(e =>
                e.Status is ExtensionStatus.PendingManager
                    or ExtensionStatus.PendingDeptHead);

        return pending is null
            ? null
            : new PendingExtensionDto(
                pending.NewEndDate,
                pending.AdditionalEstimatedCost,
                pending.TotalEstimatedCost,
                pending.Reason,
                pending.Status);
    }

    private static List<ExtensionHistoryDto> ToExtensionHistory(
        TravelRequest x)
    {
        var extensions = x.TripExtensions
            .OrderBy(e => e.CreatedAt)
            .ToList();

        if (extensions.Count == 0)
        {
            return [];
        }

      
        var raisedLogs = x.AuditLogs
            .Where(a => a.Action == AuditActionType.ExtensionRequested)
            .OrderBy(a => a.Timestamp)
            .ToList();

        var decisionLogs = x.AuditLogs
            .Where(a => a.Action is AuditActionType.ManagerApproved or AuditActionType.ManagerRejected
                     or AuditActionType.DepartmentHeadApproved or AuditActionType.DepartmentHeadRejected)
            .OrderBy(a => a.Timestamp)
            .ToList();

        var result = new List<ExtensionHistoryDto>(extensions.Count);

        for (var i = 0; i < extensions.Count; i++)
        {
            var extension = extensions[i];

            var windowStart = i < raisedLogs.Count ? raisedLogs[i].Timestamp : extension.CreatedAt;
            var windowEnd = i + 1 < extensions.Count
                ? (i + 1 < raisedLogs.Count ? raisedLogs[i + 1].Timestamp : extensions[i + 1].CreatedAt)
                : DateTime.MaxValue;

            var managerDecision = decisionLogs.FirstOrDefault(a =>
                a.Action is AuditActionType.ManagerApproved or AuditActionType.ManagerRejected
                && a.Timestamp >= windowStart && a.Timestamp < windowEnd);

            var headDecision = decisionLogs.FirstOrDefault(a =>
                a.Action is AuditActionType.DepartmentHeadApproved or AuditActionType.DepartmentHeadRejected
                && a.Timestamp >= windowStart && a.Timestamp < windowEnd);

            var managerName = managerDecision is not null
                ? $"{managerDecision.PerformedByUser.FirstName} {managerDecision.PerformedByUser.LastName}".Trim()
                : extension.Status == ExtensionStatus.PendingManager && x.Department?.Manager is { } currentManager
                    ? $"{currentManager.FirstName} {currentManager.LastName}".Trim()
                    : null;

            var headName = headDecision is not null
                ? $"{headDecision.PerformedByUser.FirstName} {headDecision.PerformedByUser.LastName}".Trim()
                : extension.Status == ExtensionStatus.PendingDeptHead && x.Department?.DepartmentHead is { } currentHead
                    ? $"{currentHead.FirstName} {currentHead.LastName}".Trim()
                    : null;

            result.Add(new ExtensionHistoryDto(
                extension.Id,
                extension.CreatedAt,
                extension.NewEndDate,
                extension.AdditionalEstimatedCost,
                extension.TotalEstimatedCost,
                extension.Status,
                managerName,
                headName));
        }

        result.Reverse();
        return result;
    }
}

