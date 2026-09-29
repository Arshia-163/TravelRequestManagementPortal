using Microsoft.AspNetCore.Identity;
using TravelManagement.Services.Shared.Enums;

namespace TravelManagement.Data.Entities;

public class ApplicationUser : IdentityUser<int>
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    
    public string? EmployeeCode { get; set; }

    public Gender Gender { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int? DepartmentId { get; set; }

    public Department? Department { get; set; }

    
    public ICollection<Department> ManagedDepartments { get; set; } = new List<Department>();

   
    public ICollection<Department> HeadedDepartments { get; set; } = new List<Department>();

    public ICollection<TravelRequest> TravelRequests { get; set; } = new List<TravelRequest>();

    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    public ICollection<Booking> BookingsMade { get; set; } = new List<Booking>();
}
