
using TravelManagement.Data.Entities;
using TravelManagement.Data.Repositories.Interfaces;
using TravelManagement.Services.Interfaces;
using TravelManagement.Services.Shared.DTOs;
using TravelManagement.Services.Shared.Enums;

namespace TravelManagement.Services.Implementations;

public sealed class BookingService : IBookingService
{
    private readonly ITravelRequestRepository requests;
    private readonly IBookingRepository bookings;
    private readonly IAuditLogService auditLog;

    public BookingService(
        ITravelRequestRepository requests,
        IBookingRepository bookings,
        IAuditLogService auditLog)
    {
        this.requests = requests;
        this.bookings = bookings;
        this.auditLog = auditLog;
    }

    public async Task<IReadOnlyList<TravelRequestDto>> GetApprovedForBookingAsync()
    {
        var items = await requests.GetApprovedAsync();
        await RequestWorkflowRules.CompleteElapsedAsync(requests, items);
        return items
            .Where(x => x.Status == TravelRequestStatus.Approved)
            .Select(TravelRequestMapper.ToDto)
            .ToList();
    }

    public async Task BookAsync(
        int userId,
        int requestId,
        BookingInput input)
    {
        var item = await requests.GetAsync(requestId)
            ?? throw new InvalidOperationException("Request was not found.");

     
        if (item.Status != TravelRequestStatus.Approved ||
            string.IsNullOrWhiteSpace(input.BookingReference))
        {
            throw new InvalidOperationException(
                "Only approved travel with a booking reference can be booked.");
        }

        if (await bookings.ExistsForRequestAsync(requestId))
        {
            throw new InvalidOperationException(
                "This approved travel request has already been booked.");
        }

        await bookings.AddAsync(new Booking
        {
            TravelRequestId = requestId,
            BookedByUserId = userId,
            BookingReference = input.BookingReference.Trim(),
            BookingDate = DateTime.UtcNow,
            Notes = input.Notes
        });

        auditLog.Record(
            item,
            userId,
            AuditActionType.Booked,
            input.BookingReference.Trim());

        await bookings.SaveAsync();
    }

    public async Task<IReadOnlyList<BookingDto>> GetBookingsForRequestAsync(
        int requestId)
    {
        var item = await requests.GetAsync(requestId)
            ?? throw new InvalidOperationException("Request was not found.");

        var items = await bookings.GetForRequestAsync(requestId);

        return items
            .Select(ToDto)
            .ToList();
    }

    public async Task<IReadOnlyList<AdminBookingDto>> GetAllBookingsAsync()
    {
        return (await bookings.GetAllAsync())
            .Select(ToAdminDto)
            .ToList();
    }

    public async Task<BookingDto> AddBookingAsync(
        int userId,
        int requestId,
        BookingCreateInput input)
    {
        var item = await requests.GetAsync(requestId)
            ?? throw new InvalidOperationException("Request was not found.");

        if (item.Status != TravelRequestStatus.Approved)
        {
            throw new InvalidOperationException(
                "Only approved travel can be booked.");
        }

        if (string.IsNullOrWhiteSpace(input.Provider) ||
            string.IsNullOrWhiteSpace(input.BookingReference))
        {
            throw new InvalidOperationException(
                "Provider and reference number are required.");
        }

        var booking = new Booking
        {
            TravelRequestId = requestId,
            BookedByUserId = userId,
            Type = input.Type,
            Provider = input.Provider.Trim(),
            BookingReference = input.BookingReference.Trim(),
            ActualCost = input.ActualCost,
            Status = input.Status,
            BookingDate = DateTime.UtcNow,
            Notes = string.IsNullOrWhiteSpace(input.Notes)
                ? null
                : input.Notes.Trim()
        };

        await bookings.AddAsync(booking);

        auditLog.Record(
            item,
            userId,
            AuditActionType.Booked,
            $"{input.Type} booking with {booking.Provider} ({booking.BookingReference}) created as {booking.Status}.");

        await bookings.SaveAsync();

       
        var saved = await bookings.GetByIdAsync(booking.Id) ?? booking;

        return ToDto(saved);
    }

    public async Task<BookingDto> UpdateBookingStatusAsync(
        int userId,
        int requestId,
        int bookingId,
        BookingStatus newStatus)
    {
        var booking = await bookings.GetByIdAsync(bookingId)
            ?? throw new InvalidOperationException("Booking was not found.");

        if (booking.TravelRequestId != requestId)
        {
            throw new InvalidOperationException(
                "Booking does not belong to this travel request.");
        }

    
       
        if (!BookingWorkflowRules.CanTransition(booking.Status, newStatus))
        {
            throw new InvalidOperationException(
                $"Booking status cannot move from {booking.Status} back to {newStatus}.");
        }

        var previousStatus = booking.Status;
        booking.Status = newStatus;

        auditLog.Record(
            booking.TravelRequest,
            userId,
            AuditActionType.BookingStatusChanged,
            $"{booking.Type} booking ({booking.BookingReference}) status changed from {previousStatus} to {newStatus}.");

        await bookings.SaveAsync();

        return ToDto(booking);
    }

    private static BookingDto ToDto(Booking b)
    {
        return new BookingDto(
            b.Id,
            b.TravelRequestId,
            b.Type,
            b.Provider,
            b.BookingReference,
            b.ActualCost,
            b.Status,
            b.BookingDate,
            $"{b.BookedByUser?.FirstName} {b.BookedByUser?.LastName}".Trim(),
            b.Notes);
    }

    private static AdminBookingDto ToAdminDto(Booking b)
    {
        return new AdminBookingDto(
            b.Id,
            b.TravelRequestId,
            $"{b.TravelRequest?.User?.FirstName} {b.TravelRequest?.User?.LastName}".Trim(),
            b.TravelRequest?.Department?.Name ?? "Unassigned",
            b.TravelRequest?.Destination ?? "",
            b.Type,
            b.Provider,
            b.BookingReference,
            b.ActualCost,
            b.TravelRequest?.Currency ?? "",
            b.Status,
            b.BookingDate,
            $"{b.BookedByUser?.FirstName} {b.BookedByUser?.LastName}".Trim(),
            b.Notes);
    }
}
internal static class BookingWorkflowRules
{
    private static int Rank(BookingStatus status)
    {
        return status switch
        {
            BookingStatus.Pending => 0,
            BookingStatus.Confirmed => 1,
            BookingStatus.Cancelled => 2,
            _ => 0
        };
    }

    public static bool CanTransition(
        BookingStatus current,
        BookingStatus next)
    {
        return Rank(next) > Rank(current);
    }
}

