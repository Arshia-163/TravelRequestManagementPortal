namespace TravelManagement.Services.Interfaces;

/// <summary>Cancellation of a travel request (spec section 17).</summary>
public interface ICancellationService
{
    Task CancelAsync(int userId, int requestId, string reason);
}
