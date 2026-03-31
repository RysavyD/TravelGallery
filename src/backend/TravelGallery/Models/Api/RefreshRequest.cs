using System.ComponentModel.DataAnnotations;

namespace TravelGallery.Models.Api;

public class RefreshRequest
{
    [Required] public string RefreshToken { get; set; } = "";
}
