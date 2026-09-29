using System.ComponentModel.DataAnnotations;

namespace TravelManagement.Services.Shared.DTOs;

public sealed class CancelInput
{
    [Required]
    [StringLength(1000)]
    public string Reason { get; set; } = string.Empty;
}