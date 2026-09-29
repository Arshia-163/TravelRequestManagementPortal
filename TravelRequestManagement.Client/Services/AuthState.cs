using TravelManagement.Services.Shared.DTOs;

namespace TravelRequestManagement.Client.Services;

public sealed class AuthState(ITravelApi api)
{
    public CurrentUserDto? Me { get; private set; }
    public bool Loaded { get; private set; }

 
    public event Action? Changed;

   
    public async Task<bool> EnsureLoadedAsync()
    {
        if (Loaded) return Me is not null;
        await RefreshAsync();
        return Me is not null;
    }

 
    public async Task RefreshAsync()
    {
        try { Me = await api.MeAsync(); }
        catch { Me = null; }
        Loaded = true;
        Changed?.Invoke();
    }

    public void SignedOut()
    {
        Me = null;
        Loaded = true;
        Changed?.Invoke();
    }
}
