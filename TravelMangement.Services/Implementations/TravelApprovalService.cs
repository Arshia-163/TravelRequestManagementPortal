
using TravelManagement.Data.Entities;
using TravelManagement.Data.Repositories.Interfaces;
using TravelManagement.Services.Interfaces;
using TravelManagement.Services.Shared.DTOs;
using TravelManagement.Services.Shared.Enums;

namespace TravelManagement.Services.Implementations;

public sealed class TravelApprovalService : ITravelApprovalService
{
    private readonly ITravelRequestRepository requests;
    private readonly IAuditLogService auditLog;

    public TravelApprovalService(
        ITravelRequestRepository requests,
        IAuditLogService auditLog)
    {
        this.requests = requests;
        this.auditLog = auditLog;
    }

    public async Task ApproveAsManagerAsync(
        int userId,
        int requestId,
        string? comment)
    {
        var item = await GetForDecisionAsync(requestId);

        if (item.Status == TravelRequestStatus.ExtensionRequested)
        {
            var extension = RequestWorkflowRules.GetPendingExtension(
                item,
                ExtensionStatus.PendingManager);

            RequireAuthorizedManager(
                item,
                userId,
                "approve this extension");

            var extensionIsFinalWithoutHead =
                extension.TotalEstimatedCost > item.Department.TravelApprovalLimit &&
                item.Department.DepartmentHeadId == item.UserId;

            auditLog.Record(
                item,
                userId,
                AuditActionType.ManagerApproved,
                extensionIsFinalWithoutHead ? WithLimitException(comment) : comment);

            if (extension.TotalEstimatedCost <= item.Department.TravelApprovalLimit ||
                item.Department.DepartmentHeadId == item.UserId)
            {
                RequestWorkflowRules.ApplyApprovedExtension(
                    item,
                    extension);
            }
            else
            {
                if (item.Department.DepartmentHeadId is null)
                {
                    throw new InvalidOperationException(
                        "A separate Department Head must be assigned for this extension.");
                }

                extension.Status = ExtensionStatus.PendingDeptHead;
            }

            item.UpdatedAt = DateTime.UtcNow;

            await requests.SaveAsync();

            return;
        }

        if (item.Status != TravelRequestStatus.PendingManager)
        {
            throw new UnauthorizedAccessException(
                "You cannot approve this request.");
        }

        RequireAuthorizedManager(
            item,
            userId,
            "approve this request");


        var overLimit = item.EstimatedCost > item.Department.TravelApprovalLimit;
        var requesterIsDepartmentHead = item.Department.DepartmentHeadId == item.UserId;
        var finalWithoutHead = overLimit && requesterIsDepartmentHead;

        auditLog.Record(
            item,
            userId,
            AuditActionType.ManagerApproved,
            finalWithoutHead ? WithLimitException(comment) : comment);

        item.Status = !overLimit || requesterIsDepartmentHead
            ? TravelRequestStatus.Approved
            : item.Department.DepartmentHeadId is not null
                ? TravelRequestStatus.PendingDeptHead
                : throw new InvalidOperationException(
                    "A separate Department Head must be assigned for this approval.");

        item.UpdatedAt = DateTime.UtcNow;

        await requests.SaveAsync();
    }

    public async Task RejectAsManagerAsync(
        int userId,
        int requestId,
        string reason)
    {
        RequestWorkflowRules.RequireReason(reason);

        var item = await GetForDecisionAsync(requestId);

        if (item.Status == TravelRequestStatus.ExtensionRequested)
        {
            var extension = RequestWorkflowRules.GetPendingExtension(
                item,
                ExtensionStatus.PendingManager);

            RequireAuthorizedManager(
                item,
                userId,
                "reject this extension");

            extension.Status = ExtensionStatus.Rejected;
            extension.UpdatedAt = DateTime.UtcNow;

            item.Status = TravelRequestStatus.Approved;

            auditLog.Record(
                item,
                userId,
                AuditActionType.ManagerRejected,
                reason);

            await requests.SaveAsync();

            return;
        }

        if (item.Status != TravelRequestStatus.PendingManager)
        {
            throw new UnauthorizedAccessException(
                "You cannot reject this request.");
        }

        RequireAuthorizedManager(
            item,
            userId,
            "reject this request");

        item.Status = TravelRequestStatus.Rejected;

        auditLog.Record(
            item,
            userId,
            AuditActionType.ManagerRejected,
            reason);

        await requests.SaveAsync();
    }

    public async Task ApproveAsDepartmentHeadAsync(
        int userId,
        int requestId,
        string? comment)
    {
        var item = await GetForDecisionAsync(requestId);

        if (item.Status == TravelRequestStatus.ExtensionRequested)
        {
            var extension = RequestWorkflowRules.GetPendingExtension(
                item,
                ExtensionStatus.PendingDeptHead);

            RequireAuthorizedDepartmentHead(
                item,
                userId,
                "approve this extension");

            RequestWorkflowRules.ApplyApprovedExtension(
                item,
                extension);

            auditLog.Record(
                item,
                userId,
                AuditActionType.DepartmentHeadApproved,
                comment);

            await requests.SaveAsync();

            return;
        }

        RequireAwaitingDepartmentHead(
            item,
            userId,
            "decide this request");

        item.Status = TravelRequestStatus.Approved;

        auditLog.Record(
            item,
            userId,
            AuditActionType.DepartmentHeadApproved,
            comment);

        await requests.SaveAsync();
    }

