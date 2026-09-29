using TravelManagement.Services.Shared.DTOs;

namespace TravelRequestManagement.Client.Services;


internal static class ApprovalQueryString
{
    public static string Build(ApprovalRequestQuery q)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(q.Search)) parts.Add($"search={Uri.EscapeDataString(q.Search.Trim())}");
        if (q.Status is { } status) parts.Add($"status={status}");
        if (q.DepartmentId is { } departmentId) parts.Add($"departmentId={departmentId}");
        if (q.TravelType is { } travelType) parts.Add($"travelType={travelType}");
        if (!string.IsNullOrWhiteSpace(q.SortBy)) parts.Add($"sortBy={Uri.EscapeDataString(q.SortBy)}");
        if (!string.IsNullOrWhiteSpace(q.DecidedBy)) parts.Add($"decidedBy={Uri.EscapeDataString(q.DecidedBy)}");
        parts.Add($"page={q.Page}");
        parts.Add($"pageSize={q.PageSize}");
        return "?" + string.Join("&", parts);
    }
}
