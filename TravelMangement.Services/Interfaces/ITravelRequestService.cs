using TravelManagement.Services.Shared.DTOs;

namespace TravelManagement.Services.Interfaces;

/// <summary>
/// Core lifecycle of a request that belongs to the requester: draft creation/
/// editing, submission into the approval workflow, and reading. Approval,
/// extension, cancellation and booking are separate services (single
/// responsibility) even though they all operate on the same TravelRequest.
/// </summary>
public interface ITravelRequestService
{
    Task<IReadOnlyList<TravelRequestDto>> GetMyAsync(int userId);

    /// <summary>Every travel request across every employee, for the Travel Admin's "see all history" list.</summary>
    Task<IReadOnlyList<TravelRequestDto>> GetAllAsync();

    Task<TravelRequestDto?> GetAsync(int id, int userId, bool canViewAll = false);

    Task<TravelRequestDto> CreateDraftAsync(int userId, TravelRequestInput input);

    Task<TravelRequestDto> UpdateDraftAsync(int userId, int requestId, TravelRequestInput input);

    Task<TravelRequestDto> SubmitAsync(int userId, int requestId);
}
