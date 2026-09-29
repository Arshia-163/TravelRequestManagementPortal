using TravelManagement.Services.Shared.DTOs;
using TravelManagement.Services.Shared.Enums;

namespace TravelRequestManagement.Client.Services;

public sealed partial class TravelApi
{
    public Task<IReadOnlyList<TravelRequestDto>> ApprovedForBookingAsync() =>
        GetList<TravelRequestDto>("api/admin/approved-requests");

    public Task<IReadOnlyList<AdminBookingDto>> AllBookingsAsync() =>
        GetList<AdminBookingDto>("api/admin/bookings");

    public Task<IReadOnlyList<BookingDto>> GetBookingsAsync(int requestId) =>
        GetList<BookingDto>($"api/admin/requests/{requestId}/bookings");

    public Task<BookingDto> AddBookingAsync(int requestId, BookingCreateInput input) =>
        Post<BookingDto>($"api/admin/requests/{requestId}/bookings", input);

    public Task<BookingDto> UpdateBookingStatusAsync(int requestId, int bookingId, BookingStatus status) =>
        Put<BookingDto>($"api/admin/requests/{requestId}/bookings/{bookingId}/status", new BookingStatusUpdateInput { Status = status });
}
