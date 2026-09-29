using TravelManagement.Data.Entities;

namespace TravelManagement.Data.Repositories.Interfaces;

public interface IUserRepository
{
    Task<int> CountAsync();

    Task<IReadOnlyList<ApplicationUser>> GetAllOrderedAsync();

    Task<IReadOnlyList<ApplicationUser>> GetRecentlyJoinedAsync(int count);

    Task<int> CountWithEmployeeCodeAsync();

    Task<bool> HasWorkflowReferencesAsync(int userId);
}
