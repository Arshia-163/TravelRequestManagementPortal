using System.ComponentModel.DataAnnotations;

namespace TravelManagement.Services.Shared.DTOs;

public sealed class DepartmentInput
{
    [Required, StringLength(200)] 
    public string Name { get; set; } = string.Empty;
    [Range(typeof(decimal), "0.01", "999999999")] 
    public decimal TravelApprovalLimit { get; set; }
    public int? ManagerId { get; set; }
    public int? DepartmentHeadId { get; set; }
}
