using TravelManagement.Services.Interfaces;
using TravelManagement.Services.Shared.DTOs;
using TravelManagement.Services.Shared.Enums;

namespace TravelRequestManagement.Services;


public sealed partial class ServerTravelApi
{
    public Task<IReadOnlyList<TravelRequestDto>> ApprovedForBookingAsync() =>
        Run(TravelAdminOnly, (sp, _) => sp.GetRequiredService<IBookingService>().GetApprovedForBookingAsync());

    public Task<IReadOnlyList<AdminBookingDto>> AllBookingsAsync() =>
        Run(TravelAdminOnly, (sp, _) => sp.GetRequiredService<IBookingService>().GetAllBookingsAsync());

    public Task<IReadOnlyList<BookingDto>> GetBookingsAsync(int requestId) =>
        Run(TravelAdminOnly, (sp, _) => sp.GetRequiredService<IBookingService>().GetBookingsForRequestAsync(requestId));

    public Task<BookingDto> AddBookingAsync(int requestId, BookingCreateInput input) =>
        Run(TravelAdminOnly, (sp, userId) => sp.GetRequiredService<IBookingService>().AddBookingAsync(userId, requestId, input));

    public Task<BookingDto> UpdateBookingStatusAsync(int requestId, int bookingId, BookingStatus status) =>
        Run(TravelAdminOnly, (sp, userId) => sp.GetRequiredService<IBookingService>().UpdateBookingStatusAsync(userId, requestId, bookingId, status));
}
