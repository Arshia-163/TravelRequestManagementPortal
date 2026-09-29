using TravelManagement.Services.Shared.DTOs;

namespace TravelRequestManagement.Client.Services;

public sealed partial class TravelApi
{
    public Task<IReadOnlyList<TravelRequestDto>> MyRequestsAsync() =>
        GetList<TravelRequestDto>("api/requests/mine");

    public Task<TravelRequestDto> GetRequestAsync(int id) =>
        Get<TravelRequestDto>($"api/requests/{id}");

    public Task<TravelRequestDto> CreateAsync(TravelRequestInput input) =>
        Post<TravelRequestDto>("api/requests", input);

    public Task<TravelRequestDto> UpdateDraftAsync(int id, TravelRequestInput input) =>
        Put<TravelRequestDto>($"api/requests/{id}", input);

    public Task SubmitAsync(int id) =>
        PostVoid($"api/requests/{id}/submit", null);

    public Task CancelAsync(int id, string reason) =>
        PostVoid($"api/requests/{id}/cancel", new CancelInput { Reason = reason });

    public Task RequestExtensionAsync(int id, ExtensionInput input) =>
        PostVoid($"api/requests/{id}/extension", input);

    public Task BookAsync(int id, BookingInput input) =>
        PostVoid($"api/requests/{id}/book", input);
}
