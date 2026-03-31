namespace TravelGallery.Models.Api;

public class TripDetailDto
{
    public TripDto Trip { get; set; } = null!;
    public List<MediaDto> Media { get; set; } = new();
}
