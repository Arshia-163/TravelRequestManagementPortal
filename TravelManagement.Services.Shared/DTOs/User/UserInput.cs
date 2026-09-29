using System.ComponentModel.DataAnnotations;
using TravelManagement.Services.Shared.Enums;

namespace TravelManagement.Services.Shared.DTOs;

public sealed class UserInput
{
    [Required] 
    public string FirstName { get; set; } = string.Empty; 

    [Required] 
    public string LastName { get; set; } = string.Empty;
    [Required, EmailAddress] 
    public string Email { get; set; } = string.Empty;

    [Required, RegularExpression(@"^\d{10}$", ErrorMessage = "Phone must be exactly 10 digits.")] 
    public string Phone { get; set; } = string.Empty;
    public Gender Gender { get; set; } public int? DepartmentId { get; set; } public List<string> Roles { get; set; } = [];

    [StringLength(100)] 
    public string? Password { get; set; }
}
