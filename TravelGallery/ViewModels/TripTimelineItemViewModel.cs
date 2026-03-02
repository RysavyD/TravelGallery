namespace TravelGallery.ViewModels;

public class TripTimelineItemViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string ShortDescription { get; set; } = string.Empty;
    public List<string> ThumbnailUrls { get; set; } = new();
    public int MediaCount { get; set; }
    public bool HasMoreMedia => MediaCount > ThumbnailUrls.Count;
}
