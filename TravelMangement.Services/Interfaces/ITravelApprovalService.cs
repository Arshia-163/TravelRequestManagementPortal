using TravelManagement.Services.Shared.DTOs;

namespace TravelManagement.Services.Interfaces;

public interface ITravelApprovalService
{
    Task ApproveAsManagerAsync(int userId, int requestId, string? comment);

    Task RejectAsManagerAsync(int userId, int requestId, string reason);

    Task ApproveAsDepartmentHeadAsync(int userId, int requestId, string? comment);

    Task RejectAsDepartmentHeadAsync(int userId, int requestId, string reason);

    Task<IReadOnlyList<TravelRequestDto>> GetPendingManagerAsync(int userId);

    Task<IReadOnlyList<TravelRequestDto>> GetManagerHistoryAsync(int userId, bool approved);

    
    Task<ApprovalRequestPage> GetAllManagerRequestsAsync(int userId, ApprovalRequestQuery query);


    Task<ApprovalRequestPage> GetAllDepartmentHeadRequestsAsync(int userId, ApprovalRequestQuery query);

    Task<IReadOnlyList<TravelRequestDto>> GetPendingDepartmentHeadAsync(int userId);

    Task<IReadOnlyList<TravelRequestDto>> GetDepartmentHeadHistoryAsync(int userId, bool approved);

    Task<TravelRequestDto?> GetForManagerViewAsync(int userId, int requestId);

    Task<TravelRequestDto?> GetForDepartmentHeadViewAsync(int userId, int requestId);
}
