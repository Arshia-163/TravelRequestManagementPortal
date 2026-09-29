using System.ComponentModel.DataAnnotations;

namespace TravelManagement.Services.Shared.DTOs;

public sealed class ExtensionInput
{
    [Required] public DateTime NewEndDate { get; set; }
    [Range(typeof(decimal), "0.01", "999999999")] public decimal AdditionalEstimatedCost { get; set; }
    [Required, StringLength(1000)] public string Reason { get; set; } = string.Empty;
}
