using System.ComponentModel.DataAnnotations;

namespace TravelGallery.Models.Api;

public class MediaReorderDto
{
    [Required] public List<int> OrderedIds { get; set; } = new();
}
