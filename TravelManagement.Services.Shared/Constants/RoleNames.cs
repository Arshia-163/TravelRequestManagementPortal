namespace TravelManagement.Services.Shared.Constants;


public static class RoleNames
{
    public const string Employee = "Employee";
    public const string Manager = "Manager";
    public const string DepartmentHead = "DepartmentHead";
    public const string TravelAdmin = "TravelAdmin";

    public static readonly string[] CanCreateTravelRequestRoles =
    {
        Employee, Manager, DepartmentHead
    };

    public static readonly string[] All =
    {
        Employee, Manager, DepartmentHead, TravelAdmin
    };
}

public static class AppRoles
{
    public const string Employee = RoleNames.Employee;
    public const string Manager = RoleNames.Manager;
    public const string DepartmentHead = RoleNames.DepartmentHead;
    public const string TravelAdmin = RoleNames.TravelAdmin;
    public const string TravelAdminPolicy = "TravelAdmin";
    public const string ManagerPolicy = "Manager";
    public const string DepartmentHeadPolicy = "DepartmentHead";
    public const string CanCreateRequestPolicy = "CanCreateRequest";
}
