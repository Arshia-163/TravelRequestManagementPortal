using Microsoft.EntityFrameworkCore;
using TravelManagement.Data.DbContext;
using TravelManagement.Data.Entities;
using TravelManagement.Data.Repositories.Interfaces;

namespace TravelManagement.Data.Repositories.Implementation;

public sealed class AuditLogRepository(ApplicationDbContext db) : IAuditLogRepository
{
    public Task AddAsync(AuditLog log) =>
        db.AuditLogs.AddAsync(log).AsTask();

    public async Task<IReadOnlyList<AuditLog>> GetForRequestAsync(int travelRequestId) =>
        await db.AuditLogs
            .Include(x => x.PerformedByUser)
            .Where(x => x.TravelRequestId == travelRequestId)
            .OrderBy(x => x.Timestamp)
            .ToListAsync();

    public Task SaveAsync() => db.SaveChangesAsync();
}