    public async Task RejectAsDepartmentHeadAsync(
        int userId,
        int requestId,
        string reason)
    {
        RequestWorkflowRules.RequireReason(reason);

        var item = await GetForDecisionAsync(requestId);

        if (item.Status == TravelRequestStatus.ExtensionRequested)
        {
            var extension = RequestWorkflowRules.GetPendingExtension(
                item,
                ExtensionStatus.PendingDeptHead);

            RequireAuthorizedDepartmentHead(
                item,
                userId,
                "reject this extension");

            extension.Status = ExtensionStatus.Rejected;
            extension.UpdatedAt = DateTime.UtcNow;

            item.Status = TravelRequestStatus.Approved;

            auditLog.Record(
                item,
                userId,
                AuditActionType.DepartmentHeadRejected,
                reason);

            await requests.SaveAsync();

            return;
        }

        RequireAwaitingDepartmentHead(
            item,
            userId,
            "decide this request");

        item.Status = TravelRequestStatus.Rejected;

        auditLog.Record(
            item,
            userId,
            AuditActionType.DepartmentHeadRejected,
            reason);

        await requests.SaveAsync();
    }

    public async Task<IReadOnlyList<TravelRequestDto>> GetPendingManagerAsync(
        int userId)
    {
        return (await requests.GetForManagerAsync(userId))
            .Select(TravelRequestMapper.ToDto)
            .ToList();
    }


    public async Task<IReadOnlyList<TravelRequestDto>> GetManagerHistoryAsync(
        int userId,
        bool approved)
    {
        var action = approved
            ? AuditActionType.ManagerApproved
            : AuditActionType.ManagerRejected;

        var items = await requests.GetByAuditActionAsync(
            userId,
            false,
            action);

        return items
            .Select(TravelRequestMapper.ToDto)
            .ToList();
    }

    public async Task<IReadOnlyList<TravelRequestDto>> GetPendingDepartmentHeadAsync(
        int userId)
    {
        return (await requests.GetForDepartmentHeadAsync(userId))
            .Select(TravelRequestMapper.ToDto)
            .ToList();
    }

    public async Task<IReadOnlyList<TravelRequestDto>> GetDepartmentHeadHistoryAsync(
        int userId,
        bool approved)
    {
        var action = approved
            ? AuditActionType.DepartmentHeadApproved
            : AuditActionType.DepartmentHeadRejected;

        var items = await requests.GetByAuditActionAsync(
            userId,
            true,
            action);

        return items
            .Select(TravelRequestMapper.ToDto)
            .ToList();
    }

    public Task<ApprovalRequestPage> GetAllManagerRequestsAsync(
        int userId,
        ApprovalRequestQuery query) => GetAllAsync(userId, false, query);

    public Task<ApprovalRequestPage> GetAllDepartmentHeadRequestsAsync(
        int userId,
        ApprovalRequestQuery query) => GetAllAsync(userId, true, query);

    private async Task<ApprovalRequestPage> GetAllAsync(
        int userId,
        bool asDepartmentHead,
        ApprovalRequestQuery query)
    {
        query ??= new ApprovalRequestQuery();

        var (items, total) = await requests.GetApprovalScopePageAsync(
            userId,
            asDepartmentHead,
            query);

        var departments = await requests.GetApprovalScopeDepartmentsAsync(
            userId,
            asDepartmentHead);

        var pageSize = Math.Clamp(query.PageSize, 1, ApprovalRequestQuery.MaxPageSize);
        var lastPage = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize));
        var page = Math.Min(Math.Max(1, query.Page), lastPage);

        return new ApprovalRequestPage(
            items.Select(x => TravelRequestMapper.ToApproverDto(x, userId, asDepartmentHead)).ToList(),
            total,
            page,
            pageSize,
            departments.Select(d => new DepartmentOptionDto(d.Id, d.Name)).ToList());
    }

    public async Task<TravelRequestDto?> GetForManagerViewAsync(
        int userId,
        int requestId)
    {
        var item = await requests.GetAsync(requestId);

        return item is null || item.Department.ManagerId != userId
            ? null
            : TravelRequestMapper.ToApproverDto(item, userId, false);
    }

    public async Task<TravelRequestDto?> GetForDepartmentHeadViewAsync(
        int userId,
        int requestId)
    {
        var item = await requests.GetAsync(requestId);

        return item is null || item.Department.DepartmentHeadId != userId
            ? null
            : TravelRequestMapper.ToApproverDto(item, userId, true);
    }

    private const string LimitExceptionNote =
        "Approved above this department's travel approval limit: the requester is the Department Head of this department, so there is no separate Department Head to escalate to.";

    private static string WithLimitException(string? comment) =>
        string.IsNullOrWhiteSpace(comment)
            ? LimitExceptionNote
            : $"{comment.Trim()} — {LimitExceptionNote}";

    private async Task<TravelRequest> GetForDecisionAsync(
        int requestId)
    {
        return await requests.GetAsync(requestId)
            ?? throw new InvalidOperationException(
                "Request was not found.");
    }

    private static void RequireAuthorizedManager(
        TravelRequest item,
        int userId,
        string action)
    {
        if (item.Department.ManagerId != userId ||
            RequestWorkflowRules.IsSelfApproval(item, userId))
        {
            throw new UnauthorizedAccessException(
                $"You cannot {action}.");
        }
    }

    private static void RequireAuthorizedDepartmentHead(
        TravelRequest item,
        int userId,
        string action)
    {
        if (item.Department.DepartmentHeadId != userId ||
            RequestWorkflowRules.IsSelfApproval(item, userId))
        {
            throw new UnauthorizedAccessException(
                $"You cannot {action}.");
        }
    }

    private static void RequireAwaitingDepartmentHead(
        TravelRequest item,
        int userId,
        string action)
    {
        if (item.Status != TravelRequestStatus.PendingDeptHead)
        {
            throw new UnauthorizedAccessException(
                $"You cannot {action}.");
        }

        RequireAuthorizedDepartmentHead(
            item,
            userId,
            action);
    }
}

