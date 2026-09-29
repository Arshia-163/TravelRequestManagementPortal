using System.ComponentModel.DataAnnotations;
using TravelManagement.Services.Shared.Enums;

namespace TravelManagement.Services.Shared.DTOs;

public sealed class TravelRequestInput
{
    public TravelType TravelType { get; set; } =
        TravelType.Domestic;

    [Required]
    [StringLength(300)]
    public string Source { get; set; } =
        string.Empty;

    [Required]
    [StringLength(300)]
    public string Destination { get; set; } =
        string.Empty;

    [Required]
    [StringLength(2000)]
    public string BusinessJustification { get; set; } =
        string.Empty;

    [Required]
    public DateTime StartDate { get; set; } =
        DateTime.Today;

    [Required]
    public DateTime EndDate { get; set; } =
        DateTime.Today.AddDays(1);

    [Range(typeof(decimal), "0.01", "999999999")]
    public decimal EstimatedCost { get; set; }

    [Required]
    [StringLength(10)]
    public string Currency { get; set; } =
        "INR";

   
    public bool AccommodationRequired { get; set; }

    [StringLength(300)]
    public string? HotelName { get; set; }

    [StringLength(300)]
    public string? HotelCity { get; set; }

    public DateTime? CheckInDate { get; set; }

    public DateTime? CheckOutDate { get; set; }
}