using Microsoft.EntityFrameworkCore;
using TravelManagement.Data.DbContext;
using TravelManagement.Data.Entities;
using TravelManagement.Data.Repositories.Interfaces;

namespace TravelManagement.Data.Repositories.Implementation;

public sealed class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext db;

    public UserRepository(ApplicationDbContext db)
    {
        this.db = db;
    }

    public async Task<int> CountAsync()
    {
        int count = await db.Users.CountAsync();

        return count;
    }

    public async Task<IReadOnlyList<ApplicationUser>> GetAllOrderedAsync()
    {
        IReadOnlyList<ApplicationUser> users = await db.Users
            .OrderBy(x => x.FirstName)
            .ThenBy(x => x.LastName)
            .ToListAsync();

        return users;
    }

    public async Task<IReadOnlyList<ApplicationUser>> GetRecentlyJoinedAsync(
        int count)
    {
        IReadOnlyList<ApplicationUser> users = await db.Users
            .OrderByDescending(x => x.CreatedAt)
            .Take(count)
            .ToListAsync();

        return users;
    }

    public async Task<int> CountWithEmployeeCodeAsync()
    {
        int count = await db.Users.CountAsync(
            x => x.EmployeeCode != null);

        return count;
    }

    public async Task<bool> HasWorkflowReferencesAsync(
        int userId)
    {
        bool hasTravelRequestReferences =
            await db.TravelRequests.AnyAsync(
                request => request.UserId == userId);

        if (hasTravelRequestReferences)
        {
            return true;
        }

        bool hasAuditLogReferences =
            await db.AuditLogs.AnyAsync(
                log => log.PerformedByUserId == userId);

        if (hasAuditLogReferences)
        {
            return true;
        }

        bool hasBookingReferences =
            await db.Bookings.AnyAsync(
                booking => booking.BookedByUserId == userId);

        return hasBookingReferences;
    }
}