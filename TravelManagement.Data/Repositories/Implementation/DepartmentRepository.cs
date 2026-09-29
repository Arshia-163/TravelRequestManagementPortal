using Microsoft.EntityFrameworkCore;

using TravelManagement.Data.DbContext;
using TravelManagement.Data.Entities;
using TravelManagement.Data.Repositories.Interfaces;

namespace TravelManagement.Data.Repositories.Implementation;

public sealed class DepartmentRepository(ApplicationDbContext db)
    : IDepartmentRepository
{
    public Task<Department?> GetAsync(
        int id)
    {
        return db.Departments
            .Include(x => x.Manager)
            .Include(x => x.DepartmentHead)
            .SingleOrDefaultAsync(x => x.Id == id);
    }

    public async Task<IReadOnlyList<Department>> GetAllAsync()
    {
        return await db.Departments
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    public Task AddAsync(
        Department department)
    {
        return db.Departments
            .AddAsync(department)
            .AsTask();
    }

    public void Delete(Department department)
    {
        db.Departments.Remove(department);
    }

    public Task SaveAsync()
    {
        return db.SaveChangesAsync();
    }

    public async Task<bool> HasDependenciesAsync(
        int departmentId)
    {
        return await db.Users
            .AnyAsync(
                user => user.DepartmentId == departmentId)
            || await db.TravelRequests
                .AnyAsync(
                    request => request.DepartmentId == departmentId);
    }

    public Task<int> CountAsync()
    {
        return db.Departments
            .CountAsync();
    }

    public Task<int> CountDistinctManagersAsync()
    {
        return db.Departments
            .Where(x => x.ManagerId != null)
            .Select(x => x.ManagerId)
            .Distinct()
            .CountAsync();
    }

    public Task<bool> NameExistsAsync(
        string name,
        int? excludeDepartmentId = null)
    {
        return db.Departments
            .AnyAsync(
                d => d.Name == name
                    && d.Id != excludeDepartmentId);
    }

    public Task<bool> IsManagerOfAnyAsync(
        int userId)
    {
        return db.Departments
            .AnyAsync(
                d => d.ManagerId == userId);
    }

    public Task<bool> IsDepartmentHeadOfAnyAsync(
        int userId)
    {
        return db.Departments
            .AnyAsync(
                d => d.DepartmentHeadId == userId);
    }
}
