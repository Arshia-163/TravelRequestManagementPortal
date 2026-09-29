namespace TravelManagement.Services.Shared.DTOs;

public sealed record DepartmentOptionDto(int Id, string Name);


public sealed record ApprovalRequestPage(
    IReadOnlyList<TravelRequestDto> Items,
    int TotalCount,
    int Page,
    int PageSize,
    IReadOnlyList<DepartmentOptionDto> Departments)
{
    public int TotalPages => PageSize <= 0 ? 1 : Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
}
