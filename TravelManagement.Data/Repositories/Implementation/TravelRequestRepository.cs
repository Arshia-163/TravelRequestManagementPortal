using Microsoft.EntityFrameworkCore;
using TravelManagement.Data.DbContext;
using TravelManagement.Data.Entities;
using TravelManagement.Data.Repositories.Interfaces;
using TravelManagement.Services.Shared.DTOs;
using TravelManagement.Services.Shared.Enums;

namespace TravelManagement.Data.Repositories.Implementation;

public sealed class TravelRequestRepository(ApplicationDbContext db)
    : ITravelRequestRepository
{
    private IQueryable<TravelRequest> DetailQuery() =>
        db.TravelRequests
            .AsSplitQuery()
            .Include(x => x.User)
            .Include(x => x.Department)
                .ThenInclude(x => x.Manager)
            .Include(x => x.Department)
                .ThenInclude(x => x.DepartmentHead)
            .Include(x => x.AuditLogs)
                .ThenInclude(x => x.PerformedByUser)
            .Include(x => x.TripExtensions)
            .Include(x => x.Bookings);

    public Task<TravelRequest?> GetAsync(
        int id) =>
        DetailQuery().SingleOrDefaultAsync(x => x.Id == id);

    public async Task<IReadOnlyList<TravelRequest>> GetAllAsync() =>
        await DetailQuery()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

    public async Task<IReadOnlyList<TravelRequest>> GetForUserAsync(
        int userId) =>
        await DetailQuery()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

    public async Task<IReadOnlyList<TravelRequest>> GetForManagerAsync(
        int userId) =>
        await DetailQuery()
            .Where(x =>
                x.Department.ManagerId == userId &&
                x.UserId != userId &&
                (
                    x.Status == TravelRequestStatus.PendingManager ||
                    (
                        x.Status == TravelRequestStatus.ExtensionRequested &&
                        x.TripExtensions.Any(
                            e => e.Status == ExtensionStatus.PendingManager)
                    )
                ))
            .ToListAsync();

    public async Task<IReadOnlyList<TravelRequest>> GetForDepartmentHeadAsync(
        int userId) =>
        await DetailQuery()
            .Where(x =>
                x.Department.DepartmentHeadId == userId &&
                x.UserId != userId &&
                (
                    x.Status == TravelRequestStatus.PendingDeptHead ||
                    (
                        x.Status == TravelRequestStatus.ExtensionRequested &&
                        x.TripExtensions.Any(
                            e => e.Status == ExtensionStatus.PendingDeptHead)
                    )
                ))
            .ToListAsync();

    public async Task<IReadOnlyList<TravelRequest>> GetByAuditActionAsync(
        int userId,
        bool asDepartmentHead,
        params AuditActionType[] actions) =>
        await DetailQuery()
            .Where(x =>
                (
                    asDepartmentHead
                        ? x.Department.DepartmentHeadId == userId
                        : x.Department.ManagerId == userId
                ) &&
                x.AuditLogs.Any(a => actions.Contains(a.Action)))
            .ToListAsync();

    private IQueryable<TravelRequest> ApprovalScope(
        int userId,
        bool asDepartmentHead)
    {
        var pendingExtensionStatus = asDepartmentHead
            ? ExtensionStatus.PendingDeptHead
            : ExtensionStatus.PendingManager;

        var pendingStatus = asDepartmentHead
            ? TravelRequestStatus.PendingDeptHead
            : TravelRequestStatus.PendingManager;

        var submitted = db.TravelRequests.Where(x =>
            (
                x.UserId != userId ||
                !(
                    x.Status == pendingStatus ||
                    (
                        x.Status == TravelRequestStatus.ExtensionRequested &&
                        x.TripExtensions.Any(
                            e => e.Status == pendingExtensionStatus)
                    )
                )
            ) &&
            x.AuditLogs.Any(a => a.Action == AuditActionType.Submitted));

        if (!asDepartmentHead)
        {
            return submitted.Where(
                x => x.Department.ManagerId == userId);
        }

        return submitted.Where(x =>
            x.Department.DepartmentHeadId == userId &&
            (
                x.Status == TravelRequestStatus.PendingDeptHead ||
                x.AuditLogs.Any(a =>
                    a.Action == AuditActionType.DepartmentHeadApproved ||
                    a.Action == AuditActionType.DepartmentHeadRejected) ||
                x.TripExtensions.Any(
                    e => e.Status == ExtensionStatus.PendingDeptHead) ||
                (
                    x.Status == TravelRequestStatus.Cancelled &&
                    x.EstimatedCost > x.Department.TravelApprovalLimit &&
                    x.AuditLogs.Any(
                        a => a.Action == AuditActionType.ManagerApproved)
                ) ||
                (
                    x.Status == TravelRequestStatus.Cancelled &&
                    x.UserId == x.Department.ManagerId
                )
            ));
    }

    public async Task<IReadOnlyList<(int Id, string Name)>>
        GetApprovalScopeDepartmentsAsync(
            int userId,
            bool asDepartmentHead)
    {
        var departments = db.Departments.Where(d =>
            asDepartmentHead
                ? d.DepartmentHeadId == userId
                : d.ManagerId == userId);

        var rows = await departments
            .OrderBy(d => d.Name)
            .Select(d => new { d.Id, d.Name })
            .ToListAsync();

        return rows
            .Select(d => (d.Id, d.Name))
            .ToList();
    }

    public async Task<(IReadOnlyList<TravelRequest> Items, int TotalCount)> GetApprovalScopePageAsync(
            int userId,
            bool asDepartmentHead,
            ApprovalRequestQuery query)
    {
        var filtered = ApprovalScope(userId, asDepartmentHead);

        if (query.Status is { } status)
        {
            filtered = filtered.Where(x => x.Status == status);
        }

        if (query.DepartmentId is { } departmentId)
        {
            filtered = filtered.Where(
                x => x.DepartmentId == departmentId);
        }

        if (query.TravelType is { } travelType)
        {
            filtered = filtered.Where(
                x => x.TravelType == travelType);
        }

        if (query.DecidedBy is "me" or "previous")
        {
            var decisionActions = asDepartmentHead
                ? new[]
                {
                    AuditActionType.DepartmentHeadApproved,
                    AuditActionType.DepartmentHeadRejected
                }
                : new[]
                {
                    AuditActionType.ManagerApproved,
                    AuditActionType.ManagerRejected
                };

            filtered = query.DecidedBy == "me"
                ? filtered.Where(x =>
                    x.AuditLogs.Any(a =>
                        decisionActions.Contains(a.Action) &&
                        a.PerformedByUserId == userId))
                : filtered.Where(x =>
                    x.AuditLogs.Any(a =>
                        decisionActions.Contains(a.Action) &&
                        a.PerformedByUserId != userId));
        }

        var search = query.Search?.Trim();

        if (!string.IsNullOrEmpty(search))
        {

            var numberText = search.StartsWith(
                "TR",
                StringComparison.OrdinalIgnoreCase)
                    ? search[2..].TrimStart('-', ' ')
                    : search;

            var requestNumber = int.TryParse(numberText, out var n)
                ? n
                : -1;

            filtered = filtered.Where(x =>
                (x.User.FirstName + " " + x.User.LastName)
                    .Contains(search) ||
                x.Destination.Contains(search) ||
                x.Source.Contains(search) ||
                x.Department.Name.Contains(search) ||
                x.Id == requestNumber);
        }

        var total = await filtered.CountAsync();

        var ordered = (query.SortBy?.Trim().ToLowerInvariant()) switch
        {
            "oldest" => filtered
                .OrderBy(x => x.CreatedAt)
                .ThenBy(x => x.Id),

            "employee" => filtered
                .OrderBy(x => x.User.FirstName)
                .ThenBy(x => x.User.LastName)
                .ThenByDescending(x => x.Id),

            "destination" => filtered
                .OrderBy(x => x.Destination)
                .ThenByDescending(x => x.Id),

            "startdate" => filtered
                .OrderByDescending(x => x.StartDate)
                .ThenByDescending(x => x.Id),

            "cost_high" => filtered
                .OrderByDescending(x => x.EstimatedCost)
                .ThenByDescending(x => x.Id),

            "cost_low" => filtered
                .OrderBy(x => x.EstimatedCost)
                .ThenByDescending(x => x.Id),

            "status" => filtered
                .OrderBy(x => x.Status)
                .ThenByDescending(x => x.Id),

            _ => filtered
                .OrderByDescending(x => x.CreatedAt)
                .ThenByDescending(x => x.Id)
        };

        var pageSize = Math.Clamp(
            query.PageSize,
            1,
            ApprovalRequestQuery.MaxPageSize);

        var page = Math.Max(1, query.Page);

        var lastPage = Math.Max(
            1,
            (int)Math.Ceiling(total / (double)pageSize));

        page = Math.Min(page, lastPage);

        var ids = await ordered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => x.Id)
            .ToListAsync();

        if (ids.Count == 0)
        {
            return (Array.Empty<TravelRequest>(), total);
        }

        var loaded = await DetailQuery()
            .Where(x => ids.Contains(x.Id))
            .ToListAsync();

        var byId = loaded.ToDictionary(x => x.Id);

        return (
            ids.Select(id => byId[id]).ToList(),
            total
        );
    }

    public async Task<IReadOnlyList<TravelRequest>> GetApprovedAsync() =>
        await DetailQuery()
            .Where(x => x.Status == TravelRequestStatus.Approved)
            .OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt)
            .ToListAsync();

    public Task AddAsync(
        TravelRequest request) =>
        db.TravelRequests.AddAsync(request).AsTask();

    public Task SaveAsync() =>
        db.SaveChangesAsync();
}