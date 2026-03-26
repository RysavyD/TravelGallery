namespace TravelGallery.Models;

public enum MediaType { Image, Video }

public class Media
{
    public int Id { get; set; }
    public int TripId { get; set; }
    public Trip Trip { get; set; } = null!;
    public string FileName { get; set; } = string.Empty;
    public MediaType MediaType { get; set; }
    public string Caption { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // EXIF metadata
    public DateTime? DateTaken { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? CameraModel { get; set; }
    public string? ExifSummary { get; set; }

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
