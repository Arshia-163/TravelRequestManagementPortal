using Microsoft.EntityFrameworkCore;
using TravelManagement.Data.DbContext;
using TravelManagement.Data.Entities;
using TravelManagement.Data.Repositories.Interfaces;

namespace TravelManagement.Data.Repositories.Implementation;

public sealed class BookingRepository(ApplicationDbContext db) : IBookingRepository
{
    public Task<bool> ExistsForRequestAsync(int travelRequestId) =>
        db.Bookings.AnyAsync(x => x.TravelRequestId == travelRequestId);

    public async Task<IReadOnlyList<Booking>> GetForRequestAsync(int travelRequestId) =>
        await db.Bookings.Include(x => x.BookedByUser)
            .Where(x => x.TravelRequestId == travelRequestId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();

    public async Task<IReadOnlyList<Booking>> GetAllAsync() =>
        await db.Bookings
            .Include(x => x.BookedByUser)
            .Include(x => x.TravelRequest).ThenInclude(r => r.User)
            .Include(x => x.TravelRequest).ThenInclude(r => r.Department)
            .OrderByDescending(x => x.BookingDate)
            .ToListAsync();

    public Task<Booking?> GetByIdAsync(int bookingId) =>
        db.Bookings.Include(x => x.BookedByUser).Include(x => x.TravelRequest)
            .SingleOrDefaultAsync(x => x.Id == bookingId);

    public Task AddAsync(Booking booking) =>
        db.Bookings.AddAsync(booking).AsTask();

    public Task SaveAsync() => db.SaveChangesAsync();
}
