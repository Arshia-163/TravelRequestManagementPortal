namespace TravelRequestManagement.Client.Services;


public sealed class DashboardNotifier
{
   
    public event Action? UsersChanged;

    
    public event Action? TravelRequestsChanged;

    public void NotifyUsersChanged() => UsersChanged?.Invoke();

    public void NotifyTravelRequestsChanged() => TravelRequestsChanged?.Invoke();
}
