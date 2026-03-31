namespace TravelGallery.Models;

public class Trip
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public ICollection<Media> Media { get; set; } = new List<Media>();
    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
    public ICollection<TravelGroup> Groups { get; set; } = new List<TravelGroup>();
}
