using TravelManagement.Services.Shared.DTOs;
using TravelManagement.Services.Shared.Enums;

namespace TravelManagement.Services.Interfaces;

/// <summary>TravelAdmin-only booking of Approved travel (spec section 18).</summary>
public interface IBookingService
{
    Task<IReadOnlyList<TravelRequestDto>> GetApprovedForBookingAsync();

    Task BookAsync(int userId, int requestId, BookingInput input);

    /// <summary>All bookings (Flight, Hotel, etc.) recorded so far for a travel request.</summary>
    Task<IReadOnlyList<BookingDto>> GetBookingsForRequestAsync(int requestId);

    /// <summary>Every booking across every request, for the Travel Admin's global bookings list.</summary>
    Task<IReadOnlyList<AdminBookingDto>> GetAllBookingsAsync();

    /// <summary>Adds a new booking record to an Approved travel request. Multiple bookings per request are allowed.</summary>
    Task<BookingDto> AddBookingAsync(int userId, int requestId, BookingCreateInput input);

    /// <summary>
    /// Moves a booking forward to a new status. Status changes are one-way — a
    /// booking can never move back to an earlier status (e.g. Cancelled -> Pending).
    /// </summary>
    Task<BookingDto> UpdateBookingStatusAsync(int userId, int requestId, int bookingId, BookingStatus newStatus);
}
