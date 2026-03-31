namespace TravelGallery.Models;

public class Comment
{
    public int Id { get; set; }
    public int MediaId { get; set; }
    public Media Media { get; set; } = null!;
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
