using TravelManagement.Services.Shared.DTOs;

namespace TravelRequestManagement.Client.Services;

public sealed partial class TravelApi
{
    public Task<IReadOnlyList<TravelRequestDto>> HeadPendingAsync() =>
        GetList<TravelRequestDto>("api/department-head/pending");

    public Task<IReadOnlyList<TravelRequestDto>> HeadHistoryAsync(bool approved) =>
        GetList<TravelRequestDto>($"api/department-head/{(approved ? "approved" : "rejected")}");

    public Task<ApprovalRequestPage> HeadAllRequestsAsync(ApprovalRequestQuery query) =>
        Get<ApprovalRequestPage>($"api/department-head/all{ApprovalQueryString.Build(query)}");

    public Task<TravelRequestDto> DeptHeadRequestDetailsAsync(int id) =>
        Get<TravelRequestDto>($"api/department-head/{id}");

    public Task DecideHeadAsync(int id, bool approve, string? reason) =>
        PostVoid($"api/department-head/{id}/{(approve ? "approve" : "reject")}", new DecisionInput { Reason = reason });
}
