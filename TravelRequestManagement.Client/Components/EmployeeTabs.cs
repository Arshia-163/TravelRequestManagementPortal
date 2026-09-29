namespace TravelRequestManagement.Client.Components;


public static class EmployeeTabs
{
    public static readonly IReadOnlyList<(string Text, string Href)> Items =
    [
      ("Dashboard", "/dashboard"),
        ("My Requests", "/"),
    ];
}
