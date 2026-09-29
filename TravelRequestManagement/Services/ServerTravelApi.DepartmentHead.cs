using TravelManagement.Services.Interfaces;
using TravelManagement.Services.Shared.DTOs;

namespace TravelRequestManagement.Services;


public sealed partial class ServerTravelApi
{
    public Task<IReadOnlyList<TravelRequestDto>> HeadPendingAsync() =>
        Run(DepartmentHeadOnly, (sp, userId) => sp.GetRequiredService<ITravelApprovalService>().GetPendingDepartmentHeadAsync(userId));

    public Task<IReadOnlyList<TravelRequestDto>> HeadHistoryAsync(bool approved) =>
        Run(DepartmentHeadOnly, (sp, userId) => sp.GetRequiredService<ITravelApprovalService>().GetDepartmentHeadHistoryAsync(userId, approved));

    public Task<ApprovalRequestPage> HeadAllRequestsAsync(ApprovalRequestQuery query) =>
        Run(DepartmentHeadOnly, (sp, userId) => sp.GetRequiredService<ITravelApprovalService>().GetAllDepartmentHeadRequestsAsync(userId, query));

    public Task<TravelRequestDto> DeptHeadRequestDetailsAsync(int id) =>
        Run(DepartmentHeadOnly, async (sp, userId) =>
            await sp.GetRequiredService<ITravelApprovalService>().GetForDepartmentHeadViewAsync(userId, id)
            ?? throw new KeyNotFoundException("Request was not found or you do not have access to it."));

    public Task DecideHeadAsync(int id, bool approve, string? reason) =>
        RunVoid(DepartmentHeadOnly, (sp, userId) =>
        {
            var approvals = sp.GetRequiredService<ITravelApprovalService>();
            return approve
                ? approvals.ApproveAsDepartmentHeadAsync(userId, id, reason)
                : approvals.RejectAsDepartmentHeadAsync(userId, id, reason ?? string.Empty);
        });
}
