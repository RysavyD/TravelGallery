namespace TravelGallery.Models.Api;

public class TripDto
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public DateTime Date { get; set; }
    public string Description { get; set; } = "";
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public int PhotoCount { get; set; }
    public string? CoverPhotoUrl { get; set; }
}
