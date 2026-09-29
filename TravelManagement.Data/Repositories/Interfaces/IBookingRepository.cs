using TravelManagement.Data.Entities;

namespace TravelManagement.Data.Repositories.Interfaces;

public interface IBookingRepository
{
    Task<bool> ExistsForRequestAsync(int travelRequestId);

    Task<IReadOnlyList<Booking>> GetForRequestAsync(int travelRequestId);

    Task<IReadOnlyList<Booking>> GetAllAsync();

    Task<Booking?> GetByIdAsync(int bookingId);

    Task AddAsync(Booking booking);

    Task SaveAsync();
}
