using TravelManagement.Data.Entities;
using TravelManagement.Services.Shared.DTOs;

namespace TravelManagement.Data.Repositories.Interfaces;

public interface ITravelRequestRepository
{
    Task<TravelRequest?> GetAsync(int id);
    Task<IReadOnlyList<TravelRequest>> GetAllAsync();
    Task<IReadOnlyList<TravelRequest>> GetForUserAsync(int userId);
    Task<IReadOnlyList<TravelRequest>> GetForManagerAsync(int userId);
    Task<IReadOnlyList<TravelRequest>> GetForDepartmentHeadAsync(int userId);
    Task<IReadOnlyList<TravelRequest>> GetByAuditActionAsync(int userId, bool asDepartmentHead, params TravelManagement.Services.Shared.Enums.AuditActionType[] actions);
    Task<IReadOnlyList<TravelRequest>> GetApprovedAsync();
    Task AddAsync(TravelRequest request);
    Task SaveAsync();


    Task<(IReadOnlyList<TravelRequest> Items, int TotalCount)> GetApprovalScopePageAsync(int userId, bool asDepartmentHead, ApprovalRequestQuery query);
    Task<IReadOnlyList<(int Id, string Name)>> GetApprovalScopeDepartmentsAsync(int userId, bool asDepartmentHead);



}
