using System.ComponentModel.DataAnnotations;

namespace TravelManagement.Services.Shared.DTOs;

public sealed class DecisionInput { 
    [StringLength(2000)] 
    public string? Reason { get; set; } 
}
