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

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
