namespace TravelGallery.Models.Api;

public class MediaDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = "";
    public string MediaType { get; set; } = "";
    public string Caption { get; set; } = "";
    public int SortOrder { get; set; }
    public string Url { get; set; } = "";
    public string ThumbnailUrl { get; set; } = "";
    public DateTime? DateTaken { get; set; }
    public string? CameraModel { get; set; }
    public string? ExifSummary { get; set; }
}
