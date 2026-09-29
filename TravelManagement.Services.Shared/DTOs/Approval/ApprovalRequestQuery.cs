using TravelManagement.Services.Shared.Enums;

namespace TravelManagement.Services.Shared.DTOs;


public sealed class ApprovalRequestQuery
{
    public const int MaxPageSize = 100;


    public string? Search { get; set; }

    public TravelRequestStatus? Status { get; set; }

    public int? DepartmentId { get; set; }

    public TravelType? TravelType { get; set; }

    public string? SortBy { get; set; } = "newest";

   
    public string? DecidedBy { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 10;
}
