using System.ComponentModel.DataAnnotations;

namespace TravelManagement.Services.Shared.DTOs;

public sealed class BookingInput { [Required, StringLength(100)] public string BookingReference { get; set; } = string.Empty; public string? Notes { get; set; } }
