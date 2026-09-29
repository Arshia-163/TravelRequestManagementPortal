namespace TravelRequestManagement.Client.Components;


public static class AdminTabs
{
    public static readonly IReadOnlyList<(string Text, string Href)> Items =
    [
        ("Dashboard", "/admin"),
        ("Departments", "/admin/departments"),
        ("Users", "/admin/users"),
        ("Approved Travel", "/admin/approved-travel"),
        ("Bookings", "/admin/bookings"),
        ("History", "/admin/history"),
    ];
}
