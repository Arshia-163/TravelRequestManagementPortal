using TravelManagement.Services.Shared.DTOs;

namespace TravelRequestManagement.Client.Services;


public sealed partial class TravelApi
{
    public Task<IReadOnlyList<TravelRequestDto>> ManagerPendingAsync() =>
        GetList<TravelRequestDto>("api/manager/pending");

    public Task<IReadOnlyList<TravelRequestDto>> ManagerHistoryAsync(bool approved) =>
        GetList<TravelRequestDto>($"api/manager/{(approved ? "approved" : "rejected")}");

    public Task<ApprovalRequestPage> ManagerAllRequestsAsync(ApprovalRequestQuery query) =>
        Get<ApprovalRequestPage>($"api/manager/all{ApprovalQueryString.Build(query)}");

    public Task<TravelRequestDto> ManagerRequestDetailsAsync(int id) =>
        Get<TravelRequestDto>($"api/manager/{id}");

    public Task DecideManagerAsync(int id, bool approve, string? reason) =>
        PostVoid($"api/manager/{id}/{(approve ? "approve" : "reject")}", new DecisionInput { Reason = reason });
}
