namespace TravelRequestManagement.Client.Components;


public static class ManagerTabs
{
    public static readonly IReadOnlyList<(string Text, string Href)> Items =
    [

        ("Pending", "/manager"),
        ("Approved by me", "/manager/approved"),
        ("Rejected by me", "/manager/rejected"),
        ("All requests", "/manager/all"),
    ];
}
