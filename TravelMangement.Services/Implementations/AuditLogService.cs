
using TravelManagement.Data.Entities;
using TravelManagement.Data.Repositories.Interfaces;
using TravelManagement.Services.Interfaces;
using TravelManagement.Services.Shared.DTOs;
using TravelManagement.Services.Shared.Enums;
using TravelMangement.Services.Mappers;

namespace TravelManagement.Services.Implementations;

public sealed class AuditLogService : IAuditLogService
{
    private readonly IAuditLogRepository auditLogs;

    public AuditLogService(IAuditLogRepository auditLogs)
    {
        this.auditLogs = auditLogs;
    }

    public AuditLog Record(
        TravelRequest request,
        int performedByUserId,
        AuditActionType action,
        string? comment = null)
    {
        var entry = new AuditLog
        {
            TravelRequestId = request.Id,
            PerformedByUserId = performedByUserId,
            Action = action,
            Comment = string.IsNullOrWhiteSpace(comment)
                ? null
                : comment.Trim()
        };


        request.AuditLogs.Add(entry);

        return entry;
    }

    public async Task<IReadOnlyList<AuditLogDto>> GetHistoryAsync(int travelRequestId)
    {
        var logs = await auditLogs.GetForRequestAsync(travelRequestId);

        return logs
            .Select(AuditLogMapper.ToDto)
            .ToList();
    }
}
