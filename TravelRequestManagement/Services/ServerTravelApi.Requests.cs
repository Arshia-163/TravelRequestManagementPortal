using TravelManagement.Services.Interfaces;
using TravelManagement.Services.Shared.Constants;
using TravelManagement.Services.Shared.DTOs;

namespace TravelRequestManagement.Services;


public sealed partial class ServerTravelApi
{
    public Task<IReadOnlyList<TravelRequestDto>> MyRequestsAsync() =>
        Run(null, (sp, userId) => sp.GetRequiredService<ITravelRequestService>().GetMyAsync(userId));

    public Task<TravelRequestDto> GetRequestAsync(int id) =>
        Run(null, async (sp, userId) =>
            await sp.GetRequiredService<ITravelRequestService>().GetAsync(id, userId)
            ?? throw new KeyNotFoundException("Request was not found or you do not have access to it."));

    public Task<TravelRequestDto> CreateAsync(TravelRequestInput input) =>
        Run(RoleNames.CanCreateTravelRequestRoles,
            (sp, userId) => sp.GetRequiredService<ITravelRequestService>().CreateDraftAsync(userId, input));

    public Task<TravelRequestDto> UpdateDraftAsync(int id, TravelRequestInput input) =>
        Run(RoleNames.CanCreateTravelRequestRoles,
            (sp, userId) => sp.GetRequiredService<ITravelRequestService>().UpdateDraftAsync(userId, id, input));

    public Task SubmitAsync(int id) =>
        RunVoid(RoleNames.CanCreateTravelRequestRoles,
            (sp, userId) => sp.GetRequiredService<ITravelRequestService>().SubmitAsync(userId, id));

    public Task CancelAsync(int id, string reason) =>
        RunVoid(RoleNames.CanCreateTravelRequestRoles,
            (sp, userId) => sp.GetRequiredService<ICancellationService>().CancelAsync(userId, id, reason));

    public Task RequestExtensionAsync(int id, ExtensionInput input) =>
        RunVoid(RoleNames.CanCreateTravelRequestRoles,
            (sp, userId) => sp.GetRequiredService<ITripExtensionService>().RequestExtensionAsync(userId, id, input));

    public Task BookAsync(int id, BookingInput input) =>
        RunVoid(TravelAdminOnly,
            (sp, userId) => sp.GetRequiredService<IBookingService>().BookAsync(userId, id, input));
}
