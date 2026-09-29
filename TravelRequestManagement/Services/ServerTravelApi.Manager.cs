using TravelManagement.Services.Interfaces;
using TravelManagement.Services.Shared.DTOs;

namespace TravelRequestManagement.Services;

public sealed partial class ServerTravelApi
{
    public Task<IReadOnlyList<TravelRequestDto>> ManagerPendingAsync() =>
        Run(ManagerOnly, (sp, userId) => sp.GetRequiredService<ITravelApprovalService>().GetPendingManagerAsync(userId));

    public Task<IReadOnlyList<TravelRequestDto>> ManagerHistoryAsync(bool approved) =>
        Run(ManagerOnly, (sp, userId) => sp.GetRequiredService<ITravelApprovalService>().GetManagerHistoryAsync(userId, approved));

    public Task<ApprovalRequestPage> ManagerAllRequestsAsync(ApprovalRequestQuery query) =>
        Run(ManagerOnly, (sp, userId) => sp.GetRequiredService<ITravelApprovalService>().GetAllManagerRequestsAsync(userId, query));

    public Task<TravelRequestDto> ManagerRequestDetailsAsync(int id) =>
        Run(ManagerOnly, async (sp, userId) =>
            await sp.GetRequiredService<ITravelApprovalService>().GetForManagerViewAsync(userId, id)
            ?? throw new KeyNotFoundException("Request was not found or you do not have access to it."));

    public Task DecideManagerAsync(int id, bool approve, string? reason) =>
        RunVoid(ManagerOnly, (sp, userId) =>
        {
            var approvals = sp.GetRequiredService<ITravelApprovalService>();
            return approve
                ? approvals.ApproveAsManagerAsync(userId, id, reason)
                : approvals.RejectAsManagerAsync(userId, id, reason ?? string.Empty);
        });
}
