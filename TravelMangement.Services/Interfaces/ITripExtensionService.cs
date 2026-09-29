using TravelManagement.Services.Shared.DTOs;

namespace TravelManagement.Services.Interfaces;

/// <summary>Requesting a trip extension (spec section 16). Approving/rejecting it is ITravelApprovalService's job.</summary>
public interface ITripExtensionService
{
    Task RequestExtensionAsync(int userId, int requestId, ExtensionInput input);
}
