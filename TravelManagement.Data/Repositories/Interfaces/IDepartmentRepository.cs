using TravelManagement.Data.Entities;
namespace TravelManagement.Data.Repositories.Interfaces;

public interface IDepartmentRepository
{
    Task<Department?> GetAsync(int id);
    Task<IReadOnlyList<Department>> GetAllAsync();
    Task AddAsync(Department department);
    void Delete(Department department);
    Task SaveAsync();

    Task<bool> HasDependenciesAsync(int departmentId);

    Task<int> CountAsync();

    Task<int> CountDistinctManagersAsync();

    Task<bool> NameExistsAsync(string name, int? excludeDepartmentId = null);

    Task<bool> IsManagerOfAnyAsync(int userId);

    Task<bool> IsDepartmentHeadOfAnyAsync(int userId);
}
