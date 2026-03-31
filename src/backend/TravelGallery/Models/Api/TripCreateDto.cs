using System.ComponentModel.DataAnnotations;

namespace TravelGallery.Models.Api;

public class TripCreateDto
{
    [Required, MaxLength(200)] public string Title { get; set; } = "";
    [Required] public DateTime Date { get; set; }
    public string? Description { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}
