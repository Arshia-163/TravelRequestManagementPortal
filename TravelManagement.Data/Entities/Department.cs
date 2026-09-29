namespace TravelManagement.Data.Entities;


public class Department
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public int? ManagerId { get; set; }
    public ApplicationUser? Manager { get; set; }

    public int? DepartmentHeadId { get; set; }
    public ApplicationUser? DepartmentHead { get; set; }

    
    public decimal TravelApprovalLimit { get; set; }

    public ICollection<ApplicationUser> Employees { get; set; } = new List<ApplicationUser>();
    public ICollection<TravelRequest> TravelRequests { get; set; } = new List<TravelRequest>();
}
