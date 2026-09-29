namespace TravelRequestManagement.Client.Components;

/// <summary>Static list of tab labels/routes for the Department Head area's header sub-nav. Presentational only.</summary>
public static class HeadTabs
{
    public static readonly IReadOnlyList<(string Text, string Href)> Items =
    [
        ("Pending", "/department-head"),
        ("Approved by me", "/department-head/approved"),
        ("Rejected by me", "/department-head/rejected"),
        ("All requests", "/department-head/all"),
    ];
}
