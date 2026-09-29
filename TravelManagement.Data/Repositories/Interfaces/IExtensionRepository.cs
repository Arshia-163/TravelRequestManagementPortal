using TravelManagement.Data.Entities;

namespace TravelManagement.Data.Repositories.Interfaces;

public interface IExtensionRepository
{
    Task AddAsync(TripExtension extension);

    Task<IReadOnlyList<TripExtension>> GetForRequestAsync(int travelRequestId);

    Task SaveAsync();
}
