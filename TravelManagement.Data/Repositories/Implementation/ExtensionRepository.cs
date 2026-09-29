using Microsoft.EntityFrameworkCore;
using TravelManagement.Data.DbContext;
using TravelManagement.Data.Entities;
using TravelManagement.Data.Repositories.Interfaces;

namespace TravelManagement.Data.Repositories.Implementation;

public sealed class ExtensionRepository : IExtensionRepository
{
    private readonly ApplicationDbContext db;

    public ExtensionRepository(ApplicationDbContext db)
    {
        this.db = db;
    }

    public async Task AddAsync(
        TripExtension extension)
    {
        await db.TripExtensions.AddAsync(extension);
    }

    public async Task<IReadOnlyList<TripExtension>> GetForRequestAsync(
        int travelRequestId)
    {
        IReadOnlyList<TripExtension> extensions = await db.TripExtensions
            .Where(x => x.TravelRequestId == travelRequestId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return extensions;
    }

    public async Task SaveAsync()
    {
        await db.SaveChangesAsync();
    }
}
