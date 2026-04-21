namespace TravelGallery.ViewModels;

public class TripTimelineItemViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string ShortDescription { get; set; } = string.Empty;
    public List<TripThumbnail> Thumbnails { get; set; } = new();
    public int MediaCount { get; set; }
    public bool HasMoreMedia => MediaCount > Thumbnails.Count;
    public List<(string Name, string Slug)> Tags { get; set; } = new();
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}

public record TripThumbnail(int MediaId, string Url);
