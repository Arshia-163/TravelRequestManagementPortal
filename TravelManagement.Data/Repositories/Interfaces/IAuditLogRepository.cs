using TravelManagement.Data.Entities;

namespace TravelManagement.Data.Repositories.Interfaces;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog log);

    Task<IReadOnlyList<AuditLog>> GetForRequestAsync(int travelRequestId);

    Task SaveAsync();
}
